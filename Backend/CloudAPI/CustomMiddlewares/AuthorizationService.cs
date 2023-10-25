using CloudAPI.AL.Models;
using CloudAPI.Services;
using System;

namespace CloudAPI.CustomMiddlewares;

public class AuthorizationService
{
    ConfigurationModel _config;
    CensorshipService _cs;

    public AuthorizationService(ConfigurationModel config, CensorshipService cs) {
        _config = config;
        _cs = cs;
    }

    public void DisableRouteOnPublicBuild() {
        if(_config.IsPublic){
            throw new UnauthorizedAccessException("Data changing operations are disabled in public build. Thank you 👍");
        }

        if(_cs.IsCensorshipOn())
            throw new UnauthorizedAccessException("Data changing operations are disabled when censorship is on");
    }
}