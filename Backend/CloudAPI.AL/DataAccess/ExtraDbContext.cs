using CloudAPI.AL.Models;
using CloudAPI.AL.Models.ExtraDb;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudAPI.AL.DataAccess;

public interface IExtraDbContext
{
    List<Comment> GetComments(int scrapOperationId);
    ScrapOperation GetScrapOperation(string albumPath, string source);
    ScrapOperation GetScrapOperation(int id);
    List<ScrapOperation> GetScrapOperations(string albumPath);
    void InsertScrapOperation(ScrapOperation param);
    void UpdateScrapOperation(ScrapOperation param);
    void OneTimeMigration();
}

public class ExtraDbContext : IExtraDbContext
{
    ConfigurationModel _config;

    public ExtraDbContext(ConfigurationModel config) {
        _config = config;
    }

    public ScrapOperation GetScrapOperation(int id) {
        using(var db = new SQLiteConnection(_config.FullExtraDbPath)) {
            return db.Table<ScrapOperation>()
                .FirstOrDefault(a => a.Id == id);
        }
    }

    public ScrapOperation GetScrapOperation(string albumPath, string source) {
        using(var db = new SQLiteConnection(_config.FullExtraDbPath)) {
            return db.Table<ScrapOperation>()
                .FirstOrDefault(a => a.AlbumPath == albumPath && a.Source == source);
        }
    }

    public List<ScrapOperation> GetScrapOperations(string albumPath) {
        using(var db = new SQLiteConnection(_config.FullExtraDbPath)) {
            var result = db.Table<ScrapOperation>()
                .Where(a => a.AlbumPath == albumPath)
                .ToList();

            return result;
        }
    }

    public void InsertScrapOperation(ScrapOperation param) {
        using(var db = new SQLiteConnection(_config.FullExtraDbPath)) {
            var existing = db.Table<ScrapOperation>()
                .FirstOrDefault(a => a.AlbumPath == param.AlbumPath && a.Source == param.Source);

            if(existing != null) {
                throw new Exception("Source already exist");
            }

            db.Insert(param);
        }
    }

    public void UpdateScrapOperation(ScrapOperation param) {
        using(var db = new SQLiteConnection(_config.FullExtraDbPath)) {
            db.Update(param);
        }
    }

    public List<Comment> GetComments(int scrapOperationId) {
        using(var db = new SQLiteConnection(_config.FullExtraDbPath)) {
            return db.Table<Comment>()
                .Where(a => a.ScrapOperationId == scrapOperationId)
                .ToList();
        }
    }

    public void OneTimeMigration() {
        using(var db = new SQLiteConnection(_config.FullExtraDbPath)) {
            db.CreateTable<ScrapOperation>();
            db.CreateTable<Comment>();
        }
    }
}
