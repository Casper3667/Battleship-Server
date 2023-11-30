using GameServer.StaticContent;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class ServerHealth : ControllerBase
{
    private readonly ILogger<ServerHealth>? _logger;

    public ServerHealth(ILogger<ServerHealth> logger)
    {
        _logger = logger;
    }
    public ServerHealth()
    {
    }

    [HttpGet(Name = "status")]
    public int Get()
    {
        return PlayerCount.Players;
    }
}
