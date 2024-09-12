using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenExchangeService _tokenExchangeService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(TokenExchangeService tokenExchangeService, ILogger<AuthController> logger)
    {
        _tokenExchangeService = tokenExchangeService;
        _logger = logger;
    }

    [HttpGet("exchange")]
    public async Task<IActionResult> ExchangeCode([FromQuery] string code, [FromQuery] string redirectUri)
    {
        _logger.LogInformation($"Received exchange request. Code: {code}, RedirectUri: {redirectUri}");
        try
        {
            var tokenResponse = await _tokenExchangeService.ExchangeCodeForTokenAsync(code, redirectUri);
            _logger.LogInformation("Token exchange successful");
            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token exchange failed");
            return BadRequest(ex.Message);
        }
    }
}