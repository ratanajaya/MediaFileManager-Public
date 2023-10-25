using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Models.ExtraDb;

public class ScrapOperation
{
    public static class OpStatus
    {
        public const string Pending = "Pending";
        public const string OnProgress = "OnProgress";
        public const string Error = "Error";
        public const string Success = "Success";
    }

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string AlbumPath { get; set; }
    [Indexed]
    public string Source { get; set; }

    public string Status { get; set; }
    public string Message { get; set; }

    public string Title { get; set; }

    public DateTime CreateDate { get; set; }
    public DateTime? OperationDate { get; set; }
}