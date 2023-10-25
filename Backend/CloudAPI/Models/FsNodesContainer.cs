using CloudAPI.AL.Models;
using System.Collections.Generic;

namespace CloudAPI.Models;

public class FsNodeContainer
{
    public string Title { get; set; }
    public List<FsNode> FsNodes { get; set; }
}