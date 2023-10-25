using LiteDB;
using SharedLibrary.Models;
using SQLite;
using System;

namespace CloudAPI.AL.Models.LogDb;

public class CrudLog
{
    public const string Insert = "I";
    public const string Update = "U";
    public const string Delete = "D";

    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    public string Operation { get; set; }
    public string AlbumPath { get; set; }
    public string AlbumFullTitle { get; set; }
    public string AlbumJson { get; set; }
    public DateTime CreateDate { get; set; }

    public CrudLog() { }

    public CrudLog(string operation, string albumPath, Album album, DateTime createDate) {
        if(operation == Delete) {
            AlbumJson = Utf8Json.JsonSerializer.ToJsonString(album);
        }

        Operation = operation;
        AlbumPath = albumPath;
        AlbumFullTitle = album.GetFullTitleDisplay();
        CreateDate = createDate;
    }
}