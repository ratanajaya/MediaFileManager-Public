using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Helpers;
using CloudAPI.AL.Models;
using CloudAPI.AL.Models.LogDb;
using CloudAPI.AL.Models.Sc;
using Microsoft.Extensions.Configuration;
using Serilog;
using SharedLibrary;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using qh = CloudAPI.AL.Helpers.QueryHelpers;

namespace CloudAPI.AL.Services;

public class LibraryRepository 
{
    ILogger _logger;
    ConfigurationModel _config;
    IAlbumInfoProvider _ai;
    ISystemIOAbstraction _io;
    IDbContext _db;
    ILogDbContext _sqlite;

    public LibraryRepository(ILogger logger, ConfigurationModel config, IAlbumInfoProvider ai, ISystemIOAbstraction io, IDbContext db, ILogDbContext sqlite) {
        _logger = logger;
        _config = config;
        _ai = ai;
        _io = io;
        _db = db;
        _sqlite = sqlite;
    }

    #region Query
    public IEnumerable<AlbumVM> GetAlbumVMs(int page, int row, string query) {
        var querySegments = qh.GetQuerySegments(query);
        var filteredAlbum = _db.AlbumVMs.Where(a => qh.MatchAllQueries(a.Album, querySegments, _ai.Tier1Artists.Concat(_ai.Tier2Artists).ToArray(), _ai.Characters));
        var orderedAlbum = filteredAlbum
            .OrderBy(a => a.Album.IsRead)
            .ThenBy(a => a.Album.GetFullTitleDisplay());
        var pagedAlbum = (page > 0 && row > 0) ? orderedAlbum.Skip((page - 1) * row).Take(row) : orderedAlbum;

        return pagedAlbum;
    }
        
    public AlbumVM GetAlbumVM(string albumPath) {
        return _db.AlbumVMs.Get(albumPath);
    }
    #endregion

    #region Command
    internal void SaveAlbumMetadata(AlbumVM albumVM) {
        _io.SerializeToJson(Path.Combine(_config.LibraryPath, albumVM.Path, Constants.FileSystem.JsonFileName), albumVM.Album);
        _sqlite.InsertCrudLog(CrudLog.Update, albumVM);
    }

    public string UpdateAlbum(AlbumVM albumVM) {
        try {
            var existing = _db.AlbumVMs.Get(albumVM.Path);

            if(existing == null)
                throw new InvalidOperationException("Album with specified id not found");
            if(existing.Album.EntryDate != albumVM.Album.EntryDate)
                throw new InvalidOperationException("Update on album's EntryDate is forbidden");

            albumVM.Album.ValidateAndCleanup();

            existing.Album = albumVM.Album;

            SaveAlbumMetadata(existing);
            _db.SaveChanges();

            return existing.Path;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.UpdateAlbum{Environment.NewLine}" +
                $"Params=[{JsonSerializer.Serialize(albumVM)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string DeleteAlbum(string albumPath, bool insertCrudLog = true) {
        try {
            var existing = _db.AlbumVMs.Get(albumPath);

            if(existing == null)
                throw new InvalidOperationException("Album with specified id not found");

            var albumFullPath = Path.Combine(_config.LibraryPath, existing.Path);

            DeleteAlbumCache(albumPath);
            _io.DeleteDirectory(albumFullPath);

            _db.RemoveAlbumVM(existing);
            _db.SaveChanges();

            if(insertCrudLog)
                _sqlite.InsertCrudLog(CrudLog.Delete, existing);

            return albumPath;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.DeleteAlbum{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public int DeleteAlbumChapter(string albumPath, string subDir) {
        try {
            var existing = _db.AlbumVMs.Get(albumPath);

            if(existing == null)
                throw new Exception("Album not found");

            var chapterPath = Path.Combine(_config.LibraryPath, existing.Path, subDir);

            _io.DeleteDirectory(chapterPath);

            int newPageCount = RecountAlbumPages(albumPath);

            return newPageCount;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.DeleteAlbumChapter{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string UpdateAlbumOuterValue(string albumPath, int lastPageIndex) {
        try {
            var existing = _db.AlbumVMs.Get(albumPath);

            existing.LastPageIndex = lastPageIndex == existing.PageCount - 1 ? 0 : lastPageIndex;

            if(!existing.Album.IsRead && lastPageIndex == existing.PageCount - 1) {
                existing.Album.IsRead = true;
                existing.LastPageIndex = 0;
                if(existing.Album.Note == "HP")
                    existing.Album.Note = "";
                SaveAlbumMetadata(existing);
            }

            _db.SaveChanges();
            return existing.Path;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.UpdateAlbumOuterValue{Environment.NewLine}" +
                $"Params=[{albumPath},{lastPageIndex}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void UpdateAlbumCorrectablePages(List<PathCorrectionModel> pathCorrections) {
        try {
            var paths = pathCorrections.Select(a => a.LibRelPath).ToList();

            var albumVms = _db.AlbumVMs.Where(a => paths.Contains(a.Path)).ToList();

            albumVms.ForEach(a => {
                a.CorrectablePageCount = pathCorrections.First(b => b.LibRelPath == a.Path).CorrectablePageCount;
                SaveAlbumMetadata(a);
            });

            _db.SaveChanges();
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.UpdateAlbumOuterValue{Environment.NewLine}" +
                $"Params=[{JsonSerializer.Serialize(pathCorrections)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string UpdateAlbumTier(string albumPath, int tier) {
        try {
            var existing = _db.AlbumVMs.Get(albumPath);

            existing.Album.Tier = tier;

            SaveAlbumMetadata(existing);

            _db.SaveChanges();
            return existing.Path;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.UpdateAlbumTier{Environment.NewLine}" +
                $"Params=[{albumPath},{tier}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public int RecountAlbumPages(string albumPath) {
        try {
            var targetAlbum = _db.AlbumVMs.Get(albumPath);
            var fullAlbumPath = Path.Combine(_config.LibraryPath, targetAlbum.Path);

            var suitableFilePaths = _io.GetSuitableFilePaths(fullAlbumPath, _ai.SuitableFileFormats, 1);

            targetAlbum.PageCount = suitableFilePaths.Count;
            targetAlbum.CoverInfo = _db.GetFirstFileInfo(suitableFilePaths);

            _db.SaveChanges();

            return targetAlbum.PageCount;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.RecountAlbumPages{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void DeleteAlbumCache(string albumPath) {
        try {
            if(string.IsNullOrWhiteSpace(albumPath))
                throw new InvalidOperationException("albumPath is null or empty");

            var sizeFolders = _io.GetDirectories(_config.FullPageCachePath);
            foreach(var sizeFolder in sizeFolders) {
                var fullALbumCachePath = Path.Combine(sizeFolder, albumPath);
                _io.DeleteDirectory(fullALbumCachePath);
            }
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.DeleteAlbumCache{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public async Task ReloadDatabase(Func<EventStreamData, Task> report) {
        try {
            await _db.Reload(report);
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.ReloadDatabase{Environment.NewLine}" +
                $"Params=[]" +
                $"{e}");

            throw;
        }
    }

    public async Task RescanDatabase(Func<EventStreamData,Task> report) {
        try {
            await _db.Rescan(report);
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.RescanDatabase{Environment.NewLine}" +
                $"Params=[]" +
                $"{e}");

            throw;
        }
    }
    #endregion

    #region Command By MetadataEditor
    string GetLibRelAlbumLocation(string firstLetter) {
        firstLetter = firstLetter.ToLower();
        string result = firstLetter == "a" ? "ABC" : firstLetter == "b" ? "ABC" : firstLetter == "c" ? "ABC" :
                        firstLetter == "d" ? "DEF" : firstLetter == "e" ? "DEF" : firstLetter == "f" ? "DEF" :
                        firstLetter == "g" ? "GHI" : firstLetter == "h" ? "GHI" : firstLetter == "i" ? "GHI" :
                        firstLetter == "j" ? "JKL" : firstLetter == "k" ? "JKL" : firstLetter == "l" ? "JKL" :
                        firstLetter == "m" ? "MNO" : firstLetter == "n" ? "MNO" : firstLetter == "o" ? "MNO" :
                        firstLetter == "p" ? "PQR" : firstLetter == "q" ? "PQR" : firstLetter == "r" ? "PQR" :
                        firstLetter == "s" ? "STU" : firstLetter == "t" ? "STU" : firstLetter == "u" ? "STU" :
                        firstLetter == "v" ? "VWXYZ" : firstLetter == "w" ? "VWXYZ" : firstLetter == "x" ? "VWXYZ" :
                        firstLetter == "y" ? "VWXYZ" : firstLetter == "z" ? "VWXYZ" :
                        "000";
        return result;
    }

    string GetFirstLetter(string source) {
        string normalizedSource = source.RemoveNonLetterDigit();
        string result = normalizedSource[0].ToString();
        return result;
    }

    public async Task<string> InsertAlbum(string originalFolderName, Album album) {
        string firstLetter = GetFirstLetter(originalFolderName);
        string libRelSubDir = GetLibRelAlbumLocation(firstLetter);
        string libRelAlbumPath = Path.Combine(libRelSubDir, originalFolderName);

        var existing = _db.AlbumVMs.Get(libRelAlbumPath);
        if(existing != null) {
            DeleteAlbum(existing.Path, false);
            await Task.Delay(20);
        }

        _io.CreateDirectory(Path.Combine(_config.LibraryPath, libRelAlbumPath));
        _io.SerializeToJson(Path.Combine(_config.LibraryPath, libRelAlbumPath, Constants.FileSystem.JsonFileName), album);

        var newAlbum = new AlbumVM {
            Album = album,
            Path = libRelAlbumPath,
            PageCount = 0,
            LastPageIndex = 0,
        };
        _db.AddAlbumVM(newAlbum);
        _db.SaveChanges();
        _sqlite.InsertCrudLog(CrudLog.Insert, newAlbum);

        return newAlbum.Path;
    }

    public string UpdateAlbumMetadata(string originalFolderName, Album album) {
        string firstLetter = GetFirstLetter(originalFolderName);
        string libRelSubDir = GetLibRelAlbumLocation(firstLetter);
        string libRelAlbumPath = Path.Combine(libRelSubDir, originalFolderName);

        var existing = _db.AlbumVMs.Get(libRelAlbumPath);
        if(existing == null) return "Album does not exist";

        _io.CreateDirectory(Path.Combine(_config.LibraryPath, libRelAlbumPath));
        _io.SerializeToJson(Path.Combine(_config.LibraryPath, libRelAlbumPath, Constants.FileSystem.JsonFileName), album);

        existing.Album = album;
        _db.SaveChanges();

        return existing.Path;
    }
    #endregion
}