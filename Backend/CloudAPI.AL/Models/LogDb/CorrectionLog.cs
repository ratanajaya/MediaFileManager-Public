using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Models.LogDb;

public class CorrectionLog
{
    [PrimaryKey]
    public string Path { get; set; }
    public DateTime LastCorrectionDate { get; set; }
    public int CorrectablePageCount { get; set; }
}
