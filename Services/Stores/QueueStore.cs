using System.Collections.Concurrent;
using Matchmaking.Models;

public class QueueStore {
    private readonly ConcurrentDictionary<int, AutoResetEvent> CancellationTokens = new();
    private readonly ConcurrentDictionary<int, DatedValue<string>> PlayerResults = new();
    private const int MAX_PARTY_SIZE = 6;
    private readonly ILogger<QueueStore> _logger;
    private readonly UnfilledGamesStore _unfilledGamesStore;

    // queue is only writable from this class. Other classes can only read the data.
    private readonly ConcurrentDictionary<string, GameMode> _queue = new();
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<IReadOnlyCollection<int>>>> Queue {
        get { return (IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<IReadOnlyCollection<int>>>>)_queue; }
    }

    public QueueStore(
        ILogger<QueueStore> logger,
        UnfilledGamesStore unfilledGameStore
    ) {
        _logger = logger;
        _unfilledGamesStore = unfilledGameStore;
    }

    public WaitResult AddToQueue(string gameModeKey, string regionKey, int leaderId, int partySize) {

        if (partySize > MAX_PARTY_SIZE) {
            return WaitResult.BadRequest($"Party size cannot exceed {MAX_PARTY_SIZE}");
        }

        if (CancellationTokens.ContainsKey(leaderId)) {
            return WaitResult.BadRequest($"Leader ID {leaderId} already in queue");
        }

        if (!_queue.TryGetValue(gameModeKey, out var gameMode)) {
            if (!gameModeKey.Contains('-') || !Int32.TryParse(gameModeKey.Split('-')[1], out int teamSize)) {
                return WaitResult.BadRequest("Game mode must be in format <name>-<team size>");
            }

            _queue[gameModeKey] = new GameMode(teamSize);
            gameMode = _queue[gameModeKey];
        }

        gameMode.Enqueue(regionKey, partySize, leaderId);
        CancellationTokens[leaderId] = new AutoResetEvent(false);
        return WaitForQueueResult(leaderId);
    }

    public WaitResult WaitForQueueResult(int playerId) {

        // When the matchmaker creates a game, it puts the result in PlayerResults BEFORE removing the CancellationToken
        // It's important to check the Cancellation token before checking PlayerResults to avoid a race condition
        if (!CancellationTokens.TryGetValue(playerId, out var wait)) {
            if (!PlayerResults.TryGetValue(playerId, out var accessCode)) {
                return WaitResult.BadRequest($"Player {playerId} not found in queue");
            }
            return WaitResult.Ready(accessCode.Value);
        }

        wait.WaitOne(1000 * 30);
        if (!PlayerResults.ContainsKey(playerId)) {
            return WaitResult.StillWaiting();
        }

        if (!PlayerResults.TryRemove(playerId, out var code)) {
            return WaitResult.Error("Match created but could not remove access code from dictionary");
        }

        return WaitResult.Ready(code.Value);
    }

    public void CreateMatch(ConcurrentQueue<string> accessCodes) {
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
    }

    public void FillGames() {
        _unfilledGamesStore.Mutex.WaitOne();
        foreach (var unfilledGame in _unfilledGamesStore.Values) {
            if (!_queue.ContainsKey(unfilledGame.GameMode)) {
                continue;
            }
            foreach (var regionQueue in _queue[unfilledGame.GameMode].Values) {
                for (int partySize = Math.Min(unfilledGame.ExtraPlayersNeeded, regionQueue.Count); partySize > 0; partySize--) {

                    if (!regionQueue[partySize - 1].IsEmpty && regionQueue[partySize - 1].TryDequeue(out var playerId)) {
                        SendPlayersToGame(new List<int>() { playerId }, unfilledGame.AccessCode);
                        unfilledGame.ExtraPlayersNeeded -= partySize;
                        if (unfilledGame.ExtraPlayersNeeded <= 0) {
                            _unfilledGamesStore.Remove(unfilledGame.AccessCode);
                        }
                        break;
                    }
                }
            }
        }
        _unfilledGamesStore.Mutex.ReleaseMutex();
    }

    public void SendPlayersToGame(List<int> players, string accessCode) {
        foreach (var player in players) {
            PlayerResults[player] = new DatedValue<string>(accessCode);
            CancellationTokens[player].Set();
            CancellationTokens.TryRemove(player, out _);
        }
    }

    public void FailedToQueuePlayers(List<int> players) {
        foreach (var player in players) {
            CancellationTokens[player].Set();
            CancellationTokens.TryRemove(player, out _);
        }
    }

    public void CleanOldResults() {
        var now = DateTime.UtcNow;
        foreach (var playerId in PlayerResults.Keys) {
            if (PlayerResults.TryGetValue(playerId, out var result)) {
                if ((now - result.Date).TotalMinutes > 30) {
                    PlayerResults.TryRemove(playerId, out _);
                }
            }
        }
    }

}