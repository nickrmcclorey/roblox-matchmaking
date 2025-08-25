using Microsoft.AspNetCore.Mvc;

public class GameModeController : Controller {

    private readonly ILogger<GameModeController> _logger;
    private readonly QueueStore _queueStore;

    public GameModeController(
        ILogger<GameModeController> logger,
        QueueStore queueStore
    ) {
        _logger = logger;
        _queueStore = queueStore;
    }

    [HttpGet("gamemodes")]
    public IActionResult GetGameModes() {
        var gameModes = _queueStore.Queue.Values.ToList();
        return Ok(gameModes);
    }

    [HttpGet("gamemodes/{gameMode}/regions")]
    public IActionResult GetRegions(string gameMode) {
        if (!_queueStore.Queue.TryGetValue(gameMode, out var gameModeData)) {
            return NotFound($"Game mode {gameMode} not found");
        }
        return Ok(gameModeData.Keys.ToList());
    }

    [HttpGet("gamemodes/regions")]
    public IActionResult GetAllRegions() {
        var regions = new HashSet<string>();
        foreach (var gameMode in _queueStore.Queue.Values) {
            foreach (var region in gameMode.Keys) {
                regions.Add(region);
            }
        }
        return Ok(regions);
    }

}