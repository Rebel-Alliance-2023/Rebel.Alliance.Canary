# VerifiedMessengerService

## Interface

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IVerifiedMessengerService
{
    Task RegisterUserAsync(string userId, string publicKey);
    Task<VerifiableCredential> IssueUserCredentialAsync(string userId);
    Task SendMessageAsync(string senderId, string recipientId, string message);
    Task<List<string>> ReceiveMessagesAsync(string userId);
}

```

## Service

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

public class VerifiedMessengerService : IVerifiedMessengerService
{
    private readonly ICredentialIssuerActor _credentialIssuer;
    private readonly ICredentialVerifierActor _credentialVerifier;
    private readonly ICredentialHolderActor _credentialHolder;
    private readonly ICryptoService _cryptoService;

    // Simulate message storage
    private readonly Dictionary<string, List<(string SenderId, string Message)>> _messageStore = new Dictionary<string, List<(string SenderId, string Message)>>();

    public VerifiedMessengerService(
        ICredentialIssuerActor credentialIssuer,
        ICredentialVerifierActor credentialVerifier,
        ICredentialHolderActor credentialHolder,
        ICryptoService cryptoService)
    {
        _credentialIssuer = credentialIssuer;
        _credentialVerifier = credentialVerifier;
        _credentialHolder = credentialHolder;
        _cryptoService = cryptoService;
    }

    public async Task RegisterUserAsync(string userId, string publicKey)
    {
        // Register user and store public key (simulate user registration)
        await Task.CompletedTask;
    }

    public async Task<VerifiableCredential> IssueUserCredentialAsync(string userId)
    {
        // Issue a verifiable credential for the user
        var claims = new Dictionary<string, string> { { "userId", userId } };
        return await _credentialIssuer.IssueCredentialAsync(userId, claims);
    }

    public async Task SendMessageAsync(string senderId, string recipientId, string message)
    {
        // Verify sender's credential
        var senderCredential = await _credentialHolder.PresentCredentialAsync(senderId);
        if (!await _credentialVerifier.VerifyCredentialAsync(senderCredential))
        {
            throw new InvalidOperationException("Sender's credential is not valid");
        }

        // Sign the message with the sender's private key
        var signature = _cryptoService.SignData(senderCredential.Proof.VerificationMethod, message);

        // Store the message along with the sender's ID
        if (!_messageStore.ContainsKey(recipientId))
        {
            _messageStore[recipientId] = new List<(string SenderId, string Message)>();
        }
        _messageStore[recipientId].Add((senderId, Convert.ToBase64String(signature) + ":" + message));

        await Task.CompletedTask;
    }

    public async Task<List<string>> ReceiveMessagesAsync(string userId)
    {
        // Retrieve messages for the user
        if (!_messageStore.ContainsKey(userId))
        {
            return new List<string>();
        }

        var messages = new List<string>();
        foreach (var (senderId, signedMessage) in _messageStore[userId])
        {
            var parts = signedMessage.Split(':');
            var signature = Convert.FromBase64String(parts[0]);
            var message = parts[1];

            // Verify sender's credential
            var senderCredential = await _credentialHolder.PresentCredentialAsync(senderId);
            if (!await _credentialVerifier.VerifyCredentialAsync(senderCredential))
            {
                throw new InvalidOperationException("Sender's credential is not valid");
            }

            // Verify the message signature
            if (!_cryptoService.VerifyData(senderCredential.Proof.VerificationMethod, message, signature))
            {
                throw new InvalidOperationException("Message signature is not valid");
            }

            messages.Add(message);
        }
        
    public async Task SendMessageAsync(string senderId, string recipientId, string message)
    {
        // Send message using SMTP client
        await _smtpClientService.SendEmailAsync(senderId, recipientId, "New Message", message);
    }
        return messages;
    }

    public void StartSmtpServer()
    {
        _smtpServerService.Start();
    }
    
}

```