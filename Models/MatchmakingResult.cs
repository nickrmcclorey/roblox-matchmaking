public class MatchmakingResult {
    public List<int> Players { get; private set; } = new List<int>();
    public bool success { get; private set; }

    public static MatchmakingResult Success(List<int> players) {
        return new MatchmakingResult() {
            Players = players,
            success = true
        };
    }

    public static MatchmakingResult Failure(List<int> players) {
        return new MatchmakingResult() {
            Players = players,
            success = false
        };
    }

    public static MatchmakingResult Failure() {
        return new MatchmakingResult() {
            Players = new List<int>(),
            success = false
        };
    }

    public MatchmakingResult Append(MatchmakingResult m) {
        Players.AddRange(m.Players);
        success = success && m.success;
        return this;
    }
}