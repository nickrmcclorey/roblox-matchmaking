using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace matchmaking.Controllers;


[Route("queue")]
public class QueueController : Controller
{

    private readonly ILogger<QueueController> _logger;
    private readonly QueueStore _queueStore;
    private readonly AccessCodeStore _accessCodeStore;

    public QueueController(
        ILogger<QueueController> logger,
        QueueStore queueStore,
        AccessCodeStore accessCodeStore
    )
    {
        _logger = logger;
        _queueStore = queueStore;
        _accessCodeStore = accessCodeStore;
    }


    [HttpPost("{gameMode}/join")]
    public IActionResult Join(string gameMode, [FromBody] JoinRequest joinRequest)
    {

        if (joinRequest.AccessCode != null)
        {
            _accessCodeStore.Enqueue(joinRequest.AccessCode);
        }

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

    [HttpGet("{gameMode}/status/{playerId}")]
    public IActionResult Status(string gameMode, int playerId)
    {
        _queueStore.CancellationTokens[playerId].WaitOne(1000 * 30); // Wait for 30 seconds
        _queueStore.Queue[gameMode]["na"][0].Contains(playerId);
        return Ok();
    }
}
