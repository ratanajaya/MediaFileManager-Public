using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Models;

public class ScrapOperationParamModel
{
    public int Id { get; set; }
    public string AlbumPath { get; set; }
    public string Source { get; set; }
}
