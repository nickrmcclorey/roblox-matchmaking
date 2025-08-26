using Microsoft.AspNetCore.Mvc;

[Route("games")]
public class UnfilledGameController : Controller {

    private readonly UnfilledGamesStore _unfilledGamesStore;

    public UnfilledGameController(UnfilledGamesStore unfilledGamesStore) {
        _unfilledGamesStore = unfilledGamesStore;
    }

    [HttpPost("{gameModeKey}/fill")]
    public void AddUnfilledGame(string gameModeKey, [FromBody] UnfilledGameRequest unfilledGame) {
        _unfilledGamesStore.Add(new UnfilledGame {
            gameMode = gameModeKey.ToLower(),
            AccessCode = unfilledGame.AccessCode,
            ExtraPlayersNeeded = unfilledGame.ExtraPlayersNeeded
        });
    }

    [HttpGet("unfilled")]
    public IActionResult AddUnfilledGames() {
        return Ok(_unfilledGamesStore.Values);
    }

}