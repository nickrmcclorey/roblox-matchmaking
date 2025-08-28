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

    public void FillGame(string accessCode, int players) {
        if (!this.ContainsKey(accessCode)) {
            return;
        }

        Mutex.WaitOne();
        this[accessCode].ExtraPlayersNeeded -= players;
        if (this[accessCode].ExtraPlayersNeeded <= 0) {
            base.Remove(accessCode);
        }
        Mutex.ReleaseMutex();
    }

    public new void Remove(string key) {
        Mutex.WaitOne();
        base.Remove(key);
        Mutex.ReleaseMutex();
    }
}