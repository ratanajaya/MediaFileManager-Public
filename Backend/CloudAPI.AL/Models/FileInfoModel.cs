using System;

namespace CloudAPI.AL.Models;

public class FileInfoModel
{
    public string Name { get; set; }
    public string Extension { get; set; }
    [Obsolete("still being used alot, but consider removing it in the future")]
    public string UncPathEncoded { get; set; } //LibRelPath

    public long Size { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }

    public PageOrientation? Orientation { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
}

public enum PageOrientation
{
    Portrait = 1,
    Landscape = 2
}