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
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.VerifiableCredentialAsRootOfTrustActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.TrustFrameworkManagerActor;
using Rebel.Alliance.Canary.InMemoryActorFramework.Actors.RevocationManagerActor;
using Rebel.Alliance.Canary.VerifiableCredentials.Messaging;
using Rebel.Alliance.Canary.InMemoryActorFramework.ActorSystem;

namespace Rebel.Alliance.Canary.Tests
{
    public class CanaryOidcTests : IClassFixture<CanaryTestFixture>
    {
        private readonly IDecentralizedOIDCProviderService _oidcProviderService;
        private readonly ICredentialVerifierActor _credentialVerifierActor;
        private readonly IOIDCClientActor _oidcClientActor;
        private readonly IVerifiableCredentialActor _verifiableCredentialActor;
        private readonly IVerifiableCredentialAsRootOfTrustActor _verifiableCredentialAsRootOfTrustActor;
        private readonly ITrustFrameworkManagerActor _trustFrameworkManagerActor;
        private readonly IRevocationManagerActor _revocationManagerActor;

        public CanaryOidcTests(CanaryTestFixture fixture)
        {
            _oidcProviderService = fixture.OidcProviderService;
            _credentialVerifierActor = fixture.CredentialVerifierActor;
            _oidcClientActor = fixture.OidcClientActor;
            _verifiableCredentialActor = fixture.VerifiableCredentialActor;
            _verifiableCredentialAsRootOfTrustActor = fixture.VerifiableCredentialAsRootOfTrustActor;
            _trustFrameworkManagerActor = fixture.TrustFrameworkManagerActor;
            _revocationManagerActor = fixture.RevocationManagerActor;
        }

        [Fact]
        public async Task CreateAndVerifyRootCredential_ShouldSucceed()
        {
            var issuerId = "root_issuer";
            var claims = new Dictionary<string, string> { { "type", "root" } };
            var masterKeyId = "master_key_1";

            var createRootMsg = new CreateRootCredentialMessage(issuerId, claims, masterKeyId);
            var rootCredential = await _verifiableCredentialAsRootOfTrustActor.CreateRootCredentialAsync(issuerId, claims, masterKeyId);

            Assert.NotNull(rootCredential);
            Assert.Equal(issuerId, rootCredential.Issuer);

            var verifyChainMsg = new VerifyCredentialChainMessage(rootCredential, rootCredential);
            var isValid = await _verifiableCredentialAsRootOfTrustActor.VerifyCredentialChainAsync(rootCredential, rootCredential);

            Assert.True(isValid);
        }

        [Fact]
        public async Task IssueAndVerifySubordinateCredential_ShouldSucceed()
        {
            // First, create a root credential
            var rootIssuerId = "root_issuer";
            var rootClaims = new Dictionary<string, string> { { "type", "root" } };
            var masterKeyId = "master_key_1";

            var rootCredential = await _verifiableCredentialAsRootOfTrustActor.CreateRootCredentialAsync(rootIssuerId, rootClaims, masterKeyId);

            // Now, issue a subordinate credential
            var subIssuerId = "sub_issuer";
            var subClaims = new Dictionary<string, string> { { "type", "subordinate" } };
            var derivedKeyId = "derived_key_1";

            var issueSubMsg = new IssueSubordinateCredentialMessage(subIssuerId, rootCredential, subClaims, derivedKeyId);
            var subCredential = await _verifiableCredentialAsRootOfTrustActor.IssueSubordinateCredentialAsync(subIssuerId, rootCredential, subClaims, derivedKeyId);

            Assert.NotNull(subCredential);
            Assert.Equal(subIssuerId, subCredential.Issuer);
            Assert.Equal(rootCredential.Id, subCredential.ParentCredentialId);

            var verifyChainMsg = new VerifyCredentialChainMessage(subCredential, rootCredential);
            var isValid = await _verifiableCredentialAsRootOfTrustActor.VerifyCredentialChainAsync(subCredential, rootCredential);

            Assert.True(isValid);
        }

        [Fact]
        public async Task RevokeCredential_ShouldInvalidateCredential()
        {
            var credential = CredentialGenerator.GenerateUserCredential("user123", new Dictionary<string, string>());

            var revokeMsg = new RevokeCredentialMessage(credential.Id);
            await _revocationManagerActor.RevokeCredentialAsync(credential.Id);

            var isRevokedMsg = new IsCredentialRevokedMessage(credential.Id);
            var isRevoked = await _revocationManagerActor.IsCredentialRevokedAsync(credential.Id);

            Assert.True(isRevoked);
        }

        [Fact]
        public async Task RegisterAndVerifyIssuer_ShouldSucceed()
        {
            var issuerId = "test_issuer";
            var publicKey = "test_public_key";

            var registerMsg = new RegisterIssuerMessage(issuerId, publicKey);
            var isRegistered = await _trustFrameworkManagerActor.RegisterIssuerAsync(issuerId, publicKey);

            Assert.True(isRegistered);

            var isTrustedMsg = new IsTrustedIssuerMessage(issuerId);
            var isTrusted = await _trustFrameworkManagerActor.IsTrustedIssuerAsync(issuerId);

            Assert.True(isTrusted);
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
        public IVerifiableCredentialActor VerifiableCredentialActor { get; }
        public IVerifiableCredentialAsRootOfTrustActor VerifiableCredentialAsRootOfTrustActor { get; }
        public ITrustFrameworkManagerActor TrustFrameworkManagerActor { get; }
        public IRevocationManagerActor RevocationManagerActor { get; }

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

            // Register all actor types
            services.AddTransient<ICredentialVerifierActor, CredentialVerifierActor>();
            services.AddTransient<IOIDCClientActor, OIDCClientActor>();
            services.AddTransient<IVerifiableCredentialActor, VerifiableCredentialActor>();
            services.AddTransient<IVerifiableCredentialAsRootOfTrustActor, VerifiableCredentialAsRootOfTrustActor>();
            services.AddTransient<ITrustFrameworkManagerActor, TrustFrameworkManagerActor>();
            services.AddTransient<IRevocationManagerActor, RevocationManagerActor>();

            var serviceProvider = services.BuildServiceProvider();

            OidcProviderService = serviceProvider.GetRequiredService<DecentralizedOIDCProvider>();
            CredentialVerifierActor = serviceProvider.GetRequiredService<ICredentialVerifierActor>();
            OidcClientActor = serviceProvider.GetRequiredService<IOIDCClientActor>();
            VerifiableCredentialActor = serviceProvider.GetRequiredService<IVerifiableCredentialActor>();
            VerifiableCredentialAsRootOfTrustActor = serviceProvider.GetRequiredService<IVerifiableCredentialAsRootOfTrustActor>();
            TrustFrameworkManagerActor = serviceProvider.GetRequiredService<ITrustFrameworkManagerActor>();
            RevocationManagerActor = serviceProvider.GetRequiredService<IRevocationManagerActor>();
        }

        public void Dispose()
        {
            // Cleanup if necessary
        }
    }
}