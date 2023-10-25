using SharedLibrary.Models;
using System;

namespace CloudAPI.AL.Models.Dashboard;

public class LogDashboardModel
{
    public string Id { get; set; }
    public string AlbumFullTitle { get; set; }
    public string Operation { get; set; }
    public DateTime CreationTime { get; set; }

    public Album Album { get; set; }
}
