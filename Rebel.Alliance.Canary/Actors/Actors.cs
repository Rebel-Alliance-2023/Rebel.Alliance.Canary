using System;
using System.Threading.Tasks;
using MediatR;
using Rebel.Alliance.Canary.Abstractions;
using Rebel.Alliance.Canary.Messaging;

namespace Rebel.Alliance.Canary.Actors
{
    public class VerifiableCredentialActorBase : ActorBase
    {
        public VerifiableCredentialActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
            SetActorStateManager(stateManager);
            SetMediator(mediator);
        }

        public override Task ReceiveAsync(IActorMessage message)
        {
            // Implementation for VerifiableCredentialActor
            return Task.CompletedTask;
        }
    }
    public abstract class CredentialIssuerActorBase : ActorBase
    {
        protected CredentialIssuerActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
        }

        public override async Task ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case IssueCredentialMessage issueMsg:
                    await HandleIssueCredentialAsync(issueMsg);
                    break;
                case SignCredentialMessage signMsg:
                    await HandleSignCredentialAsync(signMsg);
                    break;
                case ValidateIssuerMessage validateMsg:
                    await HandleValidateIssuerAsync(validateMsg);
                    break;
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        protected abstract Task HandleIssueCredentialAsync(IssueCredentialMessage message);
        protected abstract Task HandleSignCredentialAsync(SignCredentialMessage message);
        protected abstract Task HandleValidateIssuerAsync(ValidateIssuerMessage message);
    }

    public abstract class CredentialVerifierActorBase : ActorBase
    {
        protected CredentialVerifierActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
        }

        public override async Task ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case VerifyCredentialMessage verifyMsg:
                    await HandleVerifyCredentialAsync(verifyMsg);
                    break;
                case CheckSignatureMessage checkSignatureMsg:
                    await HandleCheckSignatureAsync(checkSignatureMsg);
                    break;
                case ValidateRevocationMessage validateRevocationMsg:
                    await HandleValidateRevocationAsync(validateRevocationMsg);
                    break;
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        protected abstract Task HandleVerifyCredentialAsync(VerifyCredentialMessage message);
        protected abstract Task HandleCheckSignatureAsync(CheckSignatureMessage message);
        protected abstract Task HandleValidateRevocationAsync(ValidateRevocationMessage message);
    }

    public abstract class CredentialHolderActorBase : ActorBase
    {
        protected CredentialHolderActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
        }

        public override async Task ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case StoreCredentialMessage storeMsg:
                    await HandleStoreCredentialAsync(storeMsg);
                    break;
                case PresentCredentialMessage presentMsg:
                    await HandlePresentCredentialAsync(presentMsg);
                    break;
                case RenewCredentialMessage renewMsg:
                    await HandleRenewCredentialAsync(renewMsg);
                    break;
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        protected abstract Task HandleStoreCredentialAsync(StoreCredentialMessage message);
        protected abstract Task HandlePresentCredentialAsync(PresentCredentialMessage message);
        protected abstract Task HandleRenewCredentialAsync(RenewCredentialMessage message);
    }

    public abstract class RevocationManagerActorBase : ActorBase
    {
        protected RevocationManagerActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
        }

        public override async Task ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case RevokeCredentialMessage revokeMsg:
                    await HandleRevokeCredentialAsync(revokeMsg);
                    break;
                case UpdateRegistryMessage updateMsg:
                    await HandleUpdateRegistryAsync(updateMsg);
                    break;
                case NotifyRevocationMessage notifyMsg:
                    await HandleNotifyRevocationAsync(notifyMsg);
                    break;
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        protected abstract Task HandleRevokeCredentialAsync(RevokeCredentialMessage message);
        protected abstract Task HandleUpdateRegistryAsync(UpdateRegistryMessage message);
        protected abstract Task HandleNotifyRevocationAsync(NotifyRevocationMessage message);
    }

    public abstract class TrustFrameworkManagerActorBase : ActorBase
    {
        protected TrustFrameworkManagerActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
        }

        public override async Task ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case RegisterIssuerMessage registerMsg:
                    await HandleRegisterIssuerAsync(registerMsg);
                    break;
                case CertifyIssuerMessage certifyMsg:
                    await HandleCertifyIssuerAsync(certifyMsg);
                    break;
                case RevokeIssuerMessage revokeMsg:
                    await HandleRevokeIssuerAsync(revokeMsg);
                    break;
                case IsTrustedIssuerMessage trustMsg:
                    await HandleIsTrustedIssuerAsync(trustMsg);
                    break;
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        protected abstract Task HandleRegisterIssuerAsync(RegisterIssuerMessage message);
        protected abstract Task HandleCertifyIssuerAsync(CertifyIssuerMessage message);
        protected abstract Task HandleRevokeIssuerAsync(RevokeIssuerMessage message);
        protected abstract Task HandleIsTrustedIssuerAsync(IsTrustedIssuerMessage message);
    }

    public abstract class VerifiableCredentialAsRootOfTrustActorBase : ActorBase
    {
        protected VerifiableCredentialAsRootOfTrustActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
        }

        public override async Task ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case CreateRootCredentialMessage createRootMsg:
                    await HandleCreateRootCredentialAsync(createRootMsg);
                    break;
                case IssueSubordinateCredentialMessage issueSubMsg:
                    await HandleIssueSubordinateCredentialAsync(issueSubMsg);
                    break;
                case VerifyCredentialChainMessage verifyChainMsg:
                    await HandleVerifyCredentialChainAsync(verifyChainMsg);
                    break;
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        protected abstract Task HandleCreateRootCredentialAsync(CreateRootCredentialMessage message);
        protected abstract Task HandleIssueSubordinateCredentialAsync(IssueSubordinateCredentialMessage message);
        protected abstract Task HandleVerifyCredentialChainAsync(VerifyCredentialChainMessage message);
    }

    public abstract class OIDCClientActorBase : ActorBase
    {
        protected OIDCClientActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
        }

        public override async Task ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case InitiateAuthenticationMessage initiateMsg:
                    await HandleInitiateAuthenticationAsync(initiateMsg);
                    break;
                case ExchangeAuthorizationCodeMessage exchangeMsg:
                    await HandleExchangeAuthorizationCodeAsync(exchangeMsg);
                    break;
                case ValidateTokenMessage validateMsg:
                    await HandleValidateTokenAsync(validateMsg);
                    break;
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        protected abstract Task HandleInitiateAuthenticationAsync(InitiateAuthenticationMessage message);
        protected abstract Task HandleExchangeAuthorizationCodeAsync(ExchangeAuthorizationCodeMessage message);
        protected abstract Task HandleValidateTokenAsync(ValidateTokenMessage message);
    }

    public abstract class TokenIssuerActorBase : ActorBase
    {
        protected TokenIssuerActorBase(string id, IActorStateManager stateManager, IMediator mediator)
            : base(id)
        {
        }

        public override async Task ReceiveAsync(IActorMessage message)
        {
            switch (message)
            {
                case IssueTokenMessage issueMsg:
                    await HandleIssueTokenAsync(issueMsg);
                    break;
                case ValidateTokenMessage validateMsg:
                    await HandleValidateTokenAsync(validateMsg);
                    break;
                default:
                    throw new NotSupportedException($"Message type {message.GetType().Name} is not supported.");
            }
        }

        protected abstract Task HandleIssueTokenAsync(IssueTokenMessage message);
        protected abstract Task HandleValidateTokenAsync(ValidateTokenMessage message);
    }


}