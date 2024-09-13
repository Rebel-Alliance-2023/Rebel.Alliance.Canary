using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebel.Alliance.Canary.OIDC.Models;
using Rebel.Alliance.Canary.OIDC.Services;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials;
using Rebel.Alliance.Canary.VerifiableCredentials.Generator;
using Rebel.Alliance.Canary.Configuration;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.CredentialVerifierActor;
using Rebel.Alliance.Canary.Actor.Interfaces.Actors;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem;
using Rebel.Alliance.Canary.Actor.Interfaces;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;

namespace Rebel.Alliance.Canary.Tests
{
    public class CanaryOidcTests : IClassFixture<CanaryTestFixture>
    {
        private readonly IDecentralizedOIDCProviderService _oidcProviderService;
        private readonly ICredentialVerifierActor _credentialVerifierActor;
        private readonly IOIDCClientActor _oidcClientActor;

        public CanaryOidcTests(CanaryTestFixture fixture)
        {
            _oidcProviderService = fixture.OidcProviderService;
            _credentialVerifierActor = fixture.CredentialVerifierActor;
            _oidcClientActor = fixture.OidcClientActor;
        }

        [Fact]
        public void GenerateUserCredential_ShouldCreateValidVC()
        {
            var userClaims = new Dictionary<string, string>
            {
                { "name", "John Doe" },
                { "email", "john.doe@example.com" }
            };
            var vc = CredentialGenerator.GenerateUserCredential("user123", userClaims);

            Assert.NotNull(vc);
            Assert.Equal("user123", vc.Subject);
            Assert.True(vc.Claims.ContainsKey("name"));
            Assert.True(vc.Claims.ContainsKey("email"));
        }

        [Fact]
        public async Task InitiateAuthentication_ShouldReturnAuthorizationCode()
        {
            var redirectUri = "https://testapp.com/callback";
            var authorizationCode = await _oidcClientActor.InitiateAuthenticationAsync(redirectUri);

            Assert.NotNull(authorizationCode);
            Assert.NotEmpty(authorizationCode);
        }

        [Fact]
        public async Task ExchangeAuthorizationCode_ShouldReturnTokens()
        {
            var redirectUri = "https://testapp.com/callback";
            var clientId = "testClient";
            var authorizationCode = await _oidcClientActor.InitiateAuthenticationAsync(redirectUri);

            var oidcResponse = await _oidcClientActor.ExchangeAuthorizationCodeAsync(authorizationCode, redirectUri, clientId);

            Assert.NotNull(oidcResponse);
            Assert.NotNull(oidcResponse.AccessToken);
            Assert.NotNull(oidcResponse.IdToken);
        }


        [Fact]
        public async Task ValidateToken_ShouldReturnTrue_ForValidAccessToken()
        {
            var redirectUri = "https://testapp.com/callback";
            var clientId = "testClient";
            var authorizationCode = await _oidcClientActor.InitiateAuthenticationAsync(redirectUri);
            var oidcResponse = await _oidcClientActor.ExchangeAuthorizationCodeAsync(authorizationCode, redirectUri, clientId);

            var isValid = await _credentialVerifierActor.ValidateTokenAsync(oidcResponse.AccessToken);

            Assert.True(isValid);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnTrue_ForValidIdToken()
        {
            var redirectUri = "https://testapp.com/callback";
            var clientId = "testClient";
            var authorizationCode = await _oidcClientActor.InitiateAuthenticationAsync(redirectUri);
            var oidcResponse = await _oidcClientActor.ExchangeAuthorizationCodeAsync(authorizationCode, redirectUri, clientId);

            var isValid = await _credentialVerifierActor.ValidateTokenAsync(oidcResponse.IdToken);

            Assert.True(isValid);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnFalse_ForExpiredToken()
        {
            var expiredVc = CredentialGenerator.GenerateExpiredCredential("user123", new Dictionary<string, string>());
            var redirectUri = "https://testapp.com/callback";
            var clientId = "testClient";
            var authorizationCode = await _oidcClientActor.InitiateAuthenticationAsync(redirectUri);
            var oidcResponse = await _oidcClientActor.ExchangeAuthorizationCodeAsync(authorizationCode, redirectUri, clientId);

            // Simulate token expiration by waiting
            await Task.Delay(TimeSpan.FromSeconds(1));

            var isValid = await _credentialVerifierActor.ValidateTokenAsync(oidcResponse.AccessToken);

            Assert.False(isValid);
        }

        [Fact]
        public async Task RevokeCredential_ShouldInvalidateToken()
        {
            var vc = CredentialGenerator.GenerateUserCredential("user123", new Dictionary<string, string>());
            var redirectUri = "https://testapp.com/callback";
            var clientId = "testClient";
            var authorizationCode = await _oidcClientActor.InitiateAuthenticationAsync(redirectUri);
            var oidcResponse = await _oidcClientActor.ExchangeAuthorizationCodeAsync(authorizationCode, redirectUri, clientId);

            await _oidcProviderService.RevokeCredentialAsync(vc.Id);

            var isValid = await _credentialVerifierActor.ValidateTokenAsync(oidcResponse.AccessToken);

            Assert.False(isValid);
        }
    }

    public class CanaryTestFixture : IDisposable
    {
        public IDecentralizedOIDCProviderService OidcProviderService { get; }
        public ICredentialVerifierActor CredentialVerifierActor { get; }
        public IOIDCClientActor OidcClientActor { get; }

        public CanaryTestFixture()
        {
            var services = new ServiceCollection();

            var webAppVc = new VerifiableCredential
            {
                Id = Guid.NewGuid().ToString(),
                Authority = "https://test-authority.com",
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                Claims = new Dictionary<string, string>
                {
                    { "aud", "test-audience" },
                    { "iss", "test-issuer" }
                }
            };

            services.AddCanaryActorSystem(options =>
            {
                options.ActorSystemName = "TestActorSystem";
                options.ActorFramework = "in-memory";
            });

            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<ICryptoService, CryptoService>();
            services.AddSingleton<IKeyStore, InMemoryKeyStore>();
            services.AddSingleton<IKeyManagementService, KeyManagementService>();

            services.AddTransient<ICredentialVerifierActor>(sp => new CredentialVerifierActor(
                Guid.NewGuid().ToString(),
                sp.GetRequiredService<ICryptoService>(),
                sp.GetRequiredService<IRevocationManagerActor>(),
                sp.GetRequiredService<ILogger<CredentialVerifierActor>>(),
                webAppVc
            ));

            services.AddTransient<IOIDCClientActor>(sp => new OIDCClientActor(
                Guid.NewGuid().ToString(),
                sp.GetRequiredService<IActorStateManager>(),
                sp.GetRequiredService<IActorMessageBus>(),
                sp.GetRequiredService<ILogger<OIDCClientActor>>()
            ));

            var serviceProvider = services.BuildServiceProvider();

            OidcProviderService = serviceProvider.GetRequiredService<DecentralizedOIDCProvider>();
            CredentialVerifierActor = serviceProvider.GetRequiredService<ICredentialVerifierActor>();
            OidcClientActor = serviceProvider.GetRequiredService<IOIDCClientActor>();
        }

        public void Dispose()
        {
            // Cleanup if necessary
        }
    }
}