using CloudAPI.AL.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI.Controllers.Abstraction;

public interface ILibraryController
{
    IActionResult GetAlbumVM(string path);
    IActionResult GetAlbumCardModels(int page, int row, string query);
    IActionResult GetGenreCardModels();
    IActionResult GetFeaturedArtistCardModels();

    IActionResult UpdateAlbum(AlbumVM albumVM);
    IActionResult UpdateAlbumOuterValue(AlbumOuterValueParam param);
    IActionResult UpdateAlbumTier(AlbumTierParam param);
    IActionResult RecountAlbumPages(string path);
    IActionResult RefreshAlbum(string path);
    IActionResult DeleteAlbum(string path);

    IActionResult GetAlbumFsNodeInfo(string path, bool includeDetail, bool includeDimension);

    IActionResult DeleteFile(string path, string alRelPath);
    IActionResult MoveFile(FileMoveParamModel param);
    IActionResult DeleteAlbumChapter(string path, string chapterName);

    Task ReloadDatabase();
    Task RescanDatabase();
}