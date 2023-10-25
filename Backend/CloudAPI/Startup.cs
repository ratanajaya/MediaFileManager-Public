using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary;
using Serilog;
using CloudAPI.CustomMiddlewares;
using CloudAPI.AL.Models;
using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Services;
using Microsoft.ApplicationInsights.Extensibility;
using CloudAPI.Services;
using Microsoft.OpenApi.Models;

namespace CloudAPI;

public class Startup
{
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
        services.AddControllers();

        services.AddCors(o => o.AddPolicy("MyPolicy", builder => {
            builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
        }));

        services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WKR", Version = "v1" });
        });
        services.AddMemoryCache();

        #region Services
        services.AddScoped<AuthorizationService>();
        services.AddScoped<FileRepository>();
        services.AddScoped<LibraryRepository>();
        services.AddScoped<ScRepository>();
        services.AddScoped<MediaProcessor>();
        services.AddScoped<ISystemIOAbstraction, SystemIOAbstraction>();
        services.AddScoped<ILogDbContext, LogDbContext>();
        services.AddScoped<IPcService, PcService>();
        services.AddScoped<IExtraDbContext, ExtraDbContext>();
        services.AddScoped<ExtraInfoService>();
        services.AddScoped<StaticInfoService>();
        services.AddScoped<ImageProcessor>();
        services.AddScoped<OperationService>();

        services.AddScoped<IDbContext, JsonDbContext>();
        services.AddScoped<CensorshipService>();
        services.AddScoped<IAlbumInfoProvider, AlbumInfoProvider>();
        services.AddScoped<FakeAlbumInfoProvider>();
        services.AddSingleton(new Random());

        var config = new ConfigurationModel {
            LibraryPath = Configuration.GetValue<string>("LibraryPath"),
            ScLibraryPath = Configuration.GetValue<string>("ScLibraryPath"),
            TempPath = Configuration.GetValue<string>("TempPath"),

            Version = Configuration.GetValue<string>("Version"),
            BuildType = Configuration.GetValue<string>("BuildType"),
        };
        services.AddSingleton(config);

        var logger = new Func<ILogger>(() => {
            if(config.IsPublic) {
                return new LoggerConfiguration()
                    .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                    .CreateLogger();
            }
            else {
                return new LoggerConfiguration()
                    .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serilog/log-.txt"), rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
        })();
            
        services.AddSingleton<ILogger>(logger);
        #endregion
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        app.UseCors("MyPolicy");

        app.UseRouting();

        app.UseAuthorization();

        app.UseMiddleware<CustomExceptionMiddleware>();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });

        if(env.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}