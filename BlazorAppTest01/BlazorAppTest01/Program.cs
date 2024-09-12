using BlazorAppTest01.Client.Pages;
using BlazorAppTest01.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Rebel.Alliance.Canary.Services;
using Rebel.Alliance.Canary.Actors;
using Rebel.Alliance.Canary.Messaging;
using Rebel.Alliance.Canary.Abstractions;
using System.Text;
using MediatR;
using Rebel.Alliance.Canary.Configuration;
using Rebel.Alliance.Canary.SystemProviders;
using Microsoft.Extensions.DependencyInjection;

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

// Register Canary services and actors

// Register Canary services and actors
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddSingleton<IKeyManagementService, KeyManagementService>();
builder.Services.AddSingleton<IActorMessageBus, InMemoryActorMessageBus>();
builder.Services.AddSingleton<IActorStateManager, Rebel.Alliance.Canary.Configuration.InMemoryActorStateManager>();
builder.Services.AddSingleton<DecentralizedOIDCProviderService>();
builder.Services.AddSingleton<OidcProviderService>();
builder.Services.AddSingleton<IKeyStore, InMemoryKeyStore>();

// Register actors
builder.Services.AddTransient<OIDCClientActor>(sp =>
    new OIDCClientActor(
        Guid.NewGuid().ToString(),
        sp.GetRequiredService<IActorStateManager>(),
        sp.GetRequiredService<IActorMessageBus>()
    )
);

builder.Services.AddTransient<TokenIssuerActor>(sp =>
    new TokenIssuerActor(
        sp.GetRequiredService<ICryptoService>(),
        sp.GetRequiredService<IActorMessageBus>(),
        sp.GetRequiredService<IActorStateManager>(),
        Guid.NewGuid().ToString()
    )
);

builder.Services.AddTransient<CredentialVerifierActor>(sp =>
    new CredentialVerifierActor(
        Guid.NewGuid().ToString(),
        sp.GetRequiredService<ICryptoService>(),
        sp.GetRequiredService<IRevocationManagerActor>()
    )
);

builder.Services.AddTransient<IRevocationManagerActor>(sp =>
    new RevocationManagerActor(
        Guid.NewGuid().ToString(),
        (IActorStateManager)sp.GetRequiredService<IActorMessageBus>(),
        (IMediator)sp.GetRequiredService<IActorStateManager>()
    )
);

builder.Services.AddTransient(typeof(ActorMessageHandler<>));


// Register ActorMessageHandler for each actor type

builder.Services.AddTransient(typeof(IRequestHandler<ActorMessageEnvelope<OIDCClientActor>, object>), typeof(ActorMessageHandler<OIDCClientActor>));

builder.Services.AddTransient(typeof(IRequestHandler<ActorMessageEnvelope<TokenIssuerActor>, object>), typeof(ActorMessageHandler<TokenIssuerActor>));

// Add similar lines for other actor types you have, e.g.:

builder.Services.AddTransient(typeof(IRequestHandler<ActorMessageEnvelope<CredentialVerifierActor>, object>), typeof(ActorMessageHandler<CredentialVerifierActor>));


// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
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
// ...

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorAppTest01.Client._Imports).Assembly);

app.MapGet("/connect/authorize", async (HttpContext context, DecentralizedOIDCProviderService oidcService) =>
{
    // Handle OIDC authorization requests
    var response = await oidcService.InitiateAuthenticationAsync(context.Request.Query["client_id"], context.Request.Query["redirect_uri"]);
    return Results.Redirect(response);
});

app.MapPost("/connect/token", async (HttpContext context, DecentralizedOIDCProviderService oidcService) =>
{
    var form = await context.Request.ReadFormAsync();
    var code = form["code"];
    var clientId = form["client_id"];
    var redirectUri = form["redirect_uri"];

    var tokenResponse = await oidcService.ExchangeAuthorizationCodeAsync(clientId, code, redirectUri);
    return Results.Json(tokenResponse);
});

app.Run();
