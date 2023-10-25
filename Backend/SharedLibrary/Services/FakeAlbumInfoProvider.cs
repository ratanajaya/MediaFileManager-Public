using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using C = SharedLibrary.Constants;
using Ext = SharedLibrary.Constants.Extension;

namespace SharedLibrary;

public class FakeAlbumInfoProvider : IAlbumInfoProvider
{
    public string[] Languages { get; } = { 
        C.Language.English, C.Language.Japanese, C.Language.Chinese, C.Language.Other 
    };
    public string[] Tags {
        get {
            var list = new List<string> { "Cat", "Chicken", "Cow", "Dog", "Horse", "Human", "Pig" };
            var arr = list.OrderBy(s => s).ToArray();
            return arr;
        }
    }
    public string[] Characters { get; } = { };

    public string[] Categories { get; } = {
        "Animal", "Tree"
    };
    public string[] Orientations { get; } = { C.Orientation.Portrait, C.Orientation.Landscape, C.Orientation.Auto };
    public string[] SuitableImageFormats { get; } = { Ext.Jpg, Ext.Jpeg, Ext.Jfif, Ext.Png, Ext.Gif, Ext.Webp };
    public string[] SuitableVideoFormats { get; } = { Ext.Webm, Ext.Mp4 };
    public string[] SuitableFileFormats {
        get {
            return SuitableImageFormats.Concat(SuitableVideoFormats).ToArray();
        }
    }
    public string[] CompressableImageFormats { get; } = { };

    public string[][] GenreNameAndActions { get => CreateGenreNameAndActions(); }

    string[][] CreateGenreNameAndActions() {
        return new string[][] {
        };
    }

    public List<QueryModel> GenreQueries { 
        get {
            var result = new List<QueryModel> {
                new QueryModel{ 
                    Name = "Carnivores Intersect", 
                    Query = "tag:Cat|Dog", 
                    Group = 0 
                },
                new QueryModel{
                    Name = "Carnivores Exclusive",
                    Query = "tag:Cat|Dog,tag!Chicken,tag!Cow,tag!Horse,tag!Human,tag!Pig",
                    Group = 0
                },
                new QueryModel{ 
                    Name = "Herbivores Intersect", 
                    Query = "tag:Cow|Horse", 
                    Group = 0
                },
                new QueryModel{
                    Name = "Herbivores Exclusive",
                    Query = "tag:Cow|Horse,tag!Cat,tag!Chicken,tag!Dog,tag!Human,tag!Pig",
                    Group = 0
                },
                new QueryModel{
                    Name = "Omnivores Intersect",
                    Query = "tag:Chicken|Human|Pig",
                    Group = 0
                },
                new QueryModel{
                    Name = "Omnivores Exclusive",
                    Query = "tag:Chicken|Human|Pig,tag!Cat,tag!Dog,tag!Cow,tag!Horse",
                    Group = 0
                },
                new QueryModel{
                    Name = "Non-Animal",
                    Query = "tag!Cat,tag!Chicken,tag!Cow,tag!Dog,tag!Horse,tag!Human",
                    Group = 0
                },
            };
            return result;
        }
    }

    public string[] Tier1Artists { get; } = { 
        "Alan",
        "Bob",
        "Dana White",
        "Greg",
        "Jack"
    };

    public string[] Tier2Artists { get; } = {
    };

    public bool IsImage(string path) {
        return SuitableImageFormats.Contains(Path.GetExtension(path));
    }

    public bool IsVideo(string path) {
        return SuitableVideoFormats.Contains(Path.GetExtension(path));
    }
}