using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Models.Sc;

public class PathCorrectionModel
{
    public string LibRelPath { get; set; }
    public DateTime? LastCorrectionDate { get; set; }
    public int CorrectablePageCount { get; set; }
}
