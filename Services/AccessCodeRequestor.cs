public class AccessCodeRequestor : BackgroundService {
    private readonly ILogger<AccessCodeRequestor> _logger;
    private readonly AccessCodeStore _accessCodeStore;

    public AccessCodeRequestor(ILogger<AccessCodeRequestor> logger, AccessCodeStore accessCodeStore) {
        _logger = logger;
        _accessCodeStore = accessCodeStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {

            if (_accessCodeStore.Count < 250) {
                // Make Http request to fetch access codes
            } else if (_accessCodeStore.Count > 500) {
                // Make Http request to fetch more access codes
            }

            await Task.Delay(1000, stoppingToken);
            _logger.LogInformation("Checking for available access codes...");
        }
    }
}