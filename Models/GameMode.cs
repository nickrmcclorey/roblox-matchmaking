using System.Collections.Concurrent;

// maps a region (e.g. "eu") to a list of queues, one for each party size
public class GameMode : ConcurrentDictionary<string, SingleRegionQueue> {

    public int TeamSize { get; }
    public ICollection<string> Regions => this.Keys;
    public ICollection<SingleRegionQueue> SingleRegionQueues => this.Values;

    public GameMode(int teamSize) : base() {
        TeamSize = teamSize;
    }

    public void Enqueue(string region, int partySize, int leaderId) {
        if (!this.TryGetValue(region, out var regionQueue)) {
            this[region] = new SingleRegionQueue(TeamSize);
            regionQueue = this[region];
        }

        regionQueue[partySize - 1].Enqueue(leaderId);
    }

}