using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using CloudAPI.AL.Models.ExtraDb;
using CloudAPI.AL.Services;
using CloudAPI.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using System.Linq;

namespace CloudAPI.Controllers;

[ApiController]
[Route("Static")]
public class StaticController : ControllerBase
{
    StaticInfoService _si;
    CensorshipService _cs;

    public StaticController(StaticInfoService si, CensorshipService cs) {
        _si = si;
        _cs = cs;
    }

    [HttpGet("GetAlbumInfo")]
    public IActionResult GetAlbumInfo() {
        var data = _si.GetAlbumInfoVm();

        var censoredData = _cs.ConCensorAlbumInfoVm(data);

        return Ok(censoredData);
    }

    [HttpGet("GetTagVMs")]
    public IActionResult GetTagVMs() {
        var data = _si.GetTagVMs();

        var censoredData = _cs.ConCensorQueryVms(data);

        return Ok(censoredData);
    }

    [HttpGet("GetUpscalers")]
    public IActionResult GetUpscalers() {
        var data = _si.GetUpscalers();

        return Ok(data);
    }
}