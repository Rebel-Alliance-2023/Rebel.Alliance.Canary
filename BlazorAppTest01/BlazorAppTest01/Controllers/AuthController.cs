using Microsoft.AspNetCore.Mvc;
using Rebel.Alliance.Canary.OIDC.Models;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TokenExchangeService _tokenExchangeService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(TokenExchangeService tokenExchangeService, ILogger<AuthController> logger)
    {
        _tokenExchangeService = tokenExchangeService;
        _logger = logger;
    }

    [HttpGet("exchange-code")]
    public async Task<ActionResult<TokenResponse>> ExchangeCode(
        [FromQuery, Required] string code,
        [FromQuery, Required] string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(redirectUri))
        {
            return BadRequest("Code and redirectUri are required.");
        }

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
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}