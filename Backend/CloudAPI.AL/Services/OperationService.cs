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

public class OperationService {
    ConfigurationModel _config;
    ISystemIOAbstraction _io;
    IAlbumInfoProvider _ai;
    ILogger _logger;
    ImageProcessor _ip;
    ILogDbContext _logDb;
    LibraryRepository _library;

    public OperationService(ConfigurationModel config, ISystemIOAbstraction io, IAlbumInfoProvider ai, ILogger logger, ImageProcessor ip, ILogDbContext logDb, LibraryRepository library) {
        _config = config;
        _io = io;
        _ai = ai;
        _logger = logger;
        _ip = ip;
        _logDb = logDb;
        _library = library;
    }

    #region Correction
    public List<PathCorrectionModel> HScanCorrectiblePages(List<string> libRelPaths, int thread, int upscaleTarget) {
        var result = libRelPaths.Select(a => new PathCorrectionModel {
            LibRelPath = a,
            CorrectablePageCount = (GetCorrectablePages(0, a, thread, upscaleTarget, false)).Count
        }).ToList();

        _library.UpdateAlbumCorrectablePages(result);

        return result;
    }

    public List<PathCorrectionModel> ScGetCorrectablePaths() {
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

    public List<PathCorrectionModel> ScFullScanCorrectiblePaths(int thread, int upscaleTarget) {
        var correctablePaths = ScGetCorrectablePaths();

        foreach(var path in correctablePaths) {
            var correctablePageCount = (GetCorrectablePages(1, path.LibRelPath, thread, upscaleTarget, false)).Count;

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

    public List<FileCorrectionModel> GetCorrectablePages(int type, string libRelAlbumPath, int thread, int upscaleTarget, bool clampToTarget) {
        int trueThread = Math.Clamp(thread, 1, 5);
        //This method becomes unstable and produces faulty images if performing upscaling/compression using 6 threads
        //Tested with 6 Core Ryzen 5 5600 6-Core CPU

        try {
            var libPath = _config.GetLibraryPath((LibraryType)type);

            var fullAlbumPath = Path.Combine(libPath, libRelAlbumPath);

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
                            newFcm.Extension == Constants.Extension.Png,
                            clampToTarget ? upscaleTarget : null
                        );
                    }
                    else if(ImageHelper.IsLargeEnoughForCompression(newFcm)) {
                        newFcm.CorrectionType = FileCorrectionType.Compress;
                        newFcm.Compression = ImageHelper.DetermineCompressionCondition(
                            newFcm.Height, 
                            newFcm.Width, 
                            newFcm.Extension == Constants.Extension.Png,
                            null //clampToTarget ? upscaleTarget : null //Don't to target if compression is the only operation
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

            var libPath = _config.GetLibraryPath((LibraryType)param.Type);
            var cachePath = _config.GetCachePath((LibraryType)param.Type);

            var fullAlbumPath = Path.Combine(libPath, param.LibRelPath);

            var scAlbumCachePath = Path.Combine(cachePath, param.LibRelPath);

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
                    var guid = Guid.NewGuid().ToString();
                    var stagingName = $"{guid}{src.Extension}";
                    var compressedName = param.ToJpeg ? $"{guid}.jpeg" : stagingName;

                    var fullOriPath = Path.Combine(fullAlbumPath, src.AlRelPath);
                    var fullUpsPath = Path.Combine(fullUTempPath, stagingName);
                    var fullComPath = Path.Combine(fullCTempPath, compressedName);
                    var fullDstPath = param.ToJpeg ? $"{Path.Combine(Path.GetDirectoryName(fullOriPath), Path.GetFileNameWithoutExtension(fullOriPath))}.jpeg" : fullOriPath;

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

                    var mimeType = param.ToJpeg ? SupportedMimeType.JPEG : SupportedMimeType.ORIGINAL;
                    
                    _ip.CompressImage(fullCompSrcPath, fullComPath, src.Compression.Quality, new Size(src.Compression.Width, src.Compression.Height), mimeType);

                    messageSb.Append(" | Compress Finished");

                    _io.DeleteFile(fullOriPath);

                    _io.CopyFile(fullComPath, fullDstPath, false);

                    report[i] = new() {
                        AlRelPath = src.AlRelPath,
                        AlRelDstPath = Path.GetRelativePath(fullAlbumPath, fullDstPath),
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

            if(param.Type == 0) {
                _library.UpdateAlbumCorrectablePages(new List<PathCorrectionModel> {
                    new() {
                        LibRelPath = param.LibRelPath,
                        CorrectablePageCount = 0
                    }
                });
            }
            else
                _logDb.UpdateCorrectionLog(param.LibRelPath, DateTime.Now, 0);

            _logger.Information($"Corrected {fileCount} pages in {sw.Elapsed.TotalSeconds} secs");

            Parallel.For(0, report.Length, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
                var src = report[i];

                if(src.Success) {
                    var cm = GetFileCorrectionModel(new FileInfo(Path.Combine(fullAlbumPath, src.AlRelDstPath)), fullAlbumPath);

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
}