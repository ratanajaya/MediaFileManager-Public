using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using CloudAPI.AL.Models;
using CloudAPI.AL.Services;
using SharedLibrary.Enums;
using MimeTypes;
using CloudAPI.Services;

namespace CloudAPI.Controllers;

[ApiController]
[Route("Media")]
public class MediaController : ControllerBase
{
    ConfigurationModel _config;
    FileRepository _file;
    CensorshipService _cs;

    public MediaController(ConfigurationModel config, FileRepository file, CensorshipService cs) {
        _config = config;
        _file = file;
        _cs = cs;
    }

    [HttpGet("StreamPage")]
    public IActionResult StreamPage(string libRelPath, LibraryType type) {
        var decensoredLibRelPath = _cs.ConDecensorLibRelMediaPath(libRelPath);

        string fullPath = Path.Combine(_config.GetLibraryPath(type), decensoredLibRelPath);

        return PhysicalFile(fullPath, MimeTypeMap.GetMimeType(Path.GetExtension(fullPath)), true);
    }

    [HttpGet("StreamResizedImage")]
    public async Task<IActionResult> StreamResizedImage(string libRelPath, int maxSize, LibraryType type) {
        var decensoredLibRelPath = _cs.ConDecensorLibRelMediaPath(libRelPath);

        string fullPath = await _file.GetFullCachedPath(decensoredLibRelPath, maxSize, type);

        return PhysicalFile(fullPath, MimeTypeMap.GetMimeType(Path.GetExtension(fullPath)), true);
    }
}