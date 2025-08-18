using System.Text.Json;
using System.Net.Http;
using System.Text;

public class AccessCodeRequestor : BackgroundService {
    
    private bool _isRequestingCodes = false;
    private readonly ILogger<AccessCodeRequestor> _logger;
    private readonly AccessCodeStore _accessCodeStore;
    private static HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://apis.roblox.com"),
    };

    public AccessCodeRequestor(ILogger<AccessCodeRequestor> logger, AccessCodeStore accessCodeStore) {
        _logger = logger;
        _accessCodeStore = accessCodeStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {

            if (_accessCodeStore.Count < 250 && !_isRequestingCodes) {
                // Make Http request to fetch access codes
                    var url = "/cloud/v2/universes/7937098976:publishMessage";
                    var payload = new {
                        topic = "matchCodes",
                        message = "true"
                    };
                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync(url, content, stoppingToken);

                    _isRequestingCodes = true;

            } else if (_accessCodeStore.Count > 500 && _isRequestingCodes) {
                // Make Http request to fetch more access codes
                var url = "/cloud/v2/universes/7937098976:publishMessage";
                var payload = new {
                    topic = "matchCodes",
                    message = "false"
                };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(url, content, stoppingToken);
                _isRequestingCodes = false;
            }

            await Task.Delay(1000, stoppingToken);
            _logger.LogInformation("Checking for available access codes...");
        }
    }
}
