using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class MatchMakerService : BackgroundService
{
    private readonly ILogger<MatchMakerService> _logger;
    private readonly QueueStore _queueStore;

    public MatchMakerService(ILogger<MatchMakerService> logger, QueueStore queueStore)
    {
        _logger = logger;
        _queueStore = queueStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Background service is running at: {time}", DateTimeOffset.Now);

            // Iterate through the queue
            foreach (var pair in _queueStore.Queue)
            {
                var gameMode = pair.Key;
                var regions = pair.Value;
                foreach (var regionPair in regions)
                {
                    var region = regionPair.Key;
                    var queues = regionPair.Value;
                    if (canMakeGame(queues, 2))
                    {
                        List<int> players = new List<int>();
                        queues[0].TryDequeue(out int leaderId);
                        players.Add(leaderId);
                        queues[0].TryDequeue(out int secondLeaderId);
                        players.Add(secondLeaderId);
                        _queueStore.PlayerResults[leaderId] = "AccessCode1";
                        _queueStore.PlayerResults[secondLeaderId] = "AccessCode1";
                        _queueStore.CancellationTokens[leaderId].Set();
                        _queueStore.CancellationTokens[secondLeaderId].Set();
                        _queueStore.CancellationTokens.TryRemove(leaderId, out _);
                        _queueStore.CancellationTokens.TryRemove(secondLeaderId, out _);
                    }

                }
            }

            await Task.Delay(5000, stoppingToken); // Delay for 5 seconds before the next iteration            
            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Background service is stopping at: {time}", DateTimeOffset.Now);
                break;
            }
        }
    }

    private static int queueSize(List<ConcurrentQueue<int>> queues)
    {
        return queues[0].Count * 5
        + queues[1].Count * 4
        + queues[2].Count * 3
        + queues[3].Count * 2
        + queues[4].Count * 1;
    }

    private static bool canMakeGame(List<ConcurrentQueue<int>> queues, int teamSize)
    {
        return queues[0].Count >= 2;
    }
}