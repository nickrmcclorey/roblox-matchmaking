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
    public void AddUnfilledGame([FromBody] UnfilledGame unfilledGame) {
        _unfilledGamesStore.Add(new UnfilledGame {
            GameMode = unfilledGame.GameMode.ToLower(),
            AccessCode = unfilledGame.AccessCode,
            ExtraPlayersNeeded = unfilledGame.ExtraPlayersNeeded
        });
    }

    [HttpDelete("{accessCode}")]
    public IActionResult RemoveUnfilledGame(string accessCode) {
        _unfilledGamesStore.Remove(accessCode);
        return Ok();
    }

}