using Matchmaking.Models;
using System.Collections.Concurrent;

public class MatchMakerService : BackgroundService {
    private readonly ILogger<MatchMakerService> _logger;
    private readonly QueueStore _queueStore;
    private readonly AccessCodeStore _accessCodeStore;

    public MatchMakerService(ILogger<MatchMakerService> logger, QueueStore queueStore, AccessCodeStore accessCodeStore) {
        _logger = logger;
        _queueStore = queueStore;
        _accessCodeStore = accessCodeStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            _logger.LogInformation("Background service is running at: {time}", DateTimeOffset.Now);

            // Iterate through the queue
            foreach (var pair in _queueStore.Queue) {
                var gameMode = pair.Key;
                var regions = pair.Value;
                foreach (var regionPair in regions) {
                    var region = regionPair.Key;
                    var queues = regionPair.Value;
                    if (canMakeGame(queues, 2)) {
                        var success = _accessCodeStore.TryDequeue(out string? accessCode);
                        if (!success || String.IsNullOrEmpty(accessCode)) {
                            _logger.LogWarning("No access code available for matchmaking in {GameMode} for region {Region}", gameMode, region);
                            continue;
                        }

                        var teamOne = getPlayers(regions.TeamSize, queues);
                        var teamTwo = getPlayers(regions.TeamSize, queues);
                        foreach (var player in teamOne.Concat(teamTwo)) {

                            _queueStore.PlayerResults[player] = new DatedValue<string>(accessCode);
                            _queueStore.CancellationTokens[player].Set();
                            _queueStore.CancellationTokens.TryRemove(player, out _);
                        }
                    }
                }
            }

            await Task.Delay(1, stoppingToken);
            if (stoppingToken.IsCancellationRequested) {
                _logger.LogInformation("Background service is stopping at: {time}", DateTimeOffset.Now);
                break;
            }
        }
    }

    private static int queueSize(List<ConcurrentQueue<int>> queues) {
        return queues[0].Count * 5
        + queues[1].Count * 4
        + queues[2].Count * 3
        + queues[3].Count * 2
        + queues[4].Count * 1;
    }

    private static bool canMakeGame(List<ConcurrentQueue<int>> queues, int teamSize) {
        return queues[0].Count >= 2;
    }

    private static IEnumerable<int> getPlayers(int number, List<ConcurrentQueue<int>> parties) {
        if (number == 0) {
            return new List<int>();
        }

        List<int> players = new List<int>();
        for (int partySize = number; partySize >= 1; partySize--) {
            if (parties[partySize - 1].Count > 0) {
                // TODO: Handle dequeue failure
                bool success = parties[partySize - 1].TryDequeue(out int player);
                return new List<int>() { player }.Concat(getPlayers(number - partySize, parties));
            }
        }
        return players;
    }
}