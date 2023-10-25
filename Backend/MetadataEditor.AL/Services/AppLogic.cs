using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SharedLibrary;
using System.Threading.Tasks;
using NaturalSort.Extension;
using CloudAPI.AL.Services;
using SharedLibrary.Helpers;
using MetadataEditor.AL.Models;
using SharedLibrary.Models;

namespace MetadataEditor.AL.Services;

public interface IAppLogic
{
    Task<AlbumViewModel> GetAlbumViewModelAsync(string path, Album oldAlbum);
    Task<string> SaveAlbumJson(AlbumViewModel vm);
    string PostAlbumMetadataOffline(AlbumViewModel vm);
    Task<string> PostAlbumJsonOffline(AlbumViewModel vm, IProgress<FileDisplayModel> progress);
    AlbumViewModel RenameAlbumPath(AlbumViewModel src, string newPath);

    string[] GetTags();
    string[] GetLanguages();
    string[] GetCategories();
    string[] GetOrientations();
    string[] GetCharacters();
}

public class AppLogic : IAppLogic
{
    IAlbumInfoProvider _ai;
    ISystemIOAbstraction _io;
    LibraryRepository _library;
    FileRepository _file;

    public AppLogic(IAlbumInfoProvider albumInfo, ISystemIOAbstraction io, LibraryRepository library, FileRepository file) {
        _ai = albumInfo;
        _io = io;
        _library = library;
        _file = file;
    }

    #region AlbumInfo
    public string[] GetTags() {
        return _ai.Tags;
    }

    public string[] GetCharacters() {
        return _ai.Characters;
    }

    public string[] GetLanguages() {
        return _ai.Languages;
    }

    public string[] GetCategories() {
        return _ai.Categories;
    }

    public string[] GetOrientations() {
        return _ai.Orientations;
    }
    #endregion

    #region QUERY
    public async Task<AlbumViewModel> GetAlbumViewModelAsync(string path, Album oldAlbum) {
        AlbumViewModel result = new AlbumViewModel();

        if (_io.IsFileExists(Path.Combine(path, Constants.FileSystem.JsonFileName))) {
            result.Album = await _io.DeserializeJson<Album>(Path.Combine(path, Constants.FileSystem.JsonFileName));
        }
        else {
            result.Album = CreateStarterAlbumAsync(path, oldAlbum);
        }
        result.Path = path;
        result.AlbumFiles = await GetAllFilesAsync(path);
            
        return result;
    }

    Album CreateStarterAlbumAsync(string path, Album oldAlbum = null) {
        var subDirNames = Directory.GetDirectories(path).Select(a => new DirectoryInfo(a).Name).ToList();

        string folderName = new DirectoryInfo(path).Name;

        (var title, var artists, var languages, var isWip) = AlbumHelper.InferMetadataFromName(folderName, subDirNames);

        return new Album {
            Title = title,
            Category = !string.IsNullOrEmpty(oldAlbum?.Category) 
                ? oldAlbum.Category 
                : Constants.Category.Manga,
            Orientation = !string.IsNullOrEmpty(oldAlbum?.Orientation) 
                ? oldAlbum.Orientation 
                : Constants.Orientation.Portrait,

            Artists = artists,
            Tags = oldAlbum?.Tags != null ? oldAlbum.Tags : new List<string>(),
            Languages = languages,

            Tier = 0,

            IsWip = isWip,
            IsRead = false,

            EntryDate = DateTime.Now
        };
    }

    //Only suitable files
    private async Task<List<string>> GetAllFilesAsync(string path) {
        List<string> result = new List<string>();

        string[] files = _io.GetFiles(path);
        files = files.OrderBy(s => s, StringComparer.OrdinalIgnoreCase.WithNaturalSort()).ToArray();

        foreach(string file in files) {
            if(file.ContainsAny(_ai.SuitableFileFormats)) {
                result.Add(file);
            }
        }
        string[] subDirs = await Task.Run(() => _io.GetDirectories(path));
        foreach(string subDir in subDirs) {
            result.AddRange(await GetAllFilesAsync(subDir));
        }

        return result;
    }

    //Get first image in the directory of SUITABLE FILES type. If not found search in subdirectories
    public async Task<string> GetCover(string path) {
        string albumPath = path;//Used to get relative path
        return await GetCoverDirty(path, albumPath);
    }
    async Task<string> GetCoverDirty(string path, string albumPath) {
        string[] files = _io.GetFiles(path);
        foreach(string file in files) {
            if(file.ContainsAny(_ai.SuitableFileFormats)) {
                string subPath = file.Replace(albumPath + "\\", "").Replace("\\", "/"); //Forward slash for android filesystem
                return subPath;
            }
        }
        string[] subDirs = _io.GetDirectories(path);
        foreach(string subDir in subDirs) {
            return await GetCoverDirty(subDir, albumPath);
        }
        return "";
    }
    #endregion

    #region COMMAND
    public async Task<string> SaveAlbumJson(AlbumViewModel vm) {
        try {
            await Task.Run(() => _io.SerializeToJson(Path.Combine(vm.Path, Constants.FileSystem.JsonFileName), vm.Album));

            return "Success";
        }
        catch (Exception e) {
            return "Failed | " + e.ToString();
        }
    }

    public string PostAlbumMetadataOffline(AlbumViewModel vm) {
        string originalFolder = new DirectoryInfo(vm.Path).Name;
        string albumId = _library.UpdateAlbumMetadata(originalFolder, vm.Album);
        return albumId;
    }

    public async Task<string> PostAlbumJsonOffline(AlbumViewModel vm, IProgress<FileDisplayModel> progress) {
        string originalFolder = new DirectoryInfo(vm.Path).Name;
        try {
            string albumId = await _library.InsertAlbum(originalFolder, vm.Album);

            foreach(string filePath in vm.AlbumFiles) {
                var reportModel = new FileDisplayModel {
                    Path = filePath,
                    UploadStatus = "Uploading.."
                };
                progress.Report(reportModel);

                try {
                    string fileName = Path.GetFileName(filePath);
                    string subDir = filePath.Replace(vm.Path, "").Replace(fileName, "").Replace("\\", "");
                    var fileBytes = _io.ReadFile(filePath);

                    await _file.InsertFileToAlbum(albumId, subDir, fileName, fileBytes);
                    reportModel.UploadStatus = "Success";
                }
                catch(Exception e) {
                    reportModel.UploadStatus = e.Message;
                }

                progress.Report(reportModel);
            }
            int pageCount = _library.RecountAlbumPages(albumId);

            return albumId;
        }
        catch(Exception e) {
            return "Failed | " + e.ToString();
        }
    }

    public AlbumViewModel RenameAlbumPath(AlbumViewModel src, string newFolderName) {
        _io.MoveDirectory(src.Path, newFolderName);
        src.AlbumFiles = src.AlbumFiles.Select(a => a.Replace(src.Path, newFolderName)).ToList();
        src.Path = newFolderName;
        return src;
    }
    #endregion
}