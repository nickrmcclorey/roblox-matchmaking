using System.Collections.Concurrent;

public class Matchmaker : BackgroundService {
    private readonly ILogger<Matchmaker> _logger;
    private readonly QueueStore _queueStore;
    private readonly AccessCodeStore _accessCodeStore;

    public Matchmaker(ILogger<Matchmaker> logger, QueueStore queueStore, AccessCodeStore accessCodeStore) {
        _logger = logger;
        _queueStore = queueStore;
        _accessCodeStore = accessCodeStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        int delay = 1;

        while (!stoppingToken.IsCancellationRequested) {

            bool createdGame = false;

            // Iterate through the queue
            foreach (var pair in _queueStore.Queue) {
                var gameMode = pair.Key;
                var regions = pair.Value;
                foreach (var regionPair in regions) {
                    var region = regionPair.Key;
                    var queues = regionPair.Value;
                    var counts = queues.Select(q => q.Count).ToList();
                    if (canMakeGame(counts, regions.TeamSize, 2)) {
                        var success = _accessCodeStore.TryDequeue(out string? accessCode);
                        if (!success || String.IsNullOrEmpty(accessCode)) {
                            _logger.LogWarning("No access code available for matchmaking");
                            break;
                        }

                        MatchmakingResult match = getMatch(queues, regions.TeamSize);
                        if (!match.success) {
                            // Requeue players that got removed from the queue
                            _logger.LogWarning("Matchmaking failed for {GameMode} in region {Region}.", gameMode, region);
                            _queueStore.FailedToQueuePlayers(match.Players);
                            continue;
                        }

                        createdGame = true;
                        _queueStore.CreateMatch(match.Players, accessCode);
                    }
                }
            }

            if (stoppingToken.IsCancellationRequested)
                break;

            delay = createdGame ? 1 : Math.Min(delay + 1000, 5000);
            await Task.Delay(delay, stoppingToken);
        }
        
        _logger.LogInformation("Background service is stopping at: {time}", DateTimeOffset.Now);
    }

    private static int queueSize(List<ConcurrentQueue<int>> queues) {
        int sum = 0;
        for (int i = 0; i < queues.Count; i++) {
            sum += queues[i].Count * (i + 1);
        }
        return sum;
    }

    public static bool canMakeGame(List<int> queueNumbers, int teamSize, int teams) {
        if (queueNumbers[teamSize - 1] >= teams) {
            return true;
        } else if (teams == 0) {
            return true;
        }

        int peopleNeeded = teamSize;
        int partySize = teamSize;
        while (partySize > 0 && peopleNeeded > 0) {
            if (queueNumbers[partySize - 1] > 0) {
                queueNumbers[partySize - 1]--;
                peopleNeeded -= partySize;
                partySize = peopleNeeded;
                if (peopleNeeded == 0) {
                    return canMakeGame(queueNumbers, teamSize, teams - 1);
                }
            } else {
                partySize--;
            }
        }
        
        return false;
    }

    private static MatchmakingResult getMatch(List<ConcurrentQueue<int>> parties, int teamSize) {
        return getPlayers(teamSize, parties).Append(getPlayers(teamSize, parties));
    }

    private static MatchmakingResult getPlayers(int number, List<ConcurrentQueue<int>> parties) {
        if (number == 0) {
            return MatchmakingResult.Success(new List<int>());
        }

        List<int> players = new List<int>();
        for (int partySize = number; partySize >= 1; partySize--) {
            if (parties[partySize - 1].Count > 0) {
                // TODO: Handle dequeue failure
                bool success = parties[partySize - 1].TryDequeue(out int party);
                if (!success) {
                    return MatchmakingResult.Failure(new List<int>());
                }
                return MatchmakingResult.Success(new List<int>() { party }).Append(getPlayers(number - partySize, parties));
            }
        }
        return MatchmakingResult.Failure(players);
    }
}