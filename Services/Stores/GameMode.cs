using System.Collections.Concurrent;

// maps a region (e.g. "eu") to a list of queues, one for each party size
public class GameMode : ConcurrentDictionary<string, SingleRegionQueue> {

    public int TeamSize { get; }

    public GameMode(int teamSize) : base() {
        TeamSize = teamSize;
    }

    public void Enqueue(string region, int partySize, int leaderId) {
        // var regionQueue = this.GetOrAdd(region, new SingleRegionQueue(TeamSize));
        this[region][partySize - 1].Enqueue(leaderId);
    }

    public void AddRegion(string region) {
        this.TryAdd(region, new SingleRegionQueue(TeamSize));
    }

}