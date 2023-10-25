using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkrUnitTest;

public class MockProvider
{
    public static ConfigurationModel GetConfig() {
        return new ConfigurationModel {
            LibraryPath = "Z:\\Test Library",
            ScLibraryPath = "Z:\\SC Test Library",
            BuildType = "Private",
        };
    }

    public static Mock<IDbContext> GetIDbContext() {
        var mock = new Mock<IDbContext>();

        mock.Setup(a => a.AlbumVMs).Returns(_albumVMs);

        return mock;
    }

    #region Values
    public static List<AlbumVM> _albumVMs = new List<AlbumVM>() {
            new AlbumVM {
                Path = "ABC\\[Artist1] Title1"
            },
            new AlbumVM {
                Path = "ABC\\[Artist2] Title2"
            }
        };
    #endregion
}
