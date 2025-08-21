using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Matchmaking.Models;
using System.Numerics;

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
    public IActionResult Join(string gameMode, [FromBody] JoinRequest? joinRequest) {
        if (joinRequest == null) {
            return BadRequest("Couldn't parse body");
        }

        if (joinRequest.AccessCode != null) {
            _accessCodeStore.Enqueue(joinRequest.AccessCode);
        }

        var result = _queueStore.AddToQueue(gameMode.ToLower(), joinRequest.PreferredRegion, joinRequest.PlayerId, joinRequest.PartySize);
        return GetResult(result, joinRequest.PlayerId);
    }

    [HttpGet("status/{playerId}")]
    public IActionResult Status(int playerId) {
        return GetResult(_queueStore.WaitForQueueResult(playerId), playerId);
    }

    private IActionResult GetResult(WaitResult result, int playerId) {
        if (result.Status == WaitResult.ResultType.Error) {
            return StatusCode(500, result.Message);
        } else if (result.Status == WaitResult.ResultType.BadRequest) {
            return BadRequest(result.Message);
        } else if (result.Status == WaitResult.ResultType.StillWaiting) {
            return CreatedAtAction(nameof(Status), new { playerId }, new { message = "Matchmaking in progress" });
        } else if (result.Status == WaitResult.ResultType.Ready) {
            return Ok(new { access_code = result.AccessCode });
        } else {
            return StatusCode(500, "Unknown result type");
        }
    }
}
