# Unit Test for VerifiableCredentialService

```csharp
using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SecureMessagingApp.Services;
using SecureMessagingApp.Models;

namespace SecureMessagingApp.Tests
{
public class VerifiableCredentialServiceTests
{
private readonly Mock<ILogger<VerifiableCredentialService>> _loggerMock;
private readonly IVerifiableCredentialService _verifiableCredentialService;
private readonly Mock<ICryptoService> _cryptoServiceMock;

public VerifiableCredentialServiceTests()
{
_loggerMock = new Mock<ILogger<VerifiableCredentialService>>();
_cryptoServiceMock = new Mock<ICryptoService>();
_verifiableCredentialService = new VerifiableCredentialService(_cryptoServiceMock.Object, _loggerMock.Object);
}

[Fact]
public void CreateCredential_ShouldGenerateSignedCredential()
{
// Arrange
var publicKey = new byte[] { 1, 2, 3 };
var privateKey = new byte[] { 4, 5, 6 };
var masterKey = new MasterKey(publicKey, privateKey, "mnemonic", "forwardMnemonic");

var subject = "did:example:123";
var credentialData = new Dictionary<string, string>
{
{ "alumniOf", "Example University" }
};

var expectedSignature = new byte[] { 7, 8, 9 };
_cryptoServiceMock.Setup(cs => cs.SignData(privateKey, It.IsAny<string>())).Returns(expectedSignature);

// Act
var credential = _verifiableCredentialService.CreateCredential(masterKey, subject, credentialData);

// Assert
Assert.NotNull(credential);
Assert.Equal(Convert.ToBase64String(publicKey), credential.Issuer);
Assert.Equal(subject, credential.CredentialSubject["id"]);
Assert.Equal(credentialData["alumniOf"], credential.CredentialSubject["alumniOf"]);
Assert.Equal(Convert.ToBase64String(expectedSignature), credential.Proof.Jws);
}

[Fact]
public void VerifyCredential_ShouldReturnTrueForValidSignature()
{
// Arrange
var publicKey = new byte[] { 1, 2, 3 };
var privateKey = new byte[] { 4, 5, 6 };
var masterKey = new MasterKey(publicKey, privateKey, "mnemonic", "forwardMnemonic");

var subject = "did:example:123";
var credentialData = new Dictionary<string, string>
{
{ "alumniOf", "Example University" }
};

var credential = new VerifiableCredential
{
Id = $"urn:uuid:{Guid.NewGuid()}",
Issuer = Convert.ToBase64String(publicKey),
IssuanceDate = DateTime.UtcNow,
CredentialSubject = credentialData,
Proof = new Proof
{
Created = DateTime.UtcNow,
VerificationMethod = Convert.ToBase64String(publicKey),
Jws = Convert.ToBase64String(new byte[] { 7, 8, 9 })
}
};

var serializedCredential = "Issuer:...,Subject:...,IssuedAt:...,ExpiresAt:...,alumniOf:Example University";
_cryptoServiceMock.Setup(cs => cs.VerifyData(publicKey, It.IsAny<string>(), It.IsAny<byte[]>())).Returns(true);

// Act
var isVerified = _verifiableCredentialService.VerifyCredential(credential);

// Assert
Assert.True(isVerified);
}
}
}

```