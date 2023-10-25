using SharedLibrary.Models;

namespace CloudAPI;

public class AlbumCreateModel
{
    public string OriginalFolderName { get; set; }
    public Album Album { get; set; }
}
