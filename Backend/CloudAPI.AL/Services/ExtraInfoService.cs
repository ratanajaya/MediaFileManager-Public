using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using CloudAPI.AL.Models.ExtraDb;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.Services;

public class ExtraInfoService
{
    ILogger _logger;
    IExtraDbContext _extraDb;

    public ExtraInfoService(ILogger logger, IExtraDbContext extraDb) {
        _logger = logger;
        _extraDb = extraDb;
    }

    public List<ScrapOperation> GetScrapOperations(string albumPath) {
        try {
            return _extraDb.GetScrapOperations(albumPath);
        }
        catch(Exception e) {
            _logger.Error($"ExtraInfoServices.GetScrapOperations{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public ScrapOperation InsertScrapOperation(ScrapOperationParamModel param) {
        try {
            var item = new ScrapOperation {
                AlbumPath = param.AlbumPath,
                Source = param.Source,
                Status = ScrapOperation.OpStatus.Pending,
                CreateDate = DateTime.Now,
            };
            _extraDb.InsertScrapOperation(item);

            return item;
        }
        catch(Exception e) {
            _logger.Error($"ExtraInfoServices.InsertScrapOperation{Environment.NewLine}" +
                $"Params=[{JsonConvert.SerializeObject(param)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public ScrapOperation UpdateScrapOperation(ScrapOperationParamModel param) {
        var existing = _extraDb.GetScrapOperation(param.Id);

        existing.Source = param.Source;
        existing.Status = ScrapOperation.OpStatus.Pending;

        _extraDb.UpdateScrapOperation(existing);
        return existing;
    }

    public List<Comment> GetComments(int scrapOperationId) {
        try {
            return _extraDb.GetComments(scrapOperationId);
        }
        catch(Exception e) {
            _logger.Error($"ExtraInfoServices.GetComments{Environment.NewLine}" +
                $"Params=[{scrapOperationId}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void OneTimeMigration() {
        _extraDb.OneTimeMigration();
    }
}
