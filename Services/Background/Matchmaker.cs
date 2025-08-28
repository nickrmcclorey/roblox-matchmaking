using System.Collections.Concurrent;

public class Matchmaker : BackgroundService {
    private readonly ILogger<Matchmaker> _logger;
    private readonly QueueStore _queueStore;
    private readonly AccessCodeStore _accessCodeStore;

    public Matchmaker(
        ILogger<Matchmaker> logger,
        QueueStore queueStore,
        AccessCodeStore accessCodeStore
    ) {
        _logger = logger;
        _queueStore = queueStore;
        _accessCodeStore = accessCodeStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        int delay = 1;

        while (!stoppingToken.IsCancellationRequested) {

            try {
                bool createdGame = false;
                _queueStore.FillGames();

                _queueStore.CreateMatch(_accessCodeStore);

                delay = createdGame ? 1 : Math.Min(delay + 1000, 5000);

            } catch (Exception e) {
                _logger.LogCritical(e.ToString());
            }
            
            await Task.Delay(delay, stoppingToken);
        }
        
        _logger.LogInformation("Background service is stopping at: {time}", DateTimeOffset.Now);
    }
}