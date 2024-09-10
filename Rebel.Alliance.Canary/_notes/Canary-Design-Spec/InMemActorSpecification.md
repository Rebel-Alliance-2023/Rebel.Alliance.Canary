# **Canary Actor System In-Memory Implementation Specification**

## **Overview**

This document outlines the prioritized list of in-memory actor implementations for the Canary system. The in-memory actors are designed to extend the abstract actor classes and handle various tasks related to managing and verifying credentials, issuing tokens, and maintaining a trust framework. The MediatR plugin will be used to facilitate messaging and communication among these in-memory actors.

## **Priority List of In-Memory Actor Implementations**

### **1. InMemoryVerifiableCredentialActor**
- **Purpose:** Handles the creation, signing, and management of Verifiable Credentials.
- **Extends:** `VerifiableCredentialActor`
- **MediatR Integration:** Uses MediatR to publish events related to credential creation, signing, and validation.
- **Supporting Classes/Methods:**
  - Methods for `CreateCredentialAsync`, `SignCredentialAsync`, etc.
  - Supporting data structures for managing credential details in-memory.

### **2. InMemoryCredentialIssuerActor**
- **Purpose:** Issues credentials after verifying the issuer is trusted.
- **Extends:** `CredentialIssuerActor`
- **MediatR Integration:** Uses MediatR to request trust verification from the `InMemoryTrustFrameworkManagerActor` and publish credential issuance events.
- **Supporting Classes/Methods:**
  - Implement methods like `IssueCredentialAsync`, `ValidateIssuerAsync`.
  - Logic for managing issuer keys and issuer states.

### **3. InMemoryCredentialVerifierActor**
- **Purpose:** Verifies credentials by checking their signatures and validation status.
- **Extends:** `CredentialVerifierActor`
- **MediatR Integration:** Uses MediatR to communicate with the `InMemoryRevocationManagerActor` and `InMemoryTrustFrameworkManagerActor` for validation checks.
- **Supporting Classes/Methods:**
  - Implement methods like `VerifyCredentialAsync`, `CheckSignatureAsync`.
  - Logic to handle revocation status check.

### **4. InMemoryCredentialHolderActor**
- **Purpose:** Holds and presents credentials for a particular user or entity.
- **Extends:** `CredentialHolderActor`
- **MediatR Integration:** Uses MediatR to send and receive credential storage and presentation requests.
- **Supporting Classes/Methods:**
  - Implement methods like `StoreCredentialAsync`, `PresentCredentialAsync`, `RenewCredentialAsync`.
  - In-memory storage management for credentials.

### **5. InMemoryRevocationManagerActor**
- **Purpose:** Manages the revocation of credentials.
- **Extends:** `RevocationManagerActor`
- **MediatR Integration:** Uses MediatR to publish revocation notifications and to update the status of credentials across the system.
- **Supporting Classes/Methods:**
  - Implement methods like `RevokeCredentialAsync`, `UpdateRegistryAsync`, `NotifyRevocationAsync`.
  - In-memory registry to track revocation status.

### **6. InMemoryTrustFrameworkManagerActor**
- **Purpose:** Manages the trust framework for issuers, including registering and certifying issuers.
- **Extends:** `TrustFrameworkManagerActor`
- **MediatR Integration:** Uses MediatR to publish trust-related events such as registration, certification, and revocation.
- **Supporting Classes/Methods:**
  - Implement methods like `RegisterIssuerAsync`, `CertifyIssuerAsync`, `RevokeIssuerAsync`, `IsTrustedIssuerAsync`.
  - Manage a list of trusted issuers in-memory.

### **7. InMemoryVerifiableCredentialAsRootOfTrustActor**
- **Purpose:** Handles operations related to Verifiable Credentials that are used as the root of trust.
- **Extends:** `VerifiableCredentialAsRootOfTrustActor`
- **MediatR Integration:** Uses MediatR to coordinate with other actors for credential operations, validation, and updates.
- **Supporting Classes/Methods:**
  - Implement methods related to managing a root credential's hierarchy, signing operations, etc.
  - In-memory state management for root credentials.

### **8. InMemoryOIDCClientActor**
- **Purpose:** Manages OIDC client-specific tasks, such as initiating authentication flows and handling token exchanges.
- **Extends:** `OIDCClientActor`
- **MediatR Integration:** Uses MediatR to handle communication with the `TokenIssuerActor` and the `DecentralizedOIDCProviderService`.
- **Supporting Classes/Methods:**
  - Implement methods related to `AuthenticateAsync`, `ExchangeAuthorizationCodeAsync`, etc.
  - Manage OIDC client-specific information in-memory.

### **9. InMemoryTokenIssuerActor**
- **Purpose:** Issues and validates JWT tokens and manages the cryptographic operations related to OIDC.
- **Extends:** `TokenIssuerActor`
- **MediatR Integration:** Uses MediatR to handle token issuance and validation requests from the `OIDCClientActor` and `DecentralizedOIDCProviderService`.
- **Supporting Classes/Methods:**
  - Implement methods like `IssueTokenAsync`, `ValidateTokenAsync`.
  - In-memory management of keys, issued tokens, etc.

## **Supporting Classes**

### **1. InMemoryActorStateManager**
- **Purpose:** Provides in-memory state management for all the actor types.
- **Implements:** `IActorStateManager`
- **Details:** Methods for storing, retrieving, and clearing state for different actors.

### **2. InMemorySystemProvider**
- **Purpose:** Provides a central registry and management functionality for in-memory actors.
- **Implements:** `IActorSystemProvider`
- **Details:** Logic to create, manage, and interact with in-memory actor instances.

### **3. InMemoryActorSystem**
- **Purpose:** Provides an in-memory implementation of the actor system.
- **Implements:** `IActorSystem`
- **Details:** Core logic to route messages, activate/deactivate actors, manage state, etc.

### **4. InMemoryMessageRouter**
- **Purpose:** Routes messages between actors within the in-memory system.
- **Details:** Ensures messages are delivered to the correct actor instance and method.

### **5. InMemoryActorProxyFactory**
- **Purpose:** Creates in-memory proxies for actors to simulate message-passing and inter-actor communication.
- **Implements:** `IActorProxyFactory`
- **Details:** Logic to create and manage proxies for the in-memory actors.

### **6. InMemoryCredentialRegistry**
- **Purpose:** Provides in-memory storage for issued credentials, revoked credentials, trusted issuers, etc.
- **Details:** Supports the functionality required by actors like `InMemoryRevocationManagerActor`, `InMemoryTrustFrameworkManagerActor`, etc.

## **Next Steps**

1. **Implement InMemoryActorStateManager and InMemorySystemProvider:**
   - Start by finalizing these two core components, as they provide the foundational state management and system management needed for the actors.

2. **Create InMemoryVerifiableCredentialActor and InMemoryCredentialIssuerActor:**
   - Implement these two actors next, as they are essential for the credential issuance and verification flow.

3. **Continue with Other Actors:**
   - Proceed to implement `InMemoryCredentialVerifierActor`, `InMemoryCredentialHolderActor`, and other actors in the order listed above.

## **Conclusion**

The in-memory implementation of the actor framework, integrated with MediatR for messaging, provides a robust and flexible architecture for handling the various credential management, verification, and issuance tasks required by the Canary system. By following this specification, we ensure that all components are correctly prioritized and implemented.
"""