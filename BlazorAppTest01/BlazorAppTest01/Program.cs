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
using BlazorAppTest01;

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
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
})
.AddJwtBearer(options =>
{
    options.Authority = webAppVc.Authority;
    options.Audience = webAppVc.Claims["aud"];
    // Add more JWT Bearer options as needed
})
.AddOpenIdConnect("oidc", options =>
{
    options.SignInScheme = "Cookies";
    options.Authority = webAppVc.Authority;
    options.ClientId = webAppVc.ClientId;
    options.ClientSecret = webAppVc.ClientSecret;
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    // Add any additional scopes you need
});

// Add cookie authentication
builder.Services.AddAuthentication()
    .AddCookie("Cookies");

// Add authorization
builder.Services.AddAuthorization();

// Add Canary Actor System
builder.Services.AddCanaryActorSystem(options =>
{
    options.ActorSystemName = "CanaryActorSystem";
    options.ActorFramework = "in-memory";
    options.WebAppVc = webAppVc;
});

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
