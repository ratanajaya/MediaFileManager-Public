using CloudAPI.AL.Models;
using CloudAPI.AL.Models.Sc;
using CloudAPI.AL.Services;
using CloudAPI.Controllers.Abstraction;
using CloudAPI.CustomMiddlewares;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI.Controllers;

[ApiController]
[Route("Sc")]
public class ScController : ControllerBase, ILibraryController
{
    AuthorizationService _auth;
    ScRepository _sc;
    ConfigurationModel _config;

    public ScController(AuthorizationService auth, ScRepository sc, ConfigurationModel config) {
        _auth = auth;
        _sc = sc;
        _config = config;
    }

    #region Album Query
    [HttpGet("GetAlbumCardModels")]
    public IActionResult GetAlbumCardModels(int page, int row, string query) {
        var libRelPath = query != null ? query : "";

        var data = _config.IsPrivate
            ? _sc.GetAlbumVMs(libRelPath)
            : new List<ScAlbumVM>().AsEnumerable();

        var result = data
            .Select(e => new AlbumCardModel { 
                Path = e.LibRelPath,
                FullTitle = e.Name,
                LastPageIndex = e.LastPageIndex,
                PageCount = e.PageCount,

                Languages = new List<string>(),
                Tier = 0,
                IsRead = true,
                IsWip = false,
                CoverInfo = e.CoverInfo
            })
            .ToList();

        return Ok(result);
    }
    #endregion

    #region Album Command
    [HttpPut("UpdateAlbumTier")]
    public IActionResult UpdateAlbumTier(AlbumTierParam param) {
        return Ok("Success");
    }

    [HttpGet("RecountAlbumPages")]
    public IActionResult RecountAlbumPages(string path) {
        int pageCount = _sc.CountAlbumPages(path);
        return Ok(pageCount);
    }

    [HttpPost("UpdateAlbumOuterValue")]
    public IActionResult UpdateAlbumOuterValue(AlbumOuterValueParam param) {
        _sc.UpdateAlbumOuterValue(param.AlbumPath, param.LastPageIndex);
        return Ok(param.AlbumPath);
    }

    [HttpGet("RefreshAlbum")]
    public IActionResult RefreshAlbum(string path) {
        _sc.DeleteAlbumCache(path);
        return Ok("Success");
    }

    #endregion

    #region Page Query
    [HttpGet("GetAlbumFsNodeInfo")]
    public IActionResult GetAlbumFsNodeInfo(string path, bool includeDetail, bool includeDimension) {
        var result = _sc.GetAlbumFsNodeInfo(path, includeDetail, includeDimension);

        return Ok(result);
    }
    #endregion

    #region Page Command
    [HttpDelete("DeleteFile")]
    public IActionResult DeleteFile(string path, string alRelPath) {
        _auth.DisableRouteOnPublicBuild();

        var result = _sc.DeleteFile(path, alRelPath);
        return Ok(result);
    }

    [HttpPost("MoveFile")]
    public IActionResult MoveFile(FileMoveParamModel param) {
        _auth.DisableRouteOnPublicBuild();

        var result = _sc.MoveFile(param);
        return Ok(result);
    }

    [HttpDelete("DeleteAlbumChapter")]
    public IActionResult DeleteAlbumChapter(string path, string chapterName) {
        _auth.DisableRouteOnPublicBuild();

        var newPageCount = _sc.DeleteAlbumChapter(path, chapterName);
        return Ok(newPageCount);
    }
    #endregion

    #region UNUSED
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult UpdateAlbum(AlbumVM albumVM) {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetAlbumVM(string path) {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetGenreCardModels() {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetFeaturedArtistCardModels() {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DeleteAlbum(string path) {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task ReloadDatabase() {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task RescanDatabase() {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task QuickScan() {
        throw new NotImplementedException();
    }
    #endregion

    #region SC Exclusive Features
    //[HttpGet("GetCorrectablePaths")]
    //public IActionResult GetCorrectablePaths() {
    //    var result = _sc.GetCorrectablePaths();

    //    return Ok(result);
    //}

    //[HttpPost("FullScanCorrectiblePaths")]
    //public IActionResult FullScanCorrectiblePaths(int thread, int upscaleTarget) {
    //    var result = _sc.FullScanCorrectiblePaths(thread, upscaleTarget);

    //    return Ok(result);
    //}

    //[HttpGet("GetCorrectablePages")]
    //public IActionResult GetCorrectablePages(string path, int thread, int upscaleTarget) {
    //    var result = _sc.GetCorrectablePages(path, thread, upscaleTarget);

    //    return Ok(result);
    //}

    //[HttpPost("CorrectPages")]
    //public IActionResult CorrectPages(CorrectPageParam param) {
    //    var result = _sc.CorrectPages(param);

    //    return Ok(result);
    //}
    #endregion
}