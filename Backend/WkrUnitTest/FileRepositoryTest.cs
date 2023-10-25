#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Models;
using CloudAPI.AL.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using SharedLibrary;

namespace WkrUnitTest;

[TestClass]
public class FileRepositoryTest
{
    FileRepository _fileRepo;
    LibraryRepository _libraryRepo;
    Mock<IDbContext> _dbMock;
    MediaProcessor _media;
    Mock<ISystemIOAbstraction> _ioMock;

    [TestInitialize]
    public void Initialize() {
        var config = new ConfigurationModel {
            
        };
        var ai = new AlbumInfoProvider();
        _dbMock = new Mock<IDbContext>();
        _ioMock = new Mock<ISystemIOAbstraction>();

        //_libraryRepo = new LibraryRepository(); //TODO
        _media = new MediaProcessor(
            _ioMock.Object,
            ai,
            new Mock<ILogger>().Object
        );

        _fileRepo = new FileRepository(
            config, 
            ai,
            _ioMock.Object,
            _dbMock.Object,
            _libraryRepo,
            new Mock<ILogger>().Object,
            _media
        );
    }

    //[TestMethod]
    //public void GetFullCachedPath() {
    //    Assert.Fail();
    //}
}
