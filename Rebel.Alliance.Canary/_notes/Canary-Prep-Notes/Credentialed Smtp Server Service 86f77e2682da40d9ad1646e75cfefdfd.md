# Credentialed Smtp Server Service

## Service

```csharp
using Dapr.Actors;
using Dapr.Actors.Client;
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

public class CredentialedSmtpServerService
{
    private readonly ICryptoService _cryptoService;
    private readonly string _credentialVerifierActorType;
    private readonly string _credentialHolderActorType;

    public CredentialedSmtpServerService(ICryptoService cryptoService, string credentialVerifierActorType, string credentialHolderActorType)
    {
        _cryptoService = cryptoService;
        _credentialVerifierActorType = credentialVerifierActorType;
        _credentialHolderActorType = credentialHolderActorType;
    }

    public void Start()
    {
        var options = new SmtpServerOptionsBuilder()
            .ServerName(Environment.GetEnvironmentVariable("SMTP_SERVER_NAME") ?? "localhost")
            .Port(25, 587) // default SMTP port and submission port
            .Build();

        var smtpServer = new SmtpServer.SmtpServer(options, new MailboxFilter(_credentialVerifierActorType, _credentialHolderActorType), new SimpleMailboxMessageStore());

        var cancellationTokenSource = new CancellationTokenSource();

        smtpServer.StartAsync(cancellationTokenSource.Token).Wait();
    }

    private class MailboxFilter : IMailboxFilter
    {
        private readonly string _credentialVerifierActorType;
        private readonly string _credentialHolderActorType;

        public MailboxFilter(string credentialVerifierActorType, string credentialHolderActorType)
        {
            _credentialVerifierActorType = credentialVerifierActorType;
            _credentialHolderActorType = credentialHolderActorType;
        }

        private ICredentialVerifierActor GetCredentialVerifierActor(string actorId)
        {
            var actorIdObj = new ActorId(actorId);
            return ActorProxy.Create<ICredentialVerifierActor>(actorIdObj, _credentialVerifierActorType);
        }

        private ICredentialHolderActor GetCredentialHolderActor(string actorId)
        {
            var actorIdObj = new ActorId(actorId);
            return ActorProxy.Create<ICredentialHolderActor>(actorIdObj, _credentialHolderActorType);
        }

        public async Task<SmtpResponse> CanDeliverToAsync(ISessionContext context, IMessageTransaction transaction, Mailbox to, CancellationToken cancellationToken)
        {
            var credentialHolder = GetCredentialHolderActor(to.User);
            var credential = await credentialHolder.PresentCredentialAsync(to.User);

            var credentialVerifier = GetCredentialVerifierActor(to.User);
            if (await credentialVerifier.VerifyCredentialAsync(credential))
            {
                return SmtpResponse.Ok;
            }

            return SmtpResponse.MailboxUnavailable;
        }

        public async Task<SmtpResponse> CanAcceptFromAsync(ISessionContext context, IMessageTransaction transaction, Mailbox from, CancellationToken cancellationToken)
        {
            var credentialHolder = GetCredentialHolderActor(from.User);
            var credential = await credentialHolder.PresentCredentialAsync(from.User);

            var credentialVerifier = GetCredentialVerifierActor(from.User);
            if (await credentialVerifier.VerifyCredentialAsync(credential))
            {
                return SmtpResponse.Ok;
            }

            return SmtpResponse.MailboxUnavailable;
        }
    }
}

```

## Smtp Client Service

```csharp
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Dapr.Actors;
using Dapr.Actors.Client;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

public class SmtpClientService
{
    private readonly ICryptoService _cryptoService;
    private readonly string _credentialHolderActorType;

    public SmtpClientService(ICryptoService cryptoService, string credentialHolderActorType)
    {
        _cryptoService = cryptoService;
        _credentialHolderActorType = credentialHolderActorType;
    }

    private ICredentialHolderActor GetCredentialHolderActor(string actorId)
    {
        var actorIdObj = new ActorId(actorId);
        return ActorProxy.Create<ICredentialHolderActor>(actorIdObj, _credentialHolderActorType);
    }

    public async Task SendEmailAsync(string from, string to, string subject, string body)
    {
        var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "localhost";
        var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
        var enableSsl = bool.Parse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true");

        var fromCredentialHolder = GetCredentialHolderActor(from);
        var fromCredential = await fromCredentialHolder.PresentCredentialAsync(from);

        var toCredentialHolder = GetCredentialHolderActor(to);
        var toCredential = await toCredentialHolder.PresentCredentialAsync(to);

        if (!_cryptoService.VerifyData(Convert.FromBase64String(fromCredential.Proof.VerificationMethod), from, Convert.FromBase64String(fromCredential.Proof.Jws)))
        {
            throw new InvalidOperationException("Sender's credential is not valid");
        }

        if (!_cryptoService.VerifyData(Convert.FromBase64String(toCredential.Proof.VerificationMethod), to, Convert.FromBase64String(toCredential.Proof.Jws)))
        {
            throw new InvalidOperationException("Recipient's credential is not valid");
        }

        var client = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(from, smtpPassword),
            EnableSsl = enableSsl
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

## **Dockerfile**

```docker
# Use the official .NET 6.0 runtime image as a base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 25

# Use the official .NET 6.0 SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SecureMessagingApp/SecureMessagingApp.csproj", "SecureMessagingApp/"]
RUN dotnet restore "SecureMessagingApp/SecureMessagingApp.csproj"
COPY . .
WORKDIR "/src/SecureMessagingApp"
RUN dotnet build "SecureMessagingApp.csproj" -c Release -o /app/build

# Publish the app to the /app/publish directory
FROM build AS publish
RUN dotnet publish "SecureMessagingApp.csproj" -c Release -o /app/publish

# Use the base image to run the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecureMessagingApp.dll"]

```

## Powershell

1. **Login to ACR**:
2. 

```powershell
az acr login --name <your-acr-name>
```

1. **Build the Docker Image**:

```powershell
docker build -t <your-acr-name>.azurecr.io/credentialed-smtp-server:latest .
```

1. **Push the Docker Image to ACR**:

```powershell
docker push <your-acr-name>.azurecr.io/credentialed-smtp-server:latest
```

1. **Deploy to Azure Container Apps**
    
    A. **Create an Azure Container App Environment**
    
    ```powershell
    az containerapp env create --name my-environment --resource-group my-resource-group --location eastus
    ```
    
    B. **Deploy the Container App**:
    
    ```powershell
    az containerapp create \
      --name credentialed-smtp-server \
      --resource-group my-resource-group \
      --environment my-environment \
      --image <your-acr-name>.azurecr.io/credentialed-smtp-server:latest \
      --target-port 80 \
      --env-vars SMTP_SERVER_NAME=smtp.server.name \
      --registry-server <your-acr-name>.azurecr.io \
      --registry-username <acr-username> \
      --registry-password <acr-password>
    ```
    
2. **Configure Environment Variables and Secrets**
Use Azure Key Vault and Azure App Configuration to securely manage secrets and environment variables. For example:

```powershell
az keyvault secret set --vault-name <your-keyvault-name> --name SMTPServerName --value "smtp.server.name"
az keyvault secret set --vault-name <your-keyvault-name> --name SMTPPassword --value "<your-password>"
```

Then link these secrets to your Azure Container App environment variables:

```powershell
az containerapp update \
  --name credentialed-smtp-server \
  --resource-group my-resource-group \
  --set-env-vars SMTP_SERVER_NAME=@Microsoft.KeyVault(VaultName=<your-keyvault-name>;SecretName=SMTPServerName) \
                 SMTP_PASSWORD=@Microsoft.KeyVault(VaultName=<your-keyvault-name>;SecretName=SMTPPassword
```