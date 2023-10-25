using System.Collections.Generic;

namespace CloudAPI.AL.Models;

public class AlbumFsNodeInfo
{
    public string Title { get; set; }
    public string Orientation { get; set; }
    public List<FsNode> FsNodes { get; set; }
}