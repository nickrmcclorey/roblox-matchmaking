using Microsoft.AspNetCore.Mvc;

[Route("/queue/servers")]
public class UnfilledGameController : Controller {

    private readonly UnfilledGamesStore _unfilledGamesStore;

    public UnfilledGameController(UnfilledGamesStore unfilledGamesStore) {
        _unfilledGamesStore = unfilledGamesStore;
    }

    [HttpGet("")]
    public IActionResult UnfilledGames() {
        return Ok(_unfilledGamesStore.Values);
    }

    [HttpPost("")]
    public IActionResult AddUnfilledGame([FromBody] UnfilledGame unfilledGame) {
        if (!unfilledGame.GameMode.Contains('-') || !Int32.TryParse(unfilledGame.GameMode.Split('-')[1], out int teamSize)) {
            return BadRequest("Game mode must be in format <name>-<team size>");
        }

        _unfilledGamesStore.Add(new UnfilledGame {
            GameMode = unfilledGame.GameMode.ToLower(),
            AccessCode = unfilledGame.AccessCode,
            ExtraPlayersNeeded = unfilledGame.ExtraPlayersNeeded
        });

        return Ok();
    }

    [HttpDelete("{accessCode}")]
    public IActionResult RemoveUnfilledGame(string accessCode) {
        _unfilledGamesStore.Remove(accessCode);
        return Ok();
    }

}