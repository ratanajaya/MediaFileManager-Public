using MetadataEditor.AL;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleInjector;
using System.Net.Http;
using System.Configuration;
using CloudAPI.AL.Services;
using Serilog;
using System.IO;
using MetadataEditor.AL.Models;
using MetadataEditor.AL.Services;
using Microsoft.Extensions.Caching.Memory;
using CloudAPI.AL.DataAccess;
using System.Runtime.Versioning;

namespace MetadataEditor;

[SupportedOSPlatform("windows")]
static class Program
{
    static readonly Container container;

    static Program() {
        container = new Container();
    }

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        container.Register<IAppLogic, AppLogic>(Lifestyle.Singleton);
        container.Register<ISystemIOAbstraction, SystemIOAbstraction>(Lifestyle.Singleton);
        container.Register<IAlbumInfoProvider, AlbumInfoProvider>(Lifestyle.Singleton);
        container.Register<FormMain>(Lifestyle.Singleton);
        var configuration = new ConfigurationModel {
            BrowsePath = ConfigurationManager.AppSettings["BrowsePath"],
            Args = args
        };
        container.RegisterInstance(configuration);

        #region Services from API project
        container.Register<LibraryRepository>(Lifestyle.Singleton);
        container.Register<FileRepository>(Lifestyle.Singleton);
        container.Register<MediaProcessor>(Lifestyle.Singleton);
        container.Register<IDbContext, JsonDbContext>(Lifestyle.Singleton);
        container.Register<ILogDbContext, LogDbContext>(Lifestyle.Singleton);

        var apiConf = new CloudAPI.AL.Models.ConfigurationModel {
            LibraryPath = ConfigurationManager.AppSettings["LibraryPath"],
            TempPath = ConfigurationManager.AppSettings["TempPath"],
            BuildType = "Private"
        };
        container.RegisterInstance(apiConf);
        var logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serilog/log-.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
        container.RegisterInstance<ILogger>(logger);
        #endregion

        container.RegisterInstance<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

        container.Verify();

        Application.Run(container.GetInstance<FormMain>());
    }
}