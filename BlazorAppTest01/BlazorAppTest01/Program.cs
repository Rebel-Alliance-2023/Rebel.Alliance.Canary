using BlazorAppTest01.Client.Pages;
using BlazorAppTest01.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Rebel.Alliance.Canary.Configuration;
using Rebel.Alliance.Canary.OIDC.Services;

var builder = WebApplication.CreateBuilder(args);

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
    options.Authority = builder.Configuration["Oidc:Authority"];
    options.ClientId = builder.Configuration["Oidc:ClientId"];
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

builder.Services.AddHttpClient<TokenExchangeService>();

builder.Services.AddControllers();

var app = builder.Build();

// ... [rest of the code remains the same]
