using System.Collections.Concurrent;
using Matchmaking.Models;
using Microsoft.AspNetCore.Mvc;

public class QueueStore {
    // TODO: Expose these as IReadOnlyDictionary to prevent external modification
    public ConcurrentDictionary<string, GameMode> Queue = new();
    private ConcurrentDictionary<int, AutoResetEvent> CancellationTokens = new();
    private ConcurrentDictionary<int, DatedValue<string>> PlayerResults = new();
    private const int MAX_PARTY_SIZE = 6;

    public WaitResult AddToQueue(string gameModeKey, string regionKey, int leaderId, int partySize) {
        if (partySize > MAX_PARTY_SIZE) {
            return WaitResult.BadRequest($"Party size cannot exceed {MAX_PARTY_SIZE}");
        }

        if (CancellationTokens.ContainsKey(leaderId)) {
            return WaitResult.BadRequest($"Leader ID {leaderId} already in queue");
        }

        if (!Queue.TryGetValue(gameModeKey, out var gameMode)) {
            if (!gameModeKey.Contains('-') || !Int32.TryParse(gameModeKey.Split('-')[1], out int teamSize)) {
                return WaitResult.BadRequest("Game mode must be in format <name>-<team size>");
            }

            Queue[gameModeKey] = new GameMode(teamSize);
            gameMode = Queue[gameModeKey];
        }

        if (!gameMode.TryGetValue(regionKey, out var regionQueue)) {
            gameMode[regionKey] = new List<ConcurrentQueue<int>>(gameMode.TeamSize);
            regionQueue = gameMode[regionKey];
            for (int i = 0; i < gameMode.TeamSize; i++) {
                regionQueue.Add(new ConcurrentQueue<int>());
            }
        }

        CancellationTokens[leaderId] = new AutoResetEvent(false);
        regionQueue[partySize - 1].Enqueue(leaderId);
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

    public void CreateMatch(List<int> players, string accessCode) {
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

}