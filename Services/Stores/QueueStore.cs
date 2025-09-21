using System.Collections.Concurrent;
using Matchmaking.Models;

public class QueueStore {
    private readonly ConcurrentDictionary<int, AutoResetEvent> CancellationTokens = new();
    private readonly ConcurrentDictionary<int, DatedValue<string>> PlayerResults = new();
    private const int MAX_PARTY_SIZE = 6;
    private readonly ILogger<QueueStore> _logger;

    // queue is only writable from this class. Other classes can only read the data.
    private readonly Dictionary<string, GameMode> _queue = new();
    public IReadOnlyDictionary<string, GameMode> Queue => _queue;

    public QueueStore(
        ILogger<QueueStore> logger
    ) {
        _logger = logger;
    }

    public void AddGameMode(string gameMode) {
        if (!Int32.TryParse(gameMode.Split('-')[1], out int teamSize)) {
            throw new BadHttpRequestException("GameMode key isn't formatted correctly:" + gameMode);
        }

        if (_queue.ContainsKey(gameMode)) {
            throw new BadHttpRequestException("GameMode already exists:" + gameMode);
        }

        if (!_queue.TryAdd(gameMode, new GameMode(teamSize))) {
            throw new Exception("Failed to add gamemode:" + gameMode);
        }
    }

    public void AddRegion(string region) {
        foreach (var pair in _queue) {
            pair.Value.AddRegion(region);
        }
    }

    public WaitResult AddToQueue(string gameModeKey, string regionKey, int leaderId, int partySize) {
        
        if (partySize > MAX_PARTY_SIZE) {
            return WaitResult.BadRequest($"Party size cannot exceed {MAX_PARTY_SIZE}");
        }

        if (CancellationTokens.ContainsKey(leaderId)) {
            return WaitResult.BadRequest($"Leader ID {leaderId} already in queue");
        }

        if (!gameModeKey.Contains('-') || !Int32.TryParse(gameModeKey.Split('-')[1], out int teamSize)) {
            return WaitResult.BadRequest("Game mode must be in format <name>-<team size>");
        }

        CancellationTokens[leaderId] = new AutoResetEvent(false);
        _queue[gameModeKey].Enqueue(regionKey, partySize, leaderId);
        return WaitResult.StillWaiting();
    }

    public WaitResult WaitForQueueResult(int playerId) {

        // When the matchmaker creates a game, it puts the result in PlayerResults BEFORE removing the CancellationToken
        // It's important to check the Cancellation token before checking PlayerResults to avoid a race condition
        if (!CancellationTokens.ContainsKey(playerId)) {
            if (!PlayerResults.Remove(playerId, out var accessCode)) {
                return WaitResult.BadRequest($"Player {playerId} not found in queue");
            }
            return WaitResult.Ready(accessCode.Value);
        }

        DateTime start = DateTime.Now;
        CancellationTokens[playerId].WaitOne(1000 * 30);
        _logger.LogDebug("Waited " + (DateTime.Now - start).TotalSeconds + " seconds for match to be created");
        if (!PlayerResults.ContainsKey(playerId)) {
            return WaitResult.StillWaiting();
        }

        start = DateTime.Now;
        if (!PlayerResults.Remove(playerId, out var code)) {
            return WaitResult.Error("Match created but could not remove access code from dictionary");
        }
        _logger.LogDebug("Waited " + (DateTime.Now - start).Milliseconds + " milliseconds to remove PlayerResult");

        return WaitResult.Ready(code.Value);
    }

    public int CreateMatch(ConcurrentQueue<string> accessCodes) {
        int createdGames = 0;
        foreach (var pair in _queue) {
            var gameMode = pair.Key;
            var regions = pair.Value;
            foreach (var regionPair in regions) {
                var region = regionPair.Key;
                var queues = regionPair.Value;
                if (queues.CanMakeGame(regions.TeamSize, 2)) {
                    var success = accessCodes.TryDequeue(out string? accessCode);
                    if (!success || String.IsNullOrEmpty(accessCode)) {
                        _logger.LogWarning("No access code available for matchmaking");
                        break;
                    }

                    MatchmakingResult match = queues.GetMatch(regions.TeamSize);
                    if (!match.success) {
                        // Requeue players that got removed from the queue
                        _logger.LogWarning("Matchmaking failed for {GameMode} in region {Region}.", gameMode, region);
                        FailedToQueuePlayers(match.Players);
                        continue;
                    }

                    SendPlayersToGame(match.Players, accessCode);
                    createdGames += 1;
                }
            }
        }
        return createdGames;
    }

    public void FillGames(UnfilledGamesStore unfilledGamesStore) {
        unfilledGamesStore.Mutex.WaitOne();
        foreach (var unfilledGame in unfilledGamesStore.Values) {
            if (!_queue.ContainsKey(unfilledGame.GameMode)) {
                continue;
            }
            foreach (var regionQueue in _queue[unfilledGame.GameMode].Values) {
                for (int partySize = Math.Min(unfilledGame.ExtraPlayersNeeded, regionQueue.Count); partySize > 0; partySize--) {

                    if (!regionQueue[partySize - 1].IsEmpty && regionQueue[partySize - 1].TryDequeue(out var playerId)) {
                        SendPlayersToGame(new List<int>() { playerId }, unfilledGame.AccessCode);
                        unfilledGame.ExtraPlayersNeeded -= partySize;
                        if (unfilledGame.ExtraPlayersNeeded <= 0) {
                            unfilledGamesStore.Remove(unfilledGame.AccessCode);
                        }
                        break;
                    }
                }
            }
        }
        unfilledGamesStore.Mutex.ReleaseMutex();
    }

    public void SendPlayersToGame(List<int> players, string accessCode) {
        foreach (var player in players) {
            PlayerResults[player] = new DatedValue<string>(accessCode);
            CancellationTokens[player].Set();
            CancellationTokens.TryRemove(player, out var _);
        }
    }

    public void FailedToQueuePlayers(List<int> players) {
        foreach (var player in players) {
            CancellationTokens[player].Set();
            CancellationTokens.TryRemove(player, out var _);
        }
    }

    public void CleanOldResults() {
        var now = DateTime.UtcNow;
        foreach (var playerId in PlayerResults.Keys) {
            if (PlayerResults.TryGetValue(playerId, out var result)) {
                if ((now - result.Date).TotalMinutes > 30) {
                    PlayerResults.TryRemove(playerId, out var _);
                }
            }
        }
    }

}