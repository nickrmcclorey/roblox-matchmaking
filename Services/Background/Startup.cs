
public class Startup : BackgroundService {

    private readonly ILogger<Startup> _logger;
    private readonly QueueStore _queueStore;
    private readonly IConfiguration _configuration;

    public Startup(ILogger<Startup> logger, QueueStore queueStore, IConfiguration configuration) {
        _logger = logger;
        _queueStore = queueStore;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Creating GameModes");
        List<string> gameModes = _configuration.GetSection("Matchmaking:GameModes").Get<List<string>>();
        foreach (var mode in gameModes) {
            _queueStore.AddGameMode(mode);
        }

        List<string> regions = _configuration.GetSection("Matchmaking:Regions").Get<List<string>>();
        foreach (var region in regions) {
            _queueStore.AddRegion(region);
        }

        _logger.LogInformation("Finished Creating GameModes");
        return Task.CompletedTask;
    }
}