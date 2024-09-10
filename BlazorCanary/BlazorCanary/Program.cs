using BlazorCanary.Client.Pages;
using BlazorCanary.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Rebel.Alliance.Canary; // Import Canary architecture namespace
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Actors;
using Rebel.Alliance.Canary.Services;
using System.Text;

namespace BlazorCanary;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        // Configure OIDC authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
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
                    // Custom logic for token validation
                },
                OnUserInformationReceived = context =>
                {
                    // Custom logic for user info retrieval
                    return Task.CompletedTask;
                },
                OnAccessDenied = context =>
                {
                    // Custom logic for access denied handling
                    return Task.CompletedTask;
                }
            };
        });

        // Register Canary services and actors
        builder.Services.AddSingleton<ICryptoService, CryptoService>();
        builder.Services.AddSingleton<IKeyManagementService, KeyManagementService>();
        builder.Services.AddSingleton<OidcProviderService>();
        // Add other required services and actors here

        var app = builder.Build();

        app.MapDefaultEndpoints();

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

        app.UseAuthentication(); // Enable authentication middleware
        app.UseAuthorization(); // Enable authorization middleware

        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }
}
