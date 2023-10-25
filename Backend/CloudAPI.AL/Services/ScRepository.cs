using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using CloudAPI.AL.Models.Sc;
using Serilog;
using SharedLibrary;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;

namespace CloudAPI.AL.Services;

public class ScRepository {
    ConfigurationModel _config;
    ISystemIOAbstraction _io;
    IAlbumInfoProvider _ai;
    ILogger _logger;
    MediaProcessor _media;
    ImageProcessor _ip;
    ILogDbContext _logDb;

    public ScRepository(ConfigurationModel config, ISystemIOAbstraction io, IAlbumInfoProvider ai, ILogger logger, MediaProcessor media, ImageProcessor ip, ILogDbContext logDb) {
        _config = config;
        _io = io;
        _ai = ai;
        _logger = logger;
        _media = media;
        _ip = ip;
        _logDb = logDb;
    }

    #region Sc Db
    ScDbModel _scDb;

    private void InitializeScDb() {
        if(_scDb != null) return;
        _scDb = _io.IsFileExists(_config.ScFullAlbumDbPath)
            ? _io.DeserializeJsonSync<ScDbModel>(_config.ScFullAlbumDbPath)
            : new ScDbModel();
    }

    private ScMetadataModel GetScAlbumMetadata(string albumPath) {
        InitializeScDb();

        var scMetadata = _scDb.ScMetadatas.FirstOrDefault(m => m.Path == albumPath);
        if(scMetadata != null) return scMetadata;

        var newScMetadata = new ScMetadataModel {
            Path = albumPath,
            LastPageIndex = 0
        };
        _scDb.ScMetadatas.Add(newScMetadata);
        SaveChanges();

        return newScMetadata;
    }

    private void SaveChanges() {
        InitializeScDb();
        _io.SerializeToJson(_config.ScFullAlbumDbPath, _scDb);
    }
    #endregion

    #region Query
    public IEnumerable<ScAlbumVM> GetAlbumVMs(string libRelPath) {
        FileInfoModel GetCoverInfo(string path) {
            var finalPath = path ?? _config.ScFullDefaultThumbnailPath;

            var fi = new FileInfo(finalPath);
            var libRelPath = Path.GetRelativePath(_config.ScLibraryPath, finalPath);

            return new FileInfoModel {
                Extension = fi.Extension,
                Name = fi.Name,
                Size = fi.Length,
                UncPathEncoded = libRelPath
            };
        }

        try {
            var fullPath = Path.Combine(_config.ScLibraryPath, libRelPath);
            bool isSubdirsAlbum = new Func<bool>(() => {
                var count = string.IsNullOrEmpty(libRelPath) ? 0 : libRelPath.Split(Path.DirectorySeparatorChar).Count();

                return count == _config.ScLibraryDepth;
            })();

            var subDirs = _io.GetDirectories(fullPath);

            var result = subDirs
                .Where(e => e != _config.ScFullExtraInfoPath)
                .Select(e => {
                    var libRelAlbumPath = Path.GetRelativePath(_config.ScLibraryPath, e);
                    var filePaths = _io.GetSuitableFilePathsWithNaturalSort(e, _ai.SuitableFileFormats, 1);
                    return new ScAlbumVM {
                        Name = Path.GetFileName(e),
                        LibRelPath = libRelAlbumPath,
                        LastPageIndex = GetScAlbumMetadata(libRelAlbumPath).LastPageIndex,
                        PageCount = isSubdirsAlbum ? filePaths.Count : 0,
                        CoverInfo = GetCoverInfo(filePaths.FirstOrDefault())
                    };
                });

            return result;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.GetAlbumVMs{Environment.NewLine}" +
                $"Params=[{libRelPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public AlbumFsNodeInfo GetAlbumFsNodeInfo(string libRelAlbumPath, bool includeDetail = false, bool includeDimension = false) {
        try {
            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath);

            var subDirs = _io.GetDirectories(fullAlbumPath)
                .OrderByAlphaNumeric(d => Path.GetRelativePath(fullAlbumPath, d))
                .ToList();

            var subDirNodes = subDirs
                .Select(d => {
                    var dirInfo = new DirectoryInfo(d);

                    return new FsNode {
                        NodeType = NodeType.Folder,
                        AlRelPath = Path.GetRelativePath(fullAlbumPath, d),
                        DirInfo = new() {
                            Name = dirInfo.Name,
                            Tier = 0,
                        }
                    };
                }).ToList();

            var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(fullAlbumPath, _ai.SuitableFileFormats, 1);
            var allFileNodes = allFilePaths.Select(f => {
                return new FsNode {
                    NodeType = NodeType.File,
                    AlRelPath = Path.GetRelativePath(fullAlbumPath, f),
                };
            }).ToList();

            bool trueIncludeDimension = includeDimension;

            Parallel.For(0, allFileNodes.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i, state) => {
                allFileNodes[i].FileInfo = _media.GenerateFileInfo(fullAlbumPath, allFilePaths[i], includeDetail, trueIncludeDimension);
            });

            var rootFileNodes = new List<FsNode>();
            allFileNodes.ForEach(fs => {
                //check if fs.AlRelPath is not located in a directory
                for(int i = 0; i < subDirNodes.Count; i++) {
                    if(fs.AlRelPath.StartsWith(subDirNodes[i].AlRelPath)) {
                        subDirNodes[i].DirInfo.Childs.Add(fs);
                        return;
                    }
                }
                rootFileNodes.Add(fs);
            });

            var fsNodes = rootFileNodes.Concat(subDirNodes).ToList();

            return new AlbumFsNodeInfo {
                FsNodes = fsNodes,
                Title = Path.GetFileName(fullAlbumPath),
                Orientation = Constants.Orientation.Portrait
            };
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.GetAlbumFsNodeInfo{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }
    #endregion

    #region Command
    public string DeleteFile(string libRelAlbumPath, string alRelPath) {
        try {
            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath);
            var targetPath = Path.Combine(fullAlbumPath, alRelPath);

            if(!_io.IsPathExist(targetPath)) return "Path does not exist";

            _io.DeleteFile(targetPath);
            DeleteAllPageCache(targetPath);

            return "Success";
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.DeleteFile{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath},{alRelPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public int DeleteAlbumChapter(string libRelAlbumPath, string subDir) {
        try {
            var chapterPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath, subDir);

            _io.DeleteDirectory(chapterPath);

            int newPageCount = CountAlbumPages(libRelAlbumPath);
            return newPageCount;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.DeleteAlbumChapter{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath},{subDir}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public int CountAlbumPages(string libRelAlbumPath) {
        try {
            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath);

            var suitableFilePaths = _io.GetSuitableFilePaths(fullAlbumPath, _ai.SuitableFileFormats, 1);

            return suitableFilePaths.Count;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.CountAlbumPages{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void UpdateAlbumOuterValue(string albumPath, int lastPageIndex) {
        try {
            var scMetadata = GetScAlbumMetadata(albumPath);

            scMetadata.LastPageIndex = lastPageIndex;

            SaveChanges();
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.CountAlbumPages{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void DeleteAlbumCache(string albumPath) {
        try {
            var sizeFolders = _io.GetDirectories(_config.ScFullCachePath);
            foreach(var sizeFolder in sizeFolders) {
                var fullALbumCachePath = Path.Combine(sizeFolder, albumPath);
                _io.DeleteDirectory(fullALbumCachePath);
            }
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.DeleteAlbumCache{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string MoveFile(FileMoveParamModel param) {
        try {
            var srcLibRelPath = Path.Combine(param.Src.AlbumPath, param.Src.AlRelPath);
            var dstLibRelPath = Path.Combine(param.Dst.AlbumPath, param.Dst.AlRelPath);

            var srcFullPath = Path.Combine(_config.ScLibraryPath, srcLibRelPath);
            var dstFullPath = Path.Combine(_config.ScLibraryPath, dstLibRelPath);

            if(_io.IsDirectoryExist(srcFullPath)) {
                if(_io.IsDirectoryExist(dstFullPath)) {
                    throw new Exception("Unable to overwrite folder");
                }
                _io.MoveDirectory(srcFullPath, dstFullPath);
                return (JsonSerializer.ToJsonString(new {
                    message = "Success"
                }));
            }

            if(!_io.IsFileExists(srcFullPath)) {
                throw new Exception("Source file does not exist");
            }
            var targetExist = _io.IsFileExists(dstFullPath);
            if(!param.Overwrite && targetExist) {
                var srcInfo = new FileInfo(srcFullPath);
                var dstInfo = new FileInfo(dstFullPath);

                return (JsonSerializer.ToJsonString(new {
                    message = "Destination file already exist",
                    srcInfo = new {
                        name = srcInfo.Name,
                        size = srcInfo.Length,
                        createdDate = srcInfo.CreationTime
                    },
                    dstInfo = new {
                        name = dstInfo.Name,
                        size = dstInfo.Length,
                        createdDate = dstInfo.CreationTime
                    }
                }));//WARNING Magic string
            }
            else if(param.Overwrite && targetExist) {
                _io.DeleteFile(dstFullPath);
            }
            _io.MoveFile(srcFullPath, dstFullPath);

            DeleteAllPageCache(srcFullPath);
            DeleteAllPageCache(dstFullPath);

            return (JsonSerializer.ToJsonString(new {
                message = "Success"
            }));
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.DeleteAlbumCache{Environment.NewLine}" +
                $"Params=[{JsonSerializer.Serialize(param)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    private void DeleteAllPageCache(string libRelOriginalPagePath) {
        var sizeDirs = _io.GetDirectories(_config.FullPageCachePath);

        var fullCachePaths = sizeDirs.Select(e => Path.Combine(e, $"{libRelOriginalPagePath}.jpg")).ToList();

        foreach(var path in fullCachePaths) {
            _io.DeleteFileOrDirectory(path);
        }
    }
    #endregion

    #region Operations
    public List<PathCorrectionModel> GetCorrectablePaths() {
        var allDirs = _io.GetDirectories(_config.ScLibraryPath, SearchOption.AllDirectories);
        int validPathCount = _config.ScLibraryDepth + 1;

        var libRelCorrectablePaths = allDirs
            .Where(a => !a.StartsWith(_config.ScFullExtraInfoPath)
                && Path.GetRelativePath(_config.ScLibraryPath, a).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length == validPathCount)
            .Select(a => Path.GetRelativePath(_config.ScLibraryPath, a))
            .ToList();

        var correctionLogs = _logDb.GetCorrectionLogs(libRelCorrectablePaths);

        var pathCorrectionModels = libRelCorrectablePaths
            .Select(a => {
                var log = correctionLogs.FirstOrDefault(b => b.Path == a);

                return new PathCorrectionModel {
                    LibRelPath = a,
                    LastCorrectionDate = log?.LastCorrectionDate,
                    CorrectablePageCount = (log?.CorrectablePageCount).GetValueOrDefault()
                };
            }).ToList();

        return pathCorrectionModels;
    }

    public List<PathCorrectionModel> FullScanCorrectiblePaths(int thread, int upscaleTarget) {
        var correctablePaths = GetCorrectablePaths();

        foreach(var path in correctablePaths) {
            var correctablePageCount = (GetCorrectablePages(path.LibRelPath, thread, upscaleTarget)).Count;

            _logDb.UpdateCorrectionLog(path.LibRelPath, null, correctablePageCount);

            path.CorrectablePageCount = correctablePageCount;
        }

        return correctablePaths;
    }

    private FileCorrectionModel GetFileCorrectionModel(FileInfo fileInfo, string fullAlbumPath) {
        using(var fileStream = _io.GetStream(fileInfo.FullName))
        using(var img = Image.FromStream(fileStream, false, false)) {
            return new FileCorrectionModel {
                AlRelPath = Path.GetRelativePath(fullAlbumPath, fileInfo.FullName),
                Extension = fileInfo.Extension,
                Byte = fileInfo.Length,
                ModifiedDate = fileInfo.LastWriteTime,
                Height = img.Height,
                Width = img.Width,
            };
        }
    }

    public List<FileCorrectionModel> GetCorrectablePages(string libRelAlbumPath, int thread, int upscaleTarget) {
        int trueThread = Math.Clamp(thread, 1, 5);
        //This method becomes unstable and produces faulty images if performing upscaling/compression using 6 threads
        //Tested with 6 Core Ryzen 5 5600 6-Core CPU

        try {
            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath);

            var filePaths = _io.GetSuitableFilePaths(fullAlbumPath, _ai.CompressableImageFormats, 2);

            var allFileInfos = new FileInfo[filePaths.Count];
            Parallel.For(0, filePaths.Count, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
                var filePath = filePaths[i];
                try {
                    allFileInfos[i] = new FileInfo(filePath);
                }
                catch(Exception ex) {
                    allFileInfos[i] = null;
                    _logger.Error($"GetCorrectableFiles | Parallel.For 1 | {filePath} | {ex.Message}");
                }
            });

            var correctionLog = _logDb.GetCorrectionLog(libRelAlbumPath);
            var lastCorrectionDate = (correctionLog?.LastCorrectionDate).GetValueOrDefault();

            var fileInfoAboveLastDate = allFileInfos.Where(a => a != null && a.LastWriteTime > lastCorrectionDate).ToList();

            var correctionModelAboveLastDate = new FileCorrectionModel[fileInfoAboveLastDate.Count];
            Parallel.For(0, fileInfoAboveLastDate.Count, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
                var fileInfo = fileInfoAboveLastDate[i];
                try {
                    var newFcm = GetFileCorrectionModel(fileInfo, fullAlbumPath);

                    float? multiplier = ImageHelper.DetermineUpscaleMultiplier(newFcm, upscaleTarget);
                    if(multiplier.HasValue) {
                        newFcm.CorrectionType = FileCorrectionType.Upscale;
                        newFcm.UpscaleMultiplier = multiplier;
                        newFcm.Compression = ImageHelper.DetermineCompressionCondition(
                            (int)(multiplier.Value * newFcm.Height), 
                            (int)(multiplier.Value * newFcm.Width), 
                            newFcm.Extension == Constants.Extension.Png
                        );
                    }
                    else if(ImageHelper.IsLargeEnoughForCompression(newFcm)) {
                        newFcm.CorrectionType = FileCorrectionType.Compress;
                        newFcm.Compression = ImageHelper.DetermineCompressionCondition(
                            newFcm.Height, 
                            newFcm.Width, 
                            newFcm.Extension == Constants.Extension.Png
                        );
                    }

                    correctionModelAboveLastDate[i] = newFcm;
                }
                catch(Exception ex) {
                    correctionModelAboveLastDate[i] = null;
                    _logger.Error($"GetFileViewModels | Parallel.For 2 | {fileInfo.FullName} | {ex.Message}");
                }
            });

            var fileToCorrect = correctionModelAboveLastDate.Where(a => a?.CorrectionType != null).ToList();

            return fileToCorrect;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.GetCorrectableFiles{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath}{thread}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public FileCorrectionReportModel[] CorrectPages(CorrectPageParam param) {
        var trueThread = Math.Clamp(param.Thread, 1, 5);
        //This method becomes unstable and produces faulty images if performing upscaling/compression using 6 threads
        //Tested with 6 Core Ryzen 5 5600 6-Core CPU

        try {
            var sw = new Stopwatch();
            sw.Start();

            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, param.LibRelPath);

            var scAlbumCachePath = Path.Combine(_config.ScFullCachePath, param.LibRelPath);

            var fullSTempPath = Path.Combine(scAlbumCachePath, $"[[Staging");
            Directory.CreateDirectory(fullSTempPath);
            var fullUTempPath = Path.Combine(scAlbumCachePath, $"[[Upscale");
            Directory.CreateDirectory(fullUTempPath);
            var fullCTempPath = Path.Combine(scAlbumCachePath, $"[[Compressed");
            Directory.CreateDirectory(fullCTempPath);

            var fileCount = param.FileToCorrectList.Count;

            var fileList = new Func<FileCorrectionModel[]>(() => {
                int cStart = param.FileToCorrectList.FindIndex(a => a.CorrectionType == FileCorrectionType.Compress);
                if(cStart == fileCount - 1 || cStart == -1)
                    return param.FileToCorrectList.ToArray();

                int uStart = 0;
                int cCount = fileCount - cStart;
                int uCount = fileCount - cCount;

                var sortedArr = new FileCorrectionModel[fileCount];
                for(int i = 0; i < fileCount; i++) {
                    if(i % 2 == 0) {
                        if(uStart < uCount) {
                            sortedArr[i] = param.FileToCorrectList[uStart];
                            uStart++;
                        }
                        else {
                            sortedArr[i] = param.FileToCorrectList[cStart];
                            cStart++;
                        }
                    }
                    else {
                        if(cStart < fileCount) {
                            sortedArr[i] = param.FileToCorrectList[cStart];
                            cStart++;
                        }
                        else {
                            sortedArr[i] = param.FileToCorrectList[uStart];
                            uStart++;
                        }
                    }
                }

                return sortedArr;
            })();

            var report = new FileCorrectionReportModel[fileCount];

            int[] possibleUpscaleMultipliers = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(param.UpscalerType) 
                ? new int[] { 2, 4, 8 } 
                : new int[] { 4, 8 };

            Func<string, string, int, UpscalerType, string> upscaleMethod = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(param.UpscalerType)
                ? _ip.UpscaleImageWaifu2x
                : UpscalerType.SrganD2fkJpeg == param.UpscalerType
                ? _ip.UpscaleImageRealSr
                : _ip.UpscaleImageRealEsrGan;

            Parallel.For(0, fileCount, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
                var messageSb = new StringBuilder();
                var src = fileList[i];

                try {
                    var stagingName = $"{Guid.NewGuid()}{src.Extension}";

                    var fullOriPath = Path.Combine(fullAlbumPath, src.AlRelPath);
                    var fullUpsPath = Path.Combine(fullUTempPath, stagingName);
                    var fullComPath = Path.Combine(fullCTempPath, stagingName);

                    if(src.CorrectionType == FileCorrectionType.Upscale) {
                        var fullStaPath = Path.Combine(fullSTempPath, stagingName);

                        _io.CopyFile(fullOriPath, fullStaPath);

                        var multiplier = possibleUpscaleMultipliers
                            .Where(num => num >= src.UpscaleMultiplier.GetValueOrDefault())
                            .DefaultIfEmpty(possibleUpscaleMultipliers.Max())
                            .Min();

                        var outputTxt = upscaleMethod(fullStaPath, fullUpsPath, multiplier, param.UpscalerType); //upscaleFile.UpscaleMultiplier.Value);

                        messageSb.Append("Upscale Finished");
                    }

                    var fullCompSrcPath = new Func<string>(() => {
                        if(src.CorrectionType == FileCorrectionType.Upscale) {
                            if(!_io.IsFileExists(fullUpsPath)) {
                                throw new Exception("Image at upscaled path does not exist");
                            }

                            if(new FileInfo(fullUpsPath).Length < (src.Byte / 3)) {
                                throw new Exception("Upscaled image possibly broken");
                            }

                            return fullUpsPath;
                        }
                        else {
                            return fullOriPath;
                        }
                    })();
                    
                    _ip.CompressImage(fullCompSrcPath, fullComPath, src.Compression.Quality, new Size(src.Compression.Width, src.Compression.Height), SupportedMimeType.ORIGINAL);

                    messageSb.Append(" | Compress Finished");

                    _io.CopyFile(fullComPath, fullOriPath, true);

                    report[i] = new() {
                        AlRelPath = src.AlRelPath,
                        Success = true
                    };
                }
                catch(Exception e) {
                    report[i] = new() {
                        AlRelPath = src.AlRelPath,
                        Success = false,
                        Message = messageSb.Append(" | " + e.Message).ToString()
                    };
                }
            });

            _logDb.UpdateCorrectionLog(param.LibRelPath, DateTime.Now, 0);

            _logger.Information($"Corrected {fileCount} pages in {sw.Elapsed.TotalSeconds} secs");

            Parallel.For(0, report.Length, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
                var src = report[i];

                if(src.Success) {
                    var cm = GetFileCorrectionModel(new FileInfo(Path.Combine(fullAlbumPath, src.AlRelPath)), fullAlbumPath);

                    src.Height = cm.Height;
                    src.Width = cm.Width;
                    src.Byte = cm.Byte;
                    src.BytesPer100Pixel = cm.BytesPer100Pixel;
                }
            });

            return report;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.CorrectPages{Environment.NewLine}" +
                $"Params=[{JsonSerializer.Serialize(param)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }
    #endregion

    #region LEGACY
    //public FileCorrectionReportModel[] CorrectPages(CorrectPageParam param) {
    //    try {
    //        var fullAlbumPath = Path.Combine(_config.ScLibraryPath, param.LibRelPath);

    //        var fileList = param.FileToCorrectList;
    //        var trueThread = Math.Clamp(param.Thread, 1, 6);

    //        #region Upscale
    //        var fullUTempPath = Path.Combine(fullAlbumPath, $"[[U{Guid.NewGuid().ToString()}");
    //        Directory.CreateDirectory(fullUTempPath);
    //        //var fullU2TempPath = Path.Combine(fullAlbumPath, $"[[U2");
    //        //Directory.CreateDirectory(fullU2TempPath);
    //        var toUpscaleList = fileList.Where(a => a.CorrectionType == FileCorrectionType.Upscale).ToList();

    //        var uDirNames = toUpscaleList.Select(a => Path.GetDirectoryName(a.AlRelPath)).Distinct().ToList();
    //        uDirNames.Distinct()
    //            .Where(dir => !string.IsNullOrEmpty(dir))
    //            .ToList()
    //            .ForEach(dir => {
    //                _io.CreateDirectory(Path.Combine(fullUTempPath, dir));
    //            });

    //        var sw = new Stopwatch();
    //        sw.Start();

    //        //var failedUpscaleReport = new List<FileCorrectionReportModel>();
    //        //foreach(var upscaleFile in toUpscaleList) {
    //        //    try {
    //        //        var fullSrcPath = Path.Combine(fullAlbumPath, upscaleFile.AlRelPath);
    //        //        var fullDstPath = Path.Combine(fullUTempPath, upscaleFile.AlRelPath);
    //        //        //var fullDstPath2 = Path.Combine(fullU2TempPath, upscaleFile.AlRelPath);

    //        //        //any multiplier other than 4 will result in scuffed image
    //        //        _ip.UpscaleImageX4Plus(fullSrcPath, fullDstPath, 4); //upscaleFile.UpscaleMultiplier.Value);
    //        //    }
    //        //    catch(Exception e) {
    //        //        failedUpscaleReport.Add(new() {
    //        //            AlRelPath = upscaleFile.AlRelPath,
    //        //            Success = false,
    //        //            Message = e.Message
    //        //        });
    //        //    }
    //        //}

    //        var failedUpscaleReportArr = new FileCorrectionReportModel[toUpscaleList.Count];

    //        Parallel.For(0, toUpscaleList.Count, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
    //            try {
    //                var fullSrcPath = Path.Combine(fullAlbumPath, toUpscaleList[i].AlRelPath);
    //                var fullDstPath = Path.Combine(fullUTempPath, toUpscaleList[i].AlRelPath);

    //                //any multiplier other than 4 will result in scuffed image
    //                _ip.UpscaleImageX4Plus(fullSrcPath, fullDstPath, 4); //upscaleFile.UpscaleMultiplier.Value);
    //            }
    //            catch(Exception e) {
    //                failedUpscaleReportArr[i] = new() {
    //                    AlRelPath = toUpscaleList[i].AlRelPath,
    //                    Success = false,
    //                    Message = e.Message
    //                };
    //            }
    //        });

    //        var failedUpscaleReport = failedUpscaleReportArr.Where(a => !string.IsNullOrEmpty(a?.AlRelPath)).ToList();

    //        _logger.Information($"Finished upscaling {toUpscaleList.Count} imgs in {sw.Elapsed.TotalSeconds} secs");
    //        sw.Stop();
    //        #endregion

    //        #region Compress
    //        var fullCTempPath = Path.Combine(fullAlbumPath, $"[[C{Guid.NewGuid().ToString()}");
    //        Directory.CreateDirectory(fullCTempPath);
    //        var failedUpscalePaths = failedUpscaleReport.Select(b => b.AlRelPath).ToList();
    //        var toCompressList = fileList.Where(a => !failedUpscalePaths.Contains(a.AlRelPath)).ToList();

    //        var cDirNames = toCompressList.Select(a => Path.GetDirectoryName(a.AlRelPath)).Distinct().ToList();
    //        cDirNames.Distinct()
    //            .Where(dir => !string.IsNullOrEmpty(dir))
    //            .ToList()
    //            .ForEach(dir => {
    //                _io.CreateDirectory(Path.Combine(fullCTempPath, dir));
    //            });

    //        var failedCompressReport = new List<FileCorrectionReportModel>();
    //        Parallel.For(0, toCompressList.Count, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
    //            var src = toCompressList[i];

    //            var fullOriginalPath = Path.Combine(fullAlbumPath, src.AlRelPath);
    //            var fullSrcPath = src.CorrectionType == FileCorrectionType.Upscale
    //                ? Path.Combine(fullUTempPath, src.AlRelPath)
    //                : fullOriginalPath;
    //            var fullDstPath = Path.Combine(fullCTempPath, src.AlRelPath);

    //            try {
    //                _ip.CompressImage(fullSrcPath, fullDstPath, src.Compression.Quality, new Size(src.Compression.Width, src.Compression.Height), SupportedMimeType.ORIGINAL);

    //                _io.MoveFile(fullDstPath, fullOriginalPath);
    //            }
    //            catch(Exception e) {
    //                failedCompressReport.Add(new() {
    //                    AlRelPath = src.AlRelPath,
    //                    Success = false,
    //                    Message = e.Message
    //                });
    //            }
    //        });
    //        #endregion

    //        _io.DeleteDirectory(fullUTempPath);
    //        _io.DeleteDirectory(fullCTempPath);

    //        _logDb.UpdateCorrectionLog(param.LibRelPath, DateTime.Now);

    //        var failedCompressPaths = failedCompressReport.Select(a => a.AlRelPath).ToList();
    //        var successList = toCompressList.Where(a => !failedCompressPaths.Contains(a.AlRelPath)).ToList();

    //        var successReport = new FileCorrectionReportModel[successList.Count];
    //        Parallel.For(0, successList.Count, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
    //            var src = successList[i];
    //            var cm = GetFileCorrectionModel(new FileInfo(Path.Combine(fullAlbumPath, src.AlRelPath)), fullAlbumPath);

    //            successReport[i] = new() {
    //                AlRelPath = src.AlRelPath,
    //                Success = true,

    //                Height = cm.Height,
    //                Width = cm.Width,
    //                ByteDisplay = cm.ByteDisplay,
    //                BytesPer100Pixel = cm.BytesPer100Pixel
    //            };
    //        });

    //        var allReport = failedUpscaleReport.Concat(failedCompressReport).Concat(successReport).ToArray();

    //        return allReport;
    //    }
    //    catch(Exception e) {
    //        _logger.Error($"ScRepository.CorrectPages{Environment.NewLine}" +
    //            $"Params=[{JsonSerializer.Serialize(param)}]{Environment.NewLine}" +
    //            $"{e}");

    //        throw;
    //    }
    //}
    #endregion
}