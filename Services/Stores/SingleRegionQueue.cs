using System.Collections.Concurrent;

public class SingleRegionQueue : List<ConcurrentQueue<int>> {

    public SingleRegionQueue(int teamSize) : base(teamSize) {
        for (int i = 0; i < teamSize; i++) {
            Add(new ConcurrentQueue<int>());
        }
    }

    public bool CanMakeGame(int teamSize, int teams) {
        var counts = this.Select(q => q.Count).ToList();
        return CanMakeGame(counts, teamSize, teams);
    }

    private static bool CanMakeGame(List<int> queueNumbers, int teamSize, int teams) {
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
                    return CanMakeGame(queueNumbers, teamSize, teams - 1);
                }
            } else {
                partySize--;
            }
        }

        return false;
    }
    
    public MatchmakingResult GetMatch(int teamSize) {
        return GetPlayers(teamSize).Append(GetPlayers(teamSize));
    }

    private MatchmakingResult GetPlayers(int number) {
        if (number == 0) {
            return MatchmakingResult.Success(new List<int>());
        }

        List<int> players = new List<int>();
        for (int partySize = number; partySize >= 1; partySize--) {
            if (!this[partySize - 1].IsEmpty) {
                // TODO: Handle dequeue failure
                bool success = this[partySize - 1].TryDequeue(out int party);
                if (!success) {
                    return MatchmakingResult.Failure(new List<int>());
                }
                return MatchmakingResult.Success(new List<int>() { party }).Append(GetPlayers(number - partySize));
            }
        }
        return MatchmakingResult.Failure(players);
    }
}