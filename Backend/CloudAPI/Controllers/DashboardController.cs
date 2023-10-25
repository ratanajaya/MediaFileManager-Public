using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using CloudAPI.AL.Models.Dashboard;
using CloudAPI.AL.Services;
using CloudAPI.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudAPI.Controllers;

[ApiController]
[Route("Dashboard")]
public class DashboardController : ControllerBase
{
    IAlbumInfoProvider _ai;
    FakeAlbumInfoProvider _fai;
    ConfigurationModel _config;
    LibraryRepository _library;
    ILogDbContext _sqlite;
    CensorshipService _cs;

    public DashboardController(IAlbumInfoProvider ai, FakeAlbumInfoProvider fai, ConfigurationModel config, LibraryRepository library, ILogDbContext sqlite, CensorshipService cs) {
        _ai = ai;
        _fai = fai;
        _config = config;
        _library = library;
        _sqlite = sqlite;
        _cs = cs;
    }

    private TierFractionModel GetTierFractionFromQuery(string query, string name) {
        var albums = _library.GetAlbumVMs(0, 0, query);

        //Optimized code
        int[] tc = new int[6];
        foreach(var album in albums) {
            if(!album.Album.IsRead)
                tc[5]++;
            else if(album.Album.Tier < 3)
                tc[album.Album.Tier]++;
            else if(album.Album.Note != "🌟")
                tc[3]++;
            else
                tc[4]++;
        }
        return new TierFractionModel {
            Name = name,
            Query = query,
            T0 = tc[0],
            T1 = tc[1],
            T2 = tc[2],
            T3 = tc[3],
            Ts = tc[4],
            Tn = tc[5],
        };
    }

    [HttpGet("GetQueryTierFraction")]
    public IActionResult GetQueryTierFractions(string query) {
        var decensoredQuery =_cs.ConDecensorQuery(query);

        var data = GetTierFractionFromQuery(decensoredQuery, decensoredQuery);

        var censoredData = _cs.ConCensorTierFractionModel(data);

        return Ok(censoredData);
    }

    [HttpGet("GetGenreTierFractions")]
    public IActionResult GetGenreTierFractions() {
        IAlbumInfoProvider ai = _config.IsPrivate ? _ai : _fai;

        var data = ai.GenreQueries.Where(a => a.Group != 0).Select(a => {
            return GetTierFractionFromQuery(a.Query, a.Name);
        }).ToList();

        var censoredData = _cs.ConCensorTierFractionModels(data);

        return Ok(censoredData);
    }

    [HttpGet("GetLogs")]
    public IActionResult GetLogs(int page, int row, string operation, string freeText, DateTime? startDate, DateTime? endDate) {
        var data = _sqlite.GetLogs(page, row, operation, freeText, startDate, endDate);

        var result = new TablePaginationModel<LogDashboardModel> {
            TotalPage = data.TotalPage,
            TotalItem = data.TotalItem,
            Records = data.Records.Select(a => new LogDashboardModel {
                Id = a.Id.ToString(),
                AlbumFullTitle = a.AlbumFullTitle,
                CreationTime = a.CreateDate,
                Operation = a.Operation
            }).ToList()
        };

        result.Records = _cs.ConCensorLogDashboardModels(result.Records);

        return Ok(result);
    }

    [HttpGet("GetDeleteLogs")]
    public IActionResult GetDeleteLogs(string query, bool? includeAlbum) {
        var logs = _sqlite.GetDeleteLogs(query);

        var data = logs.Select(a => new LogDashboardModel {
            Id = a.Id.ToString(),
            AlbumFullTitle = a.AlbumFullTitle,
            CreationTime = a.CreateDate,
            Operation = a.Operation,
            Album = includeAlbum.GetValueOrDefault() ? Utf8Json.JsonSerializer.Deserialize<Album>(a.AlbumJson) : null
        }).ToList();

        var censoredData = _cs.ConCensorLogDashboardModels(data);

        return Ok(censoredData);
    }

    #region LEGACY
    //[HttpGet("GetAlbums")]
    //public IActionResult GetAlbums() {
    //    var data = _config.IsPrivate
    //        ? _library.GetAlbumVMs(0, 0, null)
    //        : _pub.GetAlbumVMs(0, 0, null);

    //    var result = data.Select(a => new AlbumDashboardModel {
    //        FullTitle = a.Album.GetFullTitleDisplay(),
    //        IsRead = a.Album.IsRead,
    //        IsWip = a.Album.IsWip,
    //        Languages = a.Album.Languages,
    //        Note = a.Album.Note ?? string.Empty,
    //        PageCount = a.PageCount,
    //        Tier = a.Album.Tier,
    //        CoverInfo = a.CoverInfo
    //    }).ToList();

    //    return Ok(result);
    //}
    #endregion
}