using BlazorCanary.Client.Pages;
using BlazorCanary.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using BlazorCanary.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Rebel.Alliance.Canary.OIDC.Services;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;
using Rebel.Alliance.Canary.InMemoryActorFramework;
using Rebel.Alliance.Canary.Configuration;
using Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TokenIssuerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CascadingAuthenticationState>();

// Configure OIDC authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // JWT Bearer configuration
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["Oidc:Authority"];
    options.ClientId = builder.Configuration["Oidc:ClientId"];
    options.ClientSecret = builder.Configuration["Oidc:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async context =>
        {
            // Custom token validation logic using Canary services
            var decentralizedOidcService = context.HttpContext.RequestServices
                .GetRequiredService<DecentralizedOIDCProvider>();
            // Implement token validation logic here
        }
    };
});

// Add authorization services
builder.Services.AddAuthorization(options =>
{
    // You can add authorization policies here if needed
    // For example:
    // options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
});

// Register Canary services and actors
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddSingleton<IKeyManagementService, KeyManagementService>();
builder.Services.AddSingleton<IActorMessageBus, InMemoryActorMessageBus>();
builder.Services.AddSingleton<IActorStateManager, InMemoryActorStateManager>();
builder.Services.AddSingleton<DecentralizedOIDCProvider>();
builder.Services.AddSingleton<OidcProviderService>();
builder.Services.AddSingleton<IKeyStore, InMemoryKeyStore>();

builder.Services.AddTransient<OIDCClientActor>(sp =>
    new OIDCClientActor(
        Guid.NewGuid().ToString(),
        sp.GetRequiredService<IActorStateManager>(),
        sp.GetRequiredService<IActorMessageBus>(),
        sp.GetRequiredService<ILogger<OIDCClientActor>>() // Added logger parameter
    )
);

builder.Services.AddTransient<ITokenIssuerActor>(sp => new TokenIssuerActor(
    sp.GetRequiredService<ICryptoService>(),
    sp.GetRequiredService<IActorMessageBus>(),
    sp.GetRequiredService<IActorStateManager>(),
    sp.GetRequiredService<ILogger<TokenIssuerActor>>(),
    Guid.NewGuid().ToString()
));

//builder.Services.AddTransient<CredentialVerifierActor>(sp =>
//    new CredentialVerifierActor(
//        Guid.NewGuid().ToString(),
//        sp.GetRequiredService<ICryptoService>(),
//        sp.GetRequiredService<IRevocationManagerActor>()
//    )
//);

// Add other actors as needed

//Get this executing assembly
var assembly = Assembly.GetExecutingAssembly();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

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
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();


app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
     .AddAdditionalAssemblies(typeof(BlazorCanary.Client._Imports).Assembly);

app.Run();

