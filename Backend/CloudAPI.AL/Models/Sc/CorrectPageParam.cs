using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Models.Sc;

public class CorrectPageParam
{
    public int Type { get; set; }
    public string LibRelPath { get; set; }
    public int Thread { get; set; }
    public UpscalerType UpscalerType { get; set; }
    public List<FileCorrectionModel> FileToCorrectList { get; set; }
    public bool ToJpeg { get; set; }
}
