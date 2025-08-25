using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace matchmaking.Controllers;


[Route("queue")]
public class QueueController : Controller
{

    private readonly ILogger<QueueController> _logger;
    private readonly QueueStore _queueStore;

    public QueueController(
        ILogger<QueueController> logger,
        QueueStore queueStore
    )
    {
        _logger = logger;
        _queueStore = queueStore;
    }

    public IActionResult Index()
    {
        return Ok("Hello World!");
    }


    [HttpGet("{gameMode}/join")]
    public IActionResult Join(string gameMode, [FromBody] JoinRequest joinRequest)
    {
        var wait = _queueStore.AddToQueue(gameMode, joinRequest.PreferredRegion, joinRequest.PlayerIds[0], joinRequest.PlayerIds.Count);
        wait.WaitOne(1000 * 30); // Wait for 30 seconds

        if (!_queueStore.PlayerResults.ContainsKey(joinRequest.PlayerIds[0]))
        {
            return Created();
        }
        
        if (!_queueStore.PlayerResults.TryRemove(joinRequest.PlayerIds[0], out var access_code))
        {
            return StatusCode(500, "Match created but could not remove access code from dictionary");
        }

        return Ok(new { access_code = access_code });
    }
}
