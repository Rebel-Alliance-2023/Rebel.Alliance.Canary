# Custom SMTP Server

## Service:

```csharp
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

public class SmtpServerService
{
    private readonly ICredentialVerifierActor _credentialVerifier;
    private readonly ICredentialHolderActor _credentialHolder;

    public SmtpServerService(ICredentialVerifierActor credentialVerifier, ICredentialHolderActor credentialHolder)
    {
        _credentialVerifier = credentialVerifier;
        _credentialHolder = credentialHolder;
    }

    public void Start()
    {
        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Port(25, 587) // default SMTP port and submission port
            .Build();

        var smtpServer = new SmtpServer.SmtpServer(options, new MailboxFilter(_credentialVerifier, _credentialHolder), new SimpleMailboxMessageStore());

        var cancellationTokenSource = new CancellationTokenSource();

        smtpServer.StartAsync(cancellationTokenSource.Token).Wait();
    }

    private class MailboxFilter : IMailboxFilter
    {
        private readonly ICredentialVerifierActor _credentialVerifier;
        private readonly ICredentialHolderActor _credentialHolder;

        public MailboxFilter(ICredentialVerifierActor credentialVerifier, ICredentialHolderActor credentialHolder)
        {
            _credentialVerifier = credentialVerifier;
            _credentialHolder = credentialHolder;
        }

        public async Task<SmtpResponse> CanDeliverToAsync(ISessionContext context, IMessageTransaction transaction, Mailbox to, CancellationToken cancellationToken)
        {
            var credential = await _credentialHolder.PresentCredentialAsync(to.User);
            if (await _credentialVerifier.VerifyCredentialAsync(credential))
            {
                return SmtpResponse.Ok;
            }

            return SmtpResponse.MailboxUnavailable;
        }

        public async Task<SmtpResponse> CanAcceptFromAsync(ISessionContext context, IMessageTransaction transaction, Mailbox from, CancellationToken cancellationToken)
        {
            var credential = await _credentialHolder.PresentCredentialAsync(from.User);
            if (await _credentialVerifier.VerifyCredentialAsync(credential))
            {
                return SmtpResponse.Ok;
            }

            return SmtpResponse.MailboxUnavailable;
        }
    }
}

```

## Client

```csharp
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

public class SmtpClientService
{
    private readonly ICredentialHolderActor _credentialHolder;
    private readonly ICryptoService _cryptoService;

    public SmtpClientService(ICredentialHolderActor credentialHolder, ICryptoService cryptoService)
    {
        _credentialHolder = credentialHolder;
        _cryptoService = cryptoService;
    }

    public async Task SendEmailAsync(string from, string to, string subject, string body)
    {
        var fromCredential = await _credentialHolder.PresentCredentialAsync(from);
        var toCredential = await _credentialHolder.PresentCredentialAsync(to);

        if (!_cryptoService.VerifyData(Convert.FromBase64String(fromCredential.Proof.VerificationMethod), from, Convert.FromBase64String(fromCredential.Proof.Jws)))
        {
            throw new InvalidOperationException("Sender's credential is not valid");
        }

        if (!_cryptoService.VerifyData(Convert.FromBase64String(toCredential.Proof.VerificationMethod), to, Convert.FromBase64String(toCredential.Proof.Jws)))
        {
            throw new InvalidOperationException("Recipient's credential is not valid");
        }

        var client = new SmtpClient("localhost", 25)
        {
            Credentials = new NetworkCredential(from, "password"), // Use a valid password or a token if needed
            EnableSsl = true
        };

        var message = new MailMessage(from, to)
        {
            Subject = subject,
            Body = body
        };

        await client.SendMailAsync(message);
    }
}

```