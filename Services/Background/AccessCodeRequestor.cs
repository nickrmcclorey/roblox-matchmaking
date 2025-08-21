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
    private const string _url = "/cloud/v2/universes/7937098976:publishMessage";


    public AccessCodeRequestor(ILogger<AccessCodeRequestor> logger, AccessCodeStore accessCodeStore) {
        _logger = logger;
        _accessCodeStore = accessCodeStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {

            if (_accessCodeStore.Count < 250 && !_isRequestingCodes) {
                // Make Http request to fetch access codes
                var payload = new {
                    topic = "matchCodes",
                    message = "true"
                };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                content.Headers.Add("x-api-key", Environment.GetEnvironmentVariable("ROBLOX_API_KEY"));
                _logger.LogInformation("Asking for more codes, current count: {Count}", _accessCodeStore.Count);
                var result = await _httpClient.PostAsync(_url, content, stoppingToken);
                if (!result.IsSuccessStatusCode) {
                    _logger.LogError("Failed to request access codes: {StatusCode}", result.StatusCode);
                } else {
                    _logger.LogInformation("Successfully requested access codes.");
                }

                _isRequestingCodes = true;

            } else if (_accessCodeStore.Count > 500 && _isRequestingCodes) {
                // Make Http request to fetch more access codes
                var payload = new {
                    topic = "matchCodes",
                    message = "false"
                };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                content.Headers.Add("x-api-key", Environment.GetEnvironmentVariable("ROBLOX_API_KEY"));
                _logger.LogInformation("Asking for no more codes, current count: {Count}", _accessCodeStore.Count);
                var response = await _httpClient.PostAsync(_url, content, stoppingToken);
                _isRequestingCodes = false;

                if (!response.IsSuccessStatusCode) {
                    _logger.LogError("Failed to request no more access codes: {StatusCode}", response.StatusCode);
                } else {
                    _logger.LogInformation("Successfully requested no more access codes.");
                }
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
