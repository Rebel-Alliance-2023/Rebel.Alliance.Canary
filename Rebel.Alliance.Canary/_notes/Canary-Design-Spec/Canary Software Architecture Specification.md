
# Canary Software Architecture Specification

## Overview

The Canary software architecture aims to provide a decentralized, secure, and flexible framework for authentication and authorization using Verifiable Credentials (VCs). This architecture integrates with the .NET 8 OpenID Connect (OIDC) interface and leverages an actor-based model to manage dynamic credential validation, token issuance, and secure communications.

## Components

### 1. BlazorCanary Web Application
- **Description:** A Blazor WASM Hosted Web Application that serves as the user interface and entry point for authentication.
- **Purpose:** Allows users to authenticate via OIDC, interact with the VC system, and access protected resources.

### 2. DecentralizedOIDCProviderService
- **Description:** Acts as a bridge between the .NET 8 OIDC interface and the actor-based VC management.
- **Purpose:** Manages the OIDC authentication workflow, including initiating authentication, exchanging authorization codes, and validating tokens.
- **Key Responsibilities:**
  - Handles authentication requests from the BlazorCanary Web App.
  - Interacts with OIDCClientActor for dynamic VC-based client management.
  - Relays token requests to TokenIssuerActor and processes responses.

### 3. OIDCClientActor
- **Description:** Represents a client in the OIDC workflow.
- **Purpose:** Manages the client’s VC, initiates authentication processes, and handles authorization codes.
- **Key Responsibilities:**
  - Validates and stores client credentials.
  - Generates authorization codes and returns them to the OIDC Provider Service.
  - Issues token requests to TokenIssuerActor.

### 4. TokenIssuerActor
- **Description:** Responsible for issuing JWT tokens after validating client credentials and authorization codes.
- **Purpose:** Provides a secure mechanism for creating and signing tokens.
- **Key Responsibilities:**
  - Validates incoming authorization codes and VCs.
  - Issues JWT access and ID tokens with cryptographic signatures.
  - Interacts with CryptoService for signing operations.

### 5. VerifiableCredentialActor
- **Description:** Manages the lifecycle of Verifiable Credentials.
- **Purpose:** Ensures the integrity, validity, and secure management of VCs.
- **Key Responsibilities:**
  - Signs and verifies VCs using cryptographic methods.
  - Communicates with CryptoService for secure signing and verification.
  - Supports dynamic VC creation and modification.

### 6. CredentialVerifierActor
- **Description:** Validates tokens and checks credentials.
- **Purpose:** Ensures that tokens and VCs are valid and have not been tampered with.
- **Key Responsibilities:**
  - Validates JWT tokens for authenticity and integrity.
  - Communicates with RevocationManagerActor to check revocation status.
  - Interacts with VerifiableCredentialActor to verify VCs.

### 7. RevocationManagerActor
- **Description:** Manages the revocation status of credentials.
- **Purpose:** Keeps track of revoked credentials and ensures that expired or invalid credentials are not used.
- **Key Responsibilities:**
  - Updates and maintains a registry of revoked credentials.
  - Provides methods to check if a credential is revoked.
  - Notifies other components about revocation events.

### 8. CryptoService
- **Description:** Provides cryptographic operations required for secure VC and token management.
- **Purpose:** Handles key management, encryption, signing, and verification operations.
- **Key Responsibilities:**
  - Generates and stores cryptographic keys.
  - Signs data securely using private keys.
  - Verifies data signatures using public keys.

### 9. InMemoryActorMessageBus
- **Description:** Provides a message-passing mechanism for actors within the in-memory implementation.
- **Purpose:** Facilitates communication between actors to enable secure, decentralized processing.
- **Key Responsibilities:**
  - Implements `SendMessageAsync` methods to allow inter-actor communication.
  - Ensures messages are routed to the correct actor and method.

## Workflow

### 1. User Authentication Flow

1. **User Initiates Authentication:**
   - User clicks the "Login" button in the BlazorCanary Web App.
   - OIDC middleware triggers a request to `DecentralizedOIDCProviderService`.

2. **OIDC Workflow:**
   - `DecentralizedOIDCProviderService` sends an `InitiateAuthenticationMessage` to `OIDCClientActor`.
   - `OIDCClientActor` validates client credentials and generates an `AuthorizationCode`.
   - The authorization code is returned to the user via the OIDC middleware.

3. **Authorization Code Exchange:**
   - User consents and sends a request to exchange the authorization code for tokens.
   - `DecentralizedOIDCProviderService` sends a `TokenRequestMessage` to `TokenIssuerActor`.
   - `TokenIssuerActor` issues JWT tokens and sends a `TokenResponse`.

4. **Token Validation and Resource Access:**
   - Tokens are used for accessing protected resources.
   - `CredentialVerifierActor` validates the tokens and checks revocation status.
   - Access is granted or denied based on token validity.

## Key Concepts

- **Verifiable Credentials (VCs):** A secure, decentralized means of managing identity and credentials, ensuring the authenticity and integrity of claims.
- **OpenID Connect (OIDC):** A modern authentication protocol used for authenticating users and issuing access tokens.
- **Actor Model:** A decentralized model for managing state and processing, allowing for scalability, flexibility, and dynamic credential management.
- **Mediatr Messaging:** Used for local, in-memory actor communication in development environments. Abstracted for other actor frameworks.

## Future Enhancements

- **Support for Multiple Actor Frameworks:** Integration with Dapr, Orleans, Akka, etc.
- **Enhanced Security Measures:** Incorporate more advanced cryptographic operations and key management strategies.
- **Dynamic VC Issuance and Management:** Enable more flexible, on-demand credential issuance for different scenarios.
- **Improved Auditing and Logging:** Enhanced monitoring for all authentication and token management operations.

## Conclusion

The Canary software architecture provides a robust, decentralized solution for modern authentication and authorization workflows using Verifiable Credentials. The integration with .NET 8's OIDC interface ensures compatibility with existing web standards, while the actor-based model provides flexibility, scalability, and security for managing credentials dynamically.

