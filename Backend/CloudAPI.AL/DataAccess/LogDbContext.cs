using CloudAPI.AL.Models;
using CloudAPI.AL.Models.Dashboard;
using CloudAPI.AL.Models.LogDb;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary;
using SharedLibrary.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qh = CloudAPI.AL.Helpers.QueryHelpers;

namespace CloudAPI.AL.DataAccess;

public interface ILogDbContext
{
    CorrectionLog GetCorrectionLog(string path);
    List<CorrectionLog> GetCorrectionLogs(List<string> paths);
    List<CrudLog> GetDeleteLogs(string query);
    DateTime? GetLastCorrectionTime(string path);
    TablePaginationModel<CrudLog> GetLogs(int page, int row, string operation, string freeText, DateTime? startDate, DateTime? endDate);
    void InsertCrudLog(string operation, AlbumVM avm);
    bool IsCensorshipOn();
    bool IsLastModifiedByThisApp();
    [Obsolete]
    void OneTimeMigration();
    void UpdateCensorshipStatus(bool value);
    void UpdateCorrectionLog(string path, DateTime? correctionDate, int correctablePageCount);
    void UpdateLastModified();
}

public class LogDbContext : ILogDbContext
{
    ConfigurationModel _config;
    private IMemoryCache _cache;

    public LogDbContext(ConfigurationModel config, IMemoryCache cache) {
        _config = config;
        _cache = cache;
    }

    private void InsertUpdateKeyValue(string key, string val) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var kv = db.Table<KeyValue>().FirstOrDefault(a => a.Key == key);

            if(kv != null) {
                kv.Value = val;
                db.Update(kv);
            }
            else {
                db.Insert(new KeyValue {
                    Key = key,
                    Value = val
                });
            }
        }
    }

    public List<CrudLog> GetDeleteLogs(string query) {
        var querySegments = qh.GetQuerySegments(query);

        var deleteLogs = GetLogs(0, 0, CrudLog.Delete, null, null, null);
        var deleteLogsByQuery = deleteLogs.Records
            .Where(a => qh.MatchAllQueries(Utf8Json.JsonSerializer.Deserialize<Album>(a.AlbumJson), querySegments, new string[0], new string[0]))
            .ToList();

        return deleteLogsByQuery;
    }

    public TablePaginationModel<CrudLog> GetLogs(int page, int row, string operation, string freeText, DateTime? startDate, DateTime? endDate) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var lowerFreeText = !string.IsNullOrEmpty(freeText) ? freeText.ToLower() : null;

            var qWhere = new Func<string>(() => {
                var wFt = string.IsNullOrEmpty(lowerFreeText) ? "" :
                    $" AND LOWER(AlbumFullTitle) LIKE '%{lowerFreeText}%'";
                var wOp = string.IsNullOrEmpty(operation) ? "" :
                    $" AND Operation == '{operation}'";
                var wSd = !startDate.HasValue ? "" :
                    $" AND CreateDate >= {startDate.Value.Ticks}";
                var wEd = !endDate.HasValue ? "" :
                    $" AND CreateDate <= {endDate.Value.Ticks}";

                return $"WHERE 0=0{wFt}{wOp}{wSd}{wEd}";
            })();

            var total = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM CrudLog {qWhere}");
            var totalPage = row > 0 ? ((total - 1) / row) + 1 : 1;

            var qOrder = $"ORDER BY CreateDate DESC";

            var qLimit = (page > 0 && row > 0) ? $"LIMIT {row} OFFSET {(page - 1) * row}" : "";

            var records = db.Query<CrudLog>($"SELECT * FROM CrudLog {qWhere} {qOrder} {qLimit}");

            return new TablePaginationModel<CrudLog> {
                Records = records,
                TotalItem = total,
                TotalPage = totalPage
            };
        }
    }

    public void InsertCrudLog(string operation, AlbumVM avm) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            db.Insert(new CrudLog(operation, avm.Path, avm.Album, DateTime.Now));
        }
    }

    public bool IsCensorshipOn() {
        var cachedVal = false;
        if(_cache.TryGetValue(Constants.Kc_CensorshipStatus, out cachedVal)) {
            return cachedVal;
        }
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var kv = db.Table<KeyValue>().FirstOrDefault(a => a.Key == KeyValue.KeyCensorshipStatus);
            var val = kv?.Value == KeyValue.OnOff_On;
            _cache.Set(Constants.Kc_CensorshipStatus, val);
            return val;
        }
    }

    public void UpdateCensorshipStatus(bool value) {
        InsertUpdateKeyValue(KeyValue.KeyCensorshipStatus, value ? KeyValue.OnOff_On : KeyValue.OnOff_Off);
        _cache.Remove(Constants.Kc_CensorshipStatus);
    }

    public bool IsLastModifiedByThisApp() {
        var cachedVal = false;
        if(_cache.TryGetValue(Constants.Kc_LastModified, out cachedVal)) {
            return cachedVal;
        }
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var kv = db.Table<KeyValue>().FirstOrDefault(a => a.Key == KeyValue.KeyLastModified);
            var val = kv?.Value == _config.AppType;
            _cache.Set(Constants.Kc_LastModified, val);
            return val;
        }
    }

    public void UpdateLastModified() {
        InsertUpdateKeyValue(KeyValue.KeyLastModified, _config.AppType);
        _cache.Remove(Constants.Kc_LastModified);
    }

    public DateTime? GetLastCorrectionTime(string path) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var entity = db.Table<CorrectionLog>().FirstOrDefault(a => a.Path == path);
            return entity?.LastCorrectionDate;
        }
    }

    public CorrectionLog GetCorrectionLog(string path) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var entity = db.Table<CorrectionLog>().FirstOrDefault(a => a.Path == path);
            return entity;
        }
    }

    public List<CorrectionLog> GetCorrectionLogs(List<string> paths) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var entities = db.Table<CorrectionLog>().Where(a => paths.Contains(a.Path)).ToList();
            return entities;
        }
    }

    public void UpdateCorrectionLog(string path, DateTime? correctionDate, int correctablePageCount) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var cl = db.Table<CorrectionLog>().FirstOrDefault(a => a.Path == path);

            if(cl != null) {
                if(correctionDate != null)
                    cl.LastCorrectionDate = correctionDate.Value;

                cl.CorrectablePageCount = correctablePageCount;
                db.Update(cl);
            }
            else {
                db.Insert(new CorrectionLog {
                    Path = path,
                    LastCorrectionDate = correctionDate.GetValueOrDefault(),
                    CorrectablePageCount = correctablePageCount
                });
            }
        }
    }

    [Obsolete]
    public void OneTimeMigration() {
        //using(var ldb = new LiteDB.LiteDatabase(new LiteDB.ConnectionString { Filename = _config.FullLiteDbPath, Connection = LiteDB.ConnectionType.Direct, ReadOnly = false }))
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            //db.CreateTable<KeyValue>();
            //db.CreateTable<CrudLog>();
            db.CreateTable<CorrectionLog>();

            //var col = ldb.GetCollection<Models.LiteDb.CrudLog>();
            //var liteCrudLogs = col.Query().ToList();

            //var crudLogs = liteCrudLogs.Select(a => new CrudLog {
            //    Operation = a.Operation,
            //    CreateDate = a.Id.CreationTime,
            //    AlbumFullTitle = a.AlbumFullTitle,
            //    AlbumPath = a.AlbumPath,
            //    AlbumJson = Utf8Json.JsonSerializer.ToJsonString(a.Album)
            //}).ToList();

            //db.InsertAll(crudLogs, true);
        }
    }
}
