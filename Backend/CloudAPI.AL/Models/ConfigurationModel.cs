using SharedLibrary.Enums;
using System;
using System.IO;

namespace CloudAPI.AL.Models;

public class ConfigurationModel
{
    public string LibraryPath { get; set; }
    public string TempPath { get; set; }
    public string AppType { get; set; }
    public string Version { get; set; }
    public string BuildType { get; set; }

    public string FullPageCachePath { 
        get { return Path.Combine(TempPath, "_hCache"); }
    }
    public string ScFullCachePath {
        get { return Path.Combine(TempPath, "_scCache"); }
    }
    #region Extra Info
    public string FullAlbumDbPath {
        get { return Path.Combine(LibraryPath, "_extraInfo", "_dbAlbum.msgpack"); }
    }
    public string FullLogDbPath {
        get { return Path.Combine(LibraryPath, "_extraInfo", "_db.sqlite"); }
    }
    public string FullExtraDbPath {
        get { return Path.Combine(LibraryPath, "_extraInfo", "_extraDb.sqlite"); }
    }

    public string DefaultThumbnailName = "_defaultThumb.png";
    public string LibRelDefaultThumbnailPath {
        get { return Path.Combine("_extraInfo", DefaultThumbnailName); }
    }
    public string FullDefaultThumbnailPath {
        get { return Path.Combine(LibraryPath, LibRelDefaultThumbnailPath); }
    }
    #endregion

    #region ScExtraInfo
    public string ScLibraryPath { get; set; }
    public string ScFullExtraInfoPath {
        get { return Path.Combine(ScLibraryPath, "_extraInfo"); }
    }
    public string ScFullAlbumDbPath { 
        get { return Path.Combine(ScFullExtraInfoPath, "_scDbAlbum.json"); } 
    }
    public string ScFullDefaultThumbnailPath {
        get { return Path.Combine(ScLibraryPath, LibRelDefaultThumbnailPath); }
    }
    public int ScLibraryDepth = 1;
    #endregion

    public bool IsPrivate {
        get { return BuildType != "Public"; }
    }

    public bool IsPublic { 
        get { return BuildType == "Public"; } 
    }

    public string GetLibraryPath(LibraryType type) {
        return type == LibraryType.Regular ? LibraryPath : ScLibraryPath;
    }
    public string GetCachePath(LibraryType type) {
        return type == LibraryType.Regular ? FullPageCachePath : ScFullCachePath;
    }
    public string GetDefaultThumbnailPath(LibraryType type) {
        return type == LibraryType.Regular ? FullDefaultThumbnailPath : ScFullDefaultThumbnailPath;
    }
}