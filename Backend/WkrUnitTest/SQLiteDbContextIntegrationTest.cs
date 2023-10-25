#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using CloudAPI.AL.DataAccess;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkrUnitTest;

//Integration test, requieres the real DB
[TestClass]
public class SQLiteDbContextIntegrationTest
{
    LogDbContext _db;

    [TestInitialize]
    public void Initialize() {
        _db = new LogDbContext(
            config: new CloudAPI.AL.Models.ConfigurationModel {
                LibraryPath = "Z:\\H Library"
            },
            cache: new Mock<IMemoryCache>().Object
        );
    }

    [TestMethod]
    public void GetLogs_Where() {
        var res = _db.GetLogs(1, 10, "", "fuusen", new DateTime(2023, 1, 20), null);

        Assert.IsTrue(res.Records.Any());
    }

    [TestMethod]
    public void GetLogs_PageRow() {
        var res = _db.GetLogs(10, 10, "", "", null, null);

        Assert.IsTrue(res.Records.Any());
    }
}
