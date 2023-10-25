using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using C = SharedLibrary.Constants;
using Ext = SharedLibrary.Constants.Extension;

namespace SharedLibrary;

public class AlbumInfoProvider : IAlbumInfoProvider
{
    public string[] Languages { get; } = { 
        C.Language.English, C.Language.Japanese, C.Language.Chinese, C.Language.Other 
    };
    public string[] Tags { get; } = { };
    public string[] Characters { get; } = { };
    public string[] Categories { get; } = { C.Category.Manga, C.Category.CGSet, C.Category.SelfComp };
    public string[] Orientations { get; } = { C.Orientation.Portrait, C.Orientation.Landscape, C.Orientation.Auto };

    public string[] SuitableImageFormats { get; } = { Ext.Jpg, Ext.Jpeg, Ext.Jfif, Ext.Png, Ext.Gif, Ext.Webp };
    public string[] SuitableVideoFormats { get; } = { Ext.Webm, Ext.Mp4 };
    public string[] SuitableFileFormats {
        get { return SuitableImageFormats.Concat(SuitableVideoFormats).ToArray(); }
    }
    public string[] CompressableImageFormats { get; } = { Ext.Jpg, Ext.Jpeg, Ext.Jfif, Ext.Png };

    public List<QueryModel> GenreQueries {
        get {
            return new List<QueryModel> { };
        }
    }

    public string[] Tier1Artists { get; } = { };

    public string[] Tier2Artists { get; } = { };

    public bool IsImage(string path) {
        return SuitableImageFormats.Contains(Path.GetExtension(path));
    }

    public bool IsVideo(string path) {
        return SuitableVideoFormats.Contains(Path.GetExtension(path));
    }
}