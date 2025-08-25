using System.Collections.Concurrent;

public struct GameMode
{
    public required int TeamSize { get; set; }
    public required ConcurrentDictionary<string, List<ConcurrentQueue<int>>> Queue { get; set; }
}