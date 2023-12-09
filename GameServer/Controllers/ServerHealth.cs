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
    /// <summary>
    /// This Code Caused Program to Fail
    /// </summary>
    /// <returns></returns>
    //public ServerHealth()
    //{
    //}

    [HttpGet(Name = "status")]
    public int Get()
    {
        return PlayerCount.Players;
    }
}
