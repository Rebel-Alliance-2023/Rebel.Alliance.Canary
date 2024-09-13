using BlazorAppTest01.Client.Pages;
using BlazorAppTest01.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Rebel.Alliance.Canary.Configuration;
using Rebel.Alliance.Canary.OIDC.Services;
using Rebel.Alliance.Canary.VerifiableCredentials;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Rebel.Alliance.Canary.Security;

var builder = WebApplication.CreateBuilder(args);

// Read WebAppVerifiableCredential from configuration
var webAppVcConfig = builder.Configuration.GetSection("WebAppVerifiableCredential");
var webAppVc = new VerifiableCredential
{
    Authority = webAppVcConfig["Authority"],
    ClientId = webAppVcConfig["ClientId"],
    ClientSecret = webAppVcConfig["ClientSecret"],
    Claims = webAppVcConfig.GetSection("Claims").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>()
};

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
})
.AddJwtBearer(options =>
{
    // Configure JWT Bearer options
})
.AddOpenIdConnect("oidc", options =>
{
    options.SignInScheme = "Cookies";
    options.Authority = webAppVc.Authority;
    options.ClientId = webAppVc.ClientId;
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
});

// Add authorization
builder.Services.AddAuthorization();

// Add Canary Actor System
builder.Services.AddCanaryActorSystem(options =>
{
    options.ActorSystemName = "CanaryActorSystem";
    options.ActorFramework = "in-memory";
});

// Register CredentialVerifierActor with WebAppVerifiableCredential
builder.Services.AddTransient<ICredentialVerifierActor>(sp => new CredentialVerifierActor(
    Guid.NewGuid().ToString(),
    sp.GetRequiredService<ICryptoService>(),
    sp.GetRequiredService<IRevocationManagerActor>(),
    sp.GetRequiredService<ILogger<CredentialVerifierActor>>(),
    webAppVc
));

builder.Services.AddHttpClient<TokenExchangeService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
});

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorAppTest01.Client._Imports).Assembly);

app.MapGet("/connect/authorize", async (HttpContext context, DecentralizedOIDCProvider oidcService) =>
{
    var response = await oidcService.InitiateAuthenticationAsync(context.Request.Query["client_id"], context.Request.Query["redirect_uri"]);
    return Results.Redirect(response);
});

app.MapPost("/connect/token", async (HttpContext context, DecentralizedOIDCProvider oidcService) =>
{
    var form = await context.Request.ReadFormAsync();
    var code = form["code"];
    var clientId = form["client_id"];
    var redirectUri = form["redirect_uri"];

    var tokenResponse = await oidcService.ExchangeAuthorizationCodeAsync(clientId, code, redirectUri);
    return Results.Json(tokenResponse);
});

app.Run();
