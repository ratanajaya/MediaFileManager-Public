using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using CloudAPI.AL.Models.ExtraDb;
using CloudAPI.AL.Services;
using CloudAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloudAPI.Controllers;

[ApiController]
[Route("ExtraInfo")]
public class ExtraInfoController : ControllerBase
{
    ExtraInfoService _ei;
    ILogDbContext _logDb;

    public ExtraInfoController(ExtraInfoService ei, ILogDbContext logDb) {
        _ei = ei;
        _logDb = logDb;
    }

    [HttpGet("GetScrapOperations")]
    public IActionResult GetScrapOperations(string albumPath) {
        return Ok(_ei.GetScrapOperations(albumPath));
    }

    [HttpPost("InsertScrapOperation")]
    public IActionResult InsertScrapOperation(ScrapOperationParamModel param) {
        return Ok(_ei.InsertScrapOperation(param));
    }

    [HttpPost("UpdateScrapOperation")]
    public IActionResult UpdateScrapOperation(ScrapOperationParamModel param) {
        return Ok(_ei.UpdateScrapOperation(param));
    }

    [HttpGet("GetComments")]
    public IActionResult GetComments(int scrapOperationId) {
        return Ok(_ei.GetComments(scrapOperationId));
    }

    [HttpGet("OneTimeMigration")]
    public IActionResult OneTimeMigration() {
        //_ei.OneTimeMigration();
        _logDb.OneTimeMigration();
        return Ok();
    }
}