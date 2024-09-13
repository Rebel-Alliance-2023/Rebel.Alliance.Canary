using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Rebel.Alliance.Canary.Models;
using MediatR;
using Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem;
using Microsoft.Extensions.DependencyInjection;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.OIDCClientActor;
using Rebel.Alliance.Canary.InMemoryActorFramework;
using Rebel.Alliance.Canary.OIDC.Models;
using Rebel.Alliance.Canary.OIDC.Services;
using Rebel.Alliance.Canary.Security;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;
using Rebel.Alliance.Canary.VerifiableCredentials.Generator;
using Rebel.Alliance.Canary.Configuration;

namespace Rebel.Alliance.Canary.Tests
{
    public class CanaryOidcTests : IClassFixture<CanaryTestFixture>
    {
        private readonly IDecentralizedOIDCProviderService _oidcProviderService;

        public CanaryOidcTests(CanaryTestFixture fixture)
        {
            _oidcProviderService = fixture.OidcProviderService;
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
            var vc = CredentialGenerator.GenerateUserCredential("user123", new Dictionary<string, string>());
            var authRequest = new AuthenticationRequest
            {
                ClientId = "testClient",
                RedirectUri = "https://testapp.com/callback",
                Scope = "openid profile",
                VerifiableCredential = vc
            };

            var authResponse = await _oidcProviderService.InitiateAuthenticationAsync(authRequest);

            Assert.NotNull(authResponse);
            Assert.NotNull(authResponse.AuthorizationCode);
        }

        [Fact]
        public async Task ExchangeAuthorizationCode_ShouldReturnTokens()
        {
            var vc = CredentialGenerator.GenerateUserCredential("user123", new Dictionary<string, string>());
            var authRequest = new AuthenticationRequest
            {
                ClientId = "testClient",
                RedirectUri = "https://testapp.com/callback",
                Scope = "openid profile",
                VerifiableCredential = vc
            };

            var authResponse = await _oidcProviderService.InitiateAuthenticationAsync(authRequest);
            var tokenRequest = new TokenRequest
            {
                ClientId = "testClient",
                ClientSecret = "testSecret",
                GrantType = "authorization_code",
                Code = authResponse.AuthorizationCode,
                RedirectUri = "https://testapp.com/callback"
            };

            TokenResponse tokenResponse = await _oidcProviderService.ExchangeAuthorizationCodeAsync(tokenRequest);

            Assert.NotNull(tokenResponse);
            Assert.NotNull(tokenResponse.AccessToken);
            Assert.NotNull(tokenResponse.IdToken);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnTrue_ForValidToken()
        {
            var vc = CredentialGenerator.GenerateUserCredential("user123", new Dictionary<string, string>());
            var authRequest = new AuthenticationRequest
            {
                ClientId = "testClient",
                RedirectUri = "https://testapp.com/callback",
                Scope = "openid profile",
                VerifiableCredential = vc
            };

            var authResponse = await _oidcProviderService.InitiateAuthenticationAsync(authRequest);
            var tokenRequest = new TokenRequest
            {
                ClientId = "testClient",
                ClientSecret = "testSecret",
                GrantType = "authorization_code",
                Code = authResponse.AuthorizationCode,
                RedirectUri = "https://testapp.com/callback"
            };

            var tokenResponse = await _oidcProviderService.ExchangeAuthorizationCodeAsync(tokenRequest);
            var isValid = await _oidcProviderService.ValidateTokenAsync(tokenResponse.AccessToken);

            Assert.True(isValid);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnFalse_ForExpiredToken()
        {
            var expiredVc = CredentialGenerator.GenerateExpiredCredential("user123", new Dictionary<string, string>());
            var authRequest = new AuthenticationRequest
            {
                ClientId = "testClient",
                RedirectUri = "https://testapp.com/callback",
                Scope = "openid profile",
                VerifiableCredential = expiredVc
            };

            var authResponse = await _oidcProviderService.InitiateAuthenticationAsync(authRequest);
            var tokenRequest = new TokenRequest
            {
                ClientId = "testClient",
                ClientSecret = "testSecret",
                GrantType = "authorization_code",
                Code = authResponse.AuthorizationCode,
                RedirectUri = "https://testapp.com/callback"
            };

            var tokenResponse = await _oidcProviderService.ExchangeAuthorizationCodeAsync(tokenRequest);
            var isValid = await _oidcProviderService.ValidateTokenAsync(tokenResponse.AccessToken);

            Assert.False(isValid);
        }

        [Fact]
        public async Task RevokeCredential_ShouldInvalidateToken()
        {
            var vc = CredentialGenerator.GenerateUserCredential("user123", new Dictionary<string, string>());
            var authRequest = new AuthenticationRequest
            {
                ClientId = "testClient",
                RedirectUri = "https://testapp.com/callback",
                Scope = "openid profile",
                VerifiableCredential = vc
            };

            var authResponse = await _oidcProviderService.InitiateAuthenticationAsync(authRequest);
            var tokenRequest = new TokenRequest
            {
                ClientId = "testClient",
                ClientSecret = "testSecret",
                GrantType = "authorization_code",
                Code = authResponse.AuthorizationCode,
                RedirectUri = "https://testapp.com/callback"
            };

            var tokenResponse = await _oidcProviderService.ExchangeAuthorizationCodeAsync(tokenRequest);

            await _oidcProviderService.RevokeCredentialAsync(vc.Id);

            var isValid = await _oidcProviderService.ValidateTokenAsync(tokenResponse.AccessToken);

            Assert.False(isValid);
        }
    }

    public class CanaryTestFixture : IDisposable
    {
        public IDecentralizedOIDCProviderService OidcProviderService { get; }

        public CanaryTestFixture()
        {
            var services = new ServiceCollection();

            services.AddCanaryActorSystem(options =>
            {
                options.ActorSystemName = "TestActorSystem";
                options.ActorFramework = "in-memory";
            });

            var serviceProvider = services.BuildServiceProvider();

            OidcProviderService = serviceProvider.GetRequiredService<DecentralizedOIDCProvider>();
        }

        public void Dispose()
        {
            // Cleanup if necessary
        }
    }


}