using Microsoft.AspNetCore.Mvc;

namespace matchmaking.Controllers;


[Route("/queue/players")]
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

    [HttpGet("")]
    public IActionResult GetPlayersInGameMode([FromQuery] string? gameMode) {
        if (String.IsNullOrWhiteSpace(gameMode)) {
            return Ok(_queueStore.Queue);
        }

        if (!_queueStore.Queue.TryGetValue(gameMode, out var gameModeData)) {
            return NotFound($"Game mode {gameMode} not found");
        }

        return Ok(gameModeData);
    }

    [HttpPost("")]
    public IActionResult Join([FromBody] JoinRequest? joinRequest) {
        if (joinRequest == null) {
            return BadRequest("Couldn't parse body");
        }

        if (joinRequest.AccessCode != null) {
            _accessCodeStore.Enqueue(joinRequest.AccessCode);
        }

        var result = _queueStore.AddToQueue(joinRequest.GameMode.ToLower(), joinRequest.PreferredRegion, joinRequest.PlayerId, joinRequest.PartySize);
        return GetResult(result, joinRequest.PlayerId);
    }

    [HttpGet("{playerId}")]
    public IActionResult Status(int playerId) {
        return GetResult(_queueStore.WaitForQueueResult(playerId), playerId);
    }

    [HttpDelete("{playerId}")]
    public IActionResult Leave(int playerId) {
        return NotFound("Not implemented");
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
