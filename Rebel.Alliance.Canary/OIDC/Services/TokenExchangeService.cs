using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

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
        var tokenEndpoint = _configuration["WebAppVerifiableCredential:TokenEndpoint"];
        var clientId = _configuration["WebAppVerifiableCredential:ClientId"];
        var clientSecret = _configuration["WebAppVerifiableCredential:ClientSecret"];

        _logger.LogInformation($"Exchanging code for token. Endpoint: {tokenEndpoint}, ClientId: {clientId}, RedirectUri: {redirectUri}");

        var tokenRequest = new
        {
            grant_type = "authorization_code",
            code,
            redirect_uri = redirectUri,
            client_id = clientId,
            client_secret = clientSecret
        };

        try
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(tokenRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(tokenEndpoint, jsonContent);

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


    public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
    }
}