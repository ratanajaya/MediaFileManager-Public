using System.Collections.Generic;

namespace CloudAPI.Models;

public class HScanCorrectiblePathParam
{
    public List<string> Paths { get; set; }
    public int Thread { get; set; }
    public int UpscaleTarget { get; set; }
}
