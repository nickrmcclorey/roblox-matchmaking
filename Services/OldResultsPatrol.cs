public class OldResultsPatrol : BackgroundService {
    private readonly ILogger<OldResultsPatrol> _logger;
    private readonly QueueStore _queueStore;

    public OldResultsPatrol(ILogger<OldResultsPatrol> logger, QueueStore queueStore) {
        _logger = logger;
        _queueStore = queueStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            // Check for old results and clean them up
            var now = DateTime.UtcNow;
            foreach (var playerId in _queueStore.PlayerResults.Keys) {
                if (_queueStore.PlayerResults.TryGetValue(playerId, out var result)) {
                    if ((now - result.Date).TotalMinutes > 30) {
                        _queueStore.PlayerResults.TryRemove(playerId, out _);
                        _logger.LogInformation($"Removed old result for player {playerId}");
                    }
                }
            }

            await Task.Delay(60000, stoppingToken); // Check every minute
        }
    }
}