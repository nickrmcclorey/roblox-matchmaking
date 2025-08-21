using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Matchmaking.Models;

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
    public IActionResult Join(string gameMode, [FromBody] JoinRequest? joinRequest)
    {
        if (joinRequest == null)
        {
            return BadRequest("Couldn't parse body");
        }

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
    
    // TODO: move this to QueueStore
    private IActionResult WaitForQueueResult(int playerId) {

        // When the matchmaker creates a game, it puts the result in PlayerResults BEFORE removing the CancellationToken
        // It's important to check the Cancellation token before checking PlayerResults to avoid a race condition
        if (!_queueStore.CancellationTokens.TryGetValue(playerId, out AutoResetEvent? wait)) {
            if (!_queueStore.PlayerResults.TryGetValue(playerId, out DatedValue<string>? accessCode)) {
                return NotFound($"Player {playerId} not found in queue");
            }
            return Ok(new { access_code = accessCode.Value });
        }

        wait.WaitOne(1000 * 30);
        if (!_queueStore.PlayerResults.ContainsKey(playerId)) {
            return CreatedAtAction(nameof(Status), new { playerId }, new { message = "Matchmaking in progress" });
        }

        if (!_queueStore.PlayerResults.TryRemove(playerId, out var access_code)) {
            return StatusCode(500, "Match created but could not remove access code from dictionary");
        }

        return Ok(new { access_code = access_code });
    }
}
