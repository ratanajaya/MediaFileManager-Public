using CloudAPI.AL.Models.Sc;
using CloudAPI.AL.Services;
using CloudAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CloudAPI.Controllers;

[ApiController]
[Route("Operation")]
public class OperationController : ControllerBase
{
    private OperationService _os;

    public OperationController(OperationService os) {
        _os = os;
    }

    #region Correction
    [HttpPost("HScanCorrectiblePaths")]
    public IActionResult HScanCorrectiblePaths(HScanCorrectiblePathParam param) {
        var result = _os.HScanCorrectiblePages(param.Paths, param.Thread, param.UpscaleTarget);

        return Ok(result);
    }

    [HttpGet("ScGetCorrectablePaths")]
    public IActionResult ScGetCorrectablePaths() {
        var result = _os.ScGetCorrectablePaths();

        return Ok(result);
    }

    [HttpPost("ScFullScanCorrectiblePaths")]
    public IActionResult ScFullScanCorrectiblePaths(int thread, int upscaleTarget) {
        var result = _os.ScFullScanCorrectiblePaths(thread, upscaleTarget);

        return Ok(result);
    }

    [HttpGet("GetCorrectablePages")]
    public IActionResult GetCorrectablePages(int type, string path, int thread, int upscaleTarget, bool clampToTarget) {
        var result = _os.GetCorrectablePages(type, path, thread, upscaleTarget, clampToTarget);

        return Ok(result);
    }

    [HttpPost("CorrectPages")]
    public IActionResult CorrectPages(CorrectPageParam param) {
        var result = _os.CorrectPages(param);

        return Ok(result);
    }
    #endregion
}
