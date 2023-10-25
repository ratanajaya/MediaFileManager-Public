using System;
using System.Collections.Generic;

namespace CloudAPI.AL.Models;

[Obsolete]
public class AlbumPageInfo
{
    public string Orientation { get; set; }
    public FileInfoModel[] FileInfos { get; set; }
}