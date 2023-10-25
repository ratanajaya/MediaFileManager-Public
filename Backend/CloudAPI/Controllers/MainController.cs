using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SharedLibrary;
using CloudAPI.AL.Services;
using CloudAPI.AL.Models;
using CloudAPI.CustomMiddlewares;
using CloudAPI.Controllers.Abstraction;
using Microsoft.AspNetCore.Http;
using System.IO;
using CloudAPI.Services;

namespace CloudAPI.Controllers;

[ApiController]
[Route("Main")]
public class MainController : ControllerBase, ILibraryController
{
    AuthorizationService _auth;
    LibraryRepository _library;
    FileRepository _file;
    IAlbumInfoProvider _ai;
    ConfigurationModel _config;
    CensorshipService _cs;

    public MainController(AuthorizationService auth, LibraryRepository library, FileRepository file, IAlbumInfoProvider ai, ConfigurationModel config, CensorshipService cs) {
        _auth = auth;
        _library = library;
        _file = file;
        _ai = ai;
        _config = config;
        _cs = cs;
    }

    #region Private Methods
    (FileInfoModel fi, int count) GetCoverPageInfo(string query) {
        (var cpi, var count) = _file.GetRandomCoverPageInfoByQuery(query);

        return (cpi, count);
    }

    async Task WriteResponse(EventStreamData data) {
        #region Important Lesson from static
        //NEVER fuck around with static class configurations
        //They may cause weird behavior somewhere else in the application
        //JsonSerializer.SetDefaultResolver(StandardResolver.CamelCase);
        //var OLDdataJson = JsonSerializer.ToJsonString(data);
        #endregion

        var dataJson = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        await Response.WriteAsync($"data: {dataJson}\r\r");
        Response.Body.Flush();
    }
    #endregion

    #region AlbumInfo
    [HttpGet("GetGenreCardModels")]
    public IActionResult GetGenreCardModels() {
        var data = _ai.GenreQueries.GroupBy(gq => gq.Group)
            .Select(gr => new AlbumCardGroup {
                Name = gr.Key.ToString(),
                AlbumCms = gr.Select(gi => {
                    (var fi, var count) = GetCoverPageInfo(gi.Query);

                    return new AlbumCardModel {
                        Path = gi.Query,
                        FullTitle = gi.Name,
                        CoverInfo = fi,
                        IsRead = true,
                        IsWip = false,
                        Languages = new List<string>(),
                        LastPageIndex = 0,
                        PageCount = count,
                        Tier = 0,
                    };
                }).ToList()
            })
            .ToList();

        var censoredData = _cs.ConCensorAlbumCardGroups(data);

        return Ok(censoredData);
    }

    [HttpGet("GetFeaturedArtistCardModels")]
    public IActionResult GetFeaturedArtistCardModels() {
        var albumCMs = _ai.Tier1Artists.Select(a => {
            (var fi, var count) = GetCoverPageInfo($"artist={a}");
            return new AlbumCardModel {
                Path = $"artist={a}",
                FullTitle = a,
                CoverInfo = fi,
                PageCount = count,
                #region Useless fields
                Note = "",
                IsRead = true,
                IsWip = false,
                Languages = new List<string>(),
                LastPageIndex = 0,
                Tier = 0
                #endregion
            };
        })
        .OrderBy(a => a.FullTitle)
        .ToList();

        var data = new List<AlbumCardGroup>{
            new AlbumCardGroup {
                Name = $"Featured Artists",
                AlbumCms = albumCMs
            }
        };

        var censoredData = _cs.ConCensorAlbumCardGroups(data);

        return Ok(censoredData);
    }

    [HttpGet("GetFeaturedCharacterCardModels")]
    public IActionResult GetFeaturedCharacterCardModels() {
        var albumCMs = _ai.Characters.Select(a => {
            (var fi, var count) = GetCoverPageInfo($"character={a}");
            return new AlbumCardModel {
                Path = $"character={a}",
                FullTitle = a,
                CoverInfo = fi,
                PageCount = count,
                #region Useless fields
                Note = "",
                IsRead = true,
                IsWip = false,
                Languages = new List<string>(),
                LastPageIndex = 0,
                Tier = 0
                #endregion
            };
        })
        .OrderBy(a => a.FullTitle)
        .ToList();

        var data = new List<AlbumCardGroup>{
            new AlbumCardGroup {
                Name = $"Featured Characters",
                AlbumCms = albumCMs
            }
        };

        var censoredData = _cs.ConCensorAlbumCardGroups(data);

        return Ok(censoredData);
    }
    #endregion

    #region Album Query
    [HttpGet("GetAlbumVm")]
    public IActionResult GetAlbumVM(string path) {
        var decensoredPath = _cs.ConDecensorPath(path);
        var data = _library.GetAlbumVM(decensoredPath);

        var censoredData = _cs.ConCensorAlbumVM(data);

        return Ok(censoredData);
    }

    [HttpGet("GetAlbumCardModels")]
    public IActionResult GetAlbumCardModels(int page, int row, string query) {
        var cleanQuery = HttpUtility.UrlDecode(query);
        var decensoredQuery = _cs.ConDecensorQuery(cleanQuery);

        var albumVms = _library.GetAlbumVMs(page, row, decensoredQuery);

        var data = albumVms.Select(a => new AlbumCardModel {
            Path = a.Path,
            FullTitle = a.Album.GetFullTitleDisplay(),
            Languages = a.Album.Languages,
            IsRead = a.Album.IsRead,
            IsWip = a.Album.IsWip,
            Tier = a.Album.Tier,
            Note = a.Album.Note,
            LastPageIndex = a.LastPageIndex,
            PageCount = a.PageCount,
            EntryDate = a.Album.EntryDate,
            CorrectablePageCount = a.CorrectablePageCount,
            CoverInfo = a.CoverInfo
        })
        .ToList();

        var censoredData = _cs.ConCensorAlbumCardModels(data);

        return Ok(censoredData);
    }
    #endregion

    #region Album Command
    [HttpPost("UpdateAlbum")]
    public IActionResult UpdateAlbum(AlbumVM albumVM) {
        _auth.DisableRouteOnPublicBuild();

        string albumId = _library.UpdateAlbum(albumVM);
        return Ok(albumId);
    }

    [HttpPost("UpdateAlbumOuterValue")]
    public IActionResult UpdateAlbumOuterValue(AlbumOuterValueParam param) {
        if(_config.IsPublic || _cs.IsCensorshipOn()) return Ok();

        param.LastPageIndex = _config.IsPublic ? 0 : param.LastPageIndex;
        string albumId = _library.UpdateAlbumOuterValue(param.AlbumPath, param.LastPageIndex);
        return Ok(albumId);
    }

    [HttpPut("UpdateAlbumTier")]
    public IActionResult UpdateAlbumTier(AlbumTierParam param) {
        _auth.DisableRouteOnPublicBuild();

        _library.UpdateAlbumTier(param.AlbumPath, param.Tier);
        return Ok("Success");
    }

    [HttpGet("RecountAlbumPages")]
    public IActionResult RecountAlbumPages(string path) {
        _auth.DisableRouteOnPublicBuild();

        int pageCount = _library.RecountAlbumPages(path);
        return Ok(pageCount);
    }

    [HttpGet("RefreshAlbum")]
    public IActionResult RefreshAlbum(string path) {
        if(_config.IsPublic || _cs.IsCensorshipOn()) return Ok();

        _library.RecountAlbumPages(path);
        _library.DeleteAlbumCache(path);
        return Ok("Success");
    }

    [HttpDelete("DeleteAlbum")]
    public IActionResult DeleteAlbum(string path) {
        _auth.DisableRouteOnPublicBuild();

        _library.DeleteAlbum(path);
        _library.DeleteAlbumCache(path);
        return Ok("Success");
    }
    #endregion

    #region Page Query
    [HttpGet("GetAlbumFsNodeInfo")]
    public IActionResult GetAlbumFsNodeInfo(string path, bool includeDetail, bool includeDimension) {
        var decensoredPath = _cs.ConDecensorPath(path);

        var result = _file.GetAlbumFsNodeInfo(decensoredPath, includeDetail, includeDimension);

        var censoredResult = _cs.ConCensorAlbumFsNodeInfo(result);

        return Ok(censoredResult);
    }
    #endregion

    #region Page Command
    [HttpPost("MoveFile")]
    public IActionResult MoveFile(FileMoveParamModel param) {
        _auth.DisableRouteOnPublicBuild();

        var result = _file.MoveFile(param);
        return Ok(result);
    }

    [HttpDelete("DeleteFile")]
    public IActionResult DeleteFile(string path, string alRelPath) {
        _auth.DisableRouteOnPublicBuild();

        var libRelPath = Path.Combine(path, alRelPath);
        var result = _file.DeleteFile(libRelPath);
        return Ok(result);
    }

    [HttpDelete("DeleteAlbumChapter")]
    public IActionResult DeleteAlbumChapter(string path, string chapterName) {
        _auth.DisableRouteOnPublicBuild();

        var newPageCount = _library.DeleteAlbumChapter(path, chapterName);
        return Ok(newPageCount);
    }

    [HttpPost("UpdateAlbumChapter")]
    public IActionResult UpdateAlbumChapter(ChapterUpdateParamModel param) {
        //should only be used to update Tier
        _auth.DisableRouteOnPublicBuild();

        _file.UpdateAlbumChapter(param);

        return Ok("Success");
    }
    #endregion

    #region Library Command
    [HttpGet("ReloadDatabase")]
    public async Task ReloadDatabase() {
        _auth.DisableRouteOnPublicBuild();

        Response.Headers.Add("Content-Type", "text/event-stream");
        try {
            await _library.ReloadDatabase(WriteResponse);
        }
        catch(Exception e) {
            await WriteResponse(new EventStreamData {
                IsError = true,
                MaxStep = 0,
                CurrentStep = 0,
                Message = e.Message
            });
        }
    }

    [HttpGet("RescanDatabase")]
    public async Task RescanDatabase() {
        _auth.DisableRouteOnPublicBuild();

        Response.Headers.Add("Content-Type", "text/event-stream");
        try {
            await _library.RescanDatabase(WriteResponse);
        }
        catch(Exception e) {
            await WriteResponse(new EventStreamData {
                IsError = true,
                MaxStep = 0,
                CurrentStep = 0,
                Message = e.Message
            });
        }
    }
    #endregion
}