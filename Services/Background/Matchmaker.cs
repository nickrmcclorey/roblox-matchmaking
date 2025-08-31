using System.Collections.Concurrent;

public class Matchmaker : BackgroundService {
    private readonly ILogger<Matchmaker> _logger;
    private readonly QueueStore _queueStore;
    private readonly AccessCodeStore _accessCodeStore;
    private readonly UnfilledGamesStore _unfilledGamesStore;

    public Matchmaker(
        ILogger<Matchmaker> logger,
        QueueStore queueStore,
        AccessCodeStore accessCodeStore,
        UnfilledGamesStore unfilledGamesStore
    ) {
        _logger = logger;
        _queueStore = queueStore;
        _accessCodeStore = accessCodeStore;
        _unfilledGamesStore = unfilledGamesStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        int delay = 1;

        while (!stoppingToken.IsCancellationRequested) {

            try {
                var start = DateTime.Now;
                _queueStore.FillGames(_unfilledGamesStore);
                _logger.LogDebug("Took " + (DateTime.Now - start).TotalMilliseconds + " milliseconds to fill existing games");

                start = DateTime.Now;
                int createdGames = _queueStore.CreateMatch(_accessCodeStore);

                delay = createdGames > 0 ? 1 : Math.Min(delay + 1000, 5000);
                if (createdGames > 0) {

                    _logger.LogDebug("Took " + (DateTime.Now - start).TotalMilliseconds + " milliseconds to create new matches");
                    continue;
                }

            } catch (Exception e) {
                _logger.LogCritical(e.ToString());
            }
            
            await Task.Delay(delay, stoppingToken);
        }
        
        _logger.LogInformation("Background service is stopping at: {time}", DateTimeOffset.Now);
    }
}