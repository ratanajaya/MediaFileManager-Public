using CloudAPI.AL.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace CloudAPI.Controllers;

[ApiController]
[Route("Pc")]
public class PcController : ControllerBase {
    IPcService _pc;

    public PcController(IPcService pc) {
        _pc = pc;
    }

    [HttpPost("Sleep")]
    public IActionResult Sleep() {
        _pc.Sleep();
        return Ok();
    }

    [HttpPost("Hibernate")]
    public IActionResult Hibernate() {
        _pc.Hibernate();
        return Ok();
    }
}
