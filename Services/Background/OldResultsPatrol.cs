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
            _queueStore.CleanOldResults();
            await Task.Delay(60000, stoppingToken); // Check every minute
        }
    }
}