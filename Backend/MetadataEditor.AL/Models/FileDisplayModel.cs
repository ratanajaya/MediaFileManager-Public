using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataEditor.AL.Models;

public class FileDisplayModel
{
    string _fileNameDisplay;
    string _subDirDisplay;
    public string FileNameDisplay { get => _fileNameDisplay; set => _fileNameDisplay = Shorten(value, 20); }
    public string SubDirDisplay { get => _subDirDisplay; set => _subDirDisplay = Shorten(value, 10); }

    string Shorten(string value, int length) {
        string result = value.Replace("\\", "");
        if(result.Length < length) {
            string emptyChar = "";
            int emptyCharCount = length - result.Length;
            for(int i = 0; i < emptyCharCount; i++) {
                emptyChar += " ";
            }
            result += emptyChar;
        }
        if(result.Length > length && length > 3) {
            result = result.Substring(0, length - 3);
            result += "...";
        }
        return result;
    }

    public string UploadStatus { get; set; }
    public string Path { get; set; }
}