using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace matchmaking.Controllers;


[Route("queue")]
public class QueueController : Controller {

    private readonly ILogger<QueueController> _logger;
    private readonly QueueStore _queueStore;
    private readonly AccessCodeStore _accessCodeStore;

    public QueueController(
        ILogger<QueueController> logger,
        QueueStore queueStore,
        AccessCodeStore accessCodeStore
    ) {
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

        _queueStore.AddToQueue(gameMode, joinRequest.PreferredRegion, joinRequest.PlayerId, joinRequest.PartySize);
        return WaitForQueueResult(joinRequest.PlayerId);
    }

    [HttpGet("status/{playerId}")]
    public IActionResult Status(int playerId) {
        return WaitForQueueResult(playerId);
    }
    
    private IActionResult WaitForQueueResult(int playerId) {

        if (!_queueStore.CancellationTokens.ContainsKey(playerId)) {
            return NotFound($"Player {playerId} not found in queue");
        }

        var wait = _queueStore.CancellationTokens[playerId];
        wait.WaitOne(1000 * 30);
        if (!_queueStore.PlayerResults.ContainsKey(playerId)) {
            return Created();
        }

        if (!_queueStore.PlayerResults.TryRemove(playerId, out var access_code)){
            return StatusCode(500, "Match created but could not remove access code from dictionary");
        }

        return Ok(new { access_code = access_code });
    }
}
