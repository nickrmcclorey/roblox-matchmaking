using Microsoft.AspNetCore.Mvc;

[Route("games")]
public class UnfilledGameController : Controller {

    private readonly UnfilledGamesStore _unfilledGamesStore;

    public UnfilledGameController(UnfilledGamesStore unfilledGamesStore) {
        _unfilledGamesStore = unfilledGamesStore;
    }

    [HttpPost("{gameModeKey}/fill")]
    public void AddUnfilledGame(string gameModeKey, [FromBody] UnfilledGame unfilledGame) {
        _unfilledGamesStore.Enqueue(gameModeKey, unfilledGame);
    }

}