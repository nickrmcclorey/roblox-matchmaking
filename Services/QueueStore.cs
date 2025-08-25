using System.Collections.Concurrent;
using Matchmaking.Models;

public class QueueStore
{
    public ConcurrentDictionary<string, GameMode> Queue = new();
    public ConcurrentDictionary<int, AutoResetEvent> CancellationTokens = new();
    public ConcurrentDictionary<int, DatedValue<string>> PlayerResults = new();
    public const int MAX_PARTY_SIZE = 5;

    public void AddToQueue(string gameMode, string region, int leaderId, int partySize)
    {
        if (!Queue.ContainsKey(gameMode))
        {
            Queue[gameMode] = new GameMode(partySize);
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

        if (CancellationTokens.ContainsKey(leaderId)) {
            throw new BadHttpRequestException($"Leader ID {leaderId} already in queue");
        }

        CancellationTokens[leaderId] = new AutoResetEvent(false);
        Queue[gameMode][region][partySize - 1].Enqueue(leaderId);
    }


}