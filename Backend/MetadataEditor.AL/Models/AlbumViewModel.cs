using System;
using System.Collections.Generic;
using System.Text;
using SharedLibrary.Models;

namespace MetadataEditor.AL.Models;

public class AlbumViewModel
{
    public Album Album { get; set; }
    public string Path { get; set; }
    public List<string> AlbumFiles { get; set; }
}