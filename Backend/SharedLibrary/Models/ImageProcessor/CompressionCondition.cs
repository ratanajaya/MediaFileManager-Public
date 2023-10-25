using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models;

public class CompressionCondition
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Quality { get; set; } = 90;
}