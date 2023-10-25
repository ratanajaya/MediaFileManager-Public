using CloudAPI.AL.Models;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Services;

public class StaticInfoService
{
    IAlbumInfoProvider _ai;

    public StaticInfoService(IAlbumInfoProvider ai) {
        _ai = ai;
    }

    public AlbumInfoVm GetAlbumInfoVm() {
        return new AlbumInfoVm {
            Tags = _ai.Tags,
            Characters = _ai.Characters,
            Categories = _ai.Categories,
            Orientations = _ai.Orientations,
            Languages = _ai.Languages,
            SuitableImageFormats = _ai.SuitableImageFormats,
            SuitableVideoFormats = _ai.SuitableVideoFormats
        };
    }

    public List<QueryVM> GetTagVMs() {
        return _ai.Tags.Select(t => new QueryVM {
            Name = t,
            Tier = 0,
            Query = "tag:" + t
        }).ToList();
    }

    public List<KeyValuePair<int, string>> GetUpscalers() {
        return new List<KeyValuePair<int, string>> {
            new((int)UpscalerType.Waifu2xCunet, "Waifu2x Cunet"),
            new((int)UpscalerType.Waifu2xAnime, "Waifu2x Anime"),

            new((int)UpscalerType.SrganD2fkJpeg, "SRGAN D2FK Jpeg"),

            new((int)UpscalerType.RealesrganX4plus, "RealESRGAN X4 Plus"),
            new((int)UpscalerType.RealesrganX4plusAnime, "RealESRGAN X4 Plus Anime"),
        };
    }
}
