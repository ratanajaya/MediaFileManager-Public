using CloudAPI.AL.Models;
using System.Collections.Generic;

namespace CloudAPI.AL.Models.Dashboard;

public class AlbumDashboardModel
{
    public string FullTitle { get; set; }
    public List<string> Languages { get; set; }
    public bool IsRead { get; set; }
    public bool IsWip { get; set; }
    public int Tier { get; set; }
    public int PageCount { get; set; }
    public string Note { get; set; }
    public FileInfoModel CoverInfo { get; set; }
}