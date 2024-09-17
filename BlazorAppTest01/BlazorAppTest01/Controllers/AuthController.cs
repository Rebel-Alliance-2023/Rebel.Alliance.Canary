using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest request)
    {
        _logger.LogInformation($"Received token request. GrantType: {request.grant_type}, Code: {request.code}, RedirectUri: {request.redirect_uri}, ClientId: {request.client_id}");

        if (request.grant_type != "authorization_code")
        {
            return BadRequest(new { error = "unsupported_grant_type" });
        }

        try
        {
            // Here, implement the actual token generation logic
            var tokenResponse = await GenerateTokenResponse(request);
            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token generation failed");
            return BadRequest(new { error = "invalid_grant", error_description = ex.Message });
        }
    }

    private async Task<TokenResponse> GenerateTokenResponse(TokenRequest request)
    {
        // Validate the authorization code
        // In a real scenario, you would check this against a store of valid codes
        //if (request.code != "valid_code")
        //{
        //    throw new Exception("Invalid authorization code");
        //}

        // Generate access token and ID token
        var accessToken = GenerateJwtToken(request.client_id, "access_token");
        var idToken = GenerateJwtToken(request.client_id, "id_token");

        return new TokenResponse
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = 3600, // 1 hour
            id_token = idToken
        };
    }

    private string GenerateJwtToken(string clientId, string tokenType)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, clientId),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("token_type", tokenType)
    };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public class TokenRequest
    {
        public string grant_type { get; set; }
        public string code { get; set; }
        public string redirect_uri { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
    }

    public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string id_token { get; set; }
    }
}