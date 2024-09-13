using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class TokenExchangeService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenExchangeService> _logger;

    public TokenExchangeService(HttpClient httpClient, IConfiguration configuration, ILogger<TokenExchangeService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        var tokenEndpoint = _configuration["Oidc:TokenEndpoint"];
        var clientId = _configuration["Oidc:ClientId"];

        _logger.LogInformation($"Exchanging code for token. Endpoint: {tokenEndpoint}, ClientId: {clientId}, RedirectUri: {redirectUri}");

        var tokenRequest = new Dictionary<string, string>
        {
            {"grant_type", "authorization_code"},
            {"code", code},
            {"redirect_uri", redirectUri},
            {"client_id", clientId}
        };

        try
        {
            var response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenRequest));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Token exchange successful. Response: {content}");
                return content;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Token exchange failed. Status: {response.StatusCode}, Error: {errorContent}");
                throw new HttpRequestException($"Token exchange failed: {response.StatusCode}, Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during token exchange");
            throw;
        }
    }
}