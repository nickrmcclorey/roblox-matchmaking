using System.Collections.Concurrent;

public class UnfilledGamesStore {

    public ConcurrentDictionary<string, ConcurrentQueue<UnfilledGame>> UnfilledGames = new();

    public void Enqueue(string gameModeKey, UnfilledGame unfilledGame) {

        if (!UnfilledGames.TryGetValue(gameModeKey, out var queue)) {
            UnfilledGames[gameModeKey] = new ConcurrentQueue<UnfilledGame>();
            queue = UnfilledGames[gameModeKey];
        }

        queue.Enqueue(unfilledGame);
    }

    public UnfilledGame? Peek(string gameModeKey) {
        if (UnfilledGames.TryGetValue(gameModeKey, out var queue)) {
            if (queue.TryPeek(out var unfilledGame)) {
                return unfilledGame;
            }
        }
        return null;
    }

    public UnfilledGame? Dequeue(string gameModeKey) {
        if (UnfilledGames.TryGetValue(gameModeKey, out var queue)) {
            if (queue.TryDequeue(out var unfilledGame)) {
                return unfilledGame;
            }
        }
        return null;
    }

}