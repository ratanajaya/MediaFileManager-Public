using CloudAPI.AL.Models;
using CloudAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace CloudAPI.Controllers;

[ApiController]
[Route("")]
public class TestController : ControllerBase
{
    public ConfigurationModel _config;
    public ILogger _logger;
    public CensorshipService _cs;

    public TestController(ConfigurationModel config, ILogger logger, CensorshipService cs) {
        _config = config;
        _logger = logger;
        _cs = cs;
    }

    [HttpGet]
    public IActionResult GetJson() {
        var liteDbStatus = new Func<(bool, bool?, string)>(() => {
            try {
                var useCensorship = _cs.IsCensorshipOn();

                return (true, useCensorship, "LiteDb is accessible");
            }
            catch(Exception e) {
                return (false, null, e.Message);
            }
        })();
        

        return Ok(new { 
            Message= "API is online",
            Version = _config.Version,
            BuildType = _config.BuildType,
            LibraryPath = _config.LibraryPath,
            LiteDbStatus = new { 
               Accessible = liteDbStatus.Item1,
               UseCensorship = liteDbStatus.Item2,
               Message = liteDbStatus.Item3
            }
        });
    }

    [HttpGet("Wololo")]
    public IActionResult Wololo() {
        _logger.Warning("Wololo triggered");

        return Ok(new {
            Message = "Wololo"
        });
    }
}