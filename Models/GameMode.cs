using System.Collections.Concurrent;

// maps a region (e.g. "eu") to a list of queues, one for each party size
public class GameMode : ConcurrentDictionary<string, List<ConcurrentQueue<int>>> {
    public int TeamSize { get; }

    public GameMode(int teamSize) : base() {
        TeamSize = teamSize;
    }
}