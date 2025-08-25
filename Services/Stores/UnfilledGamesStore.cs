using System.Collections.Concurrent;

public class UnfilledGamesStore : Dictionary<string, UnfilledGame> {

    public Mutex Mutex = new();

    public void Add(UnfilledGame unfilledGame) {
        Mutex.WaitOne();
        if (this.ContainsKey(unfilledGame.AccessCode)) {
            this[unfilledGame.AccessCode].ExtraPlayersNeeded += unfilledGame.ExtraPlayersNeeded;
        } else {
            this[unfilledGame.AccessCode] = unfilledGame;
        }

        Mutex.ReleaseMutex();
    }
}