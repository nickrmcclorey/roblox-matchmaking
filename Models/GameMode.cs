using System.Collections.Concurrent;

public class GameMode : ConcurrentDictionary<string, List<ConcurrentQueue<int>>> {
    public int TeamSize { get; }

    public GameMode(int teamSize) : base() {
        TeamSize = teamSize;
    }
}