using System.Collections.Concurrent;

public class QueueStore
{
    //                       Game Mode           Region Size Queue
    public ConcurrentDictionary<string, ConcurrentDictionary<string, List<ConcurrentQueue<int>>>> Queue = new();
    public ConcurrentDictionary<int, AutoResetEvent> CancellationTokens = new();
    public ConcurrentDictionary<int, string> PlayerResults = new();
    public const int MAX_PARTY_SIZE = 5;

    public AutoResetEvent AddToQueue(string gameMode, string region, int leaderId, int partySize)
    {
        if (!Queue.ContainsKey(gameMode))
        {
            Queue[gameMode] = new ConcurrentDictionary<string, List<ConcurrentQueue<int>>>();
        }

        if (!Queue[gameMode].ContainsKey(region))
        {
            Queue[gameMode][region] = new List<ConcurrentQueue<int>>(5)
            {
                new ConcurrentQueue<int>(),
                new ConcurrentQueue<int>(),
                new ConcurrentQueue<int>(),
                new ConcurrentQueue<int>(),
                new ConcurrentQueue<int>()
            };
        }


        if (partySize > MAX_PARTY_SIZE)
        {
            throw new BadHttpRequestException($"Party size cannot exceed {MAX_PARTY_SIZE}");
        }

        CancellationTokenSource token = new CancellationTokenSource();
        CancellationTokens[leaderId] = new AutoResetEvent(false);
        Queue[gameMode][region][partySize - 1].Enqueue(leaderId);
        return CancellationTokens[leaderId];
    }


}