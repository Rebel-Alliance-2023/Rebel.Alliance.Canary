
# Simplified OIDC Interface in .NET 8 and Integration with Canary Architecture

## Table of Contents
1. [Introduction](#introduction)
2. [Overview of the Simplified OIDC Interface in .NET 8](#overview-of-the-simplified-oidc-interface-in-net-8)
3. [Key Features of the Simplified OIDC Interface](#key-features-of-the-simplified-oidc-interface)
4. [Canary Architecture and Virtual Actor Design](#canary-architecture-and-virtual-actor-design)
5. [Integration of Canary Architecture with Simplified OIDC Interface](#integration-of-canary-architecture-with-simplified-oidc-interface)
6. [Potential Enhancements and Future Directions](#potential-enhancements-and-future-directions)
7. [Conclusion](#conclusion)

## Introduction

This document describes the simplified OpenID Connect (OIDC) interface introduced in .NET 8 and how the Canary Architecture, with its abstracted Virtual Actor design, will integrate into this new interface. The integration aims to provide a secure, decentralized, and flexible identity management system using Verifiable Credentials (VCs) within an ASP.NET Core application.

## Overview of the Simplified OIDC Interface in .NET 8

The simplified OIDC interface in .NET 8 offers a streamlined way to implement OIDC authentication and authorization in ASP.NET Core applications. This interface provides built-in middleware to handle common OIDC workflows, reducing the complexity of integration and enhancing security through automatic token management and event handling.

## Key Features of the Simplified OIDC Interface

### 1. Simplified Configuration
- Configures OIDC providers with minimal code by specifying essential parameters like `Authority`, `ClientId`, and `ClientSecret` in the `appsettings.json` file.
- Supports dynamic configuration changes, making it adaptable to different environments and trust frameworks.

### 2. Default Middleware Integration
- Integrates directly with ASP.NET Core's middleware pipeline using `AddOpenIdConnect` and `AddJwtBearer` methods.
- Handles the complete OIDC flow, including redirections, token exchanges, and authentication.

### 3. Enhanced Event Handling
- Offers event hooks (`OnTokenValidated`, `OnUserInformationReceived`, etc.) for developers to customize the authentication process.
- Enables custom logic for validating tokens, handling errors, and retrieving user information.

### 4. Support for Modern OIDC Flows
- Supports Authorization Code Flow with Proof Key for Code Exchange (PKCE), enhancing security by requiring additional verification during token exchange.

### 5. Automatic Token Management
- Manages tokens automatically, including caching, renewal, and expiration, using ASP.NET Core's built-in data protection mechanisms.

## Canary Architecture and Virtual Actor Design

The Canary Architecture uses an abstracted Virtual Actor design that decouples the core logic of OIDC Verifiable Credential management from any specific actor framework. This allows the architecture to integrate seamlessly with multiple frameworks, such as Orleans, Akka, and Dapr, while leveraging the benefits of each.

### Key Components of the Canary Architecture

- **OIDCClientActor**: Handles OIDC client operations, such as initiating authentication flows and exchanging tokens.
- **TokenIssuerActor**: Issues and validates JWT tokens based on dynamically retrieved configuration from VCs.
- **DecentralizedOIDCProviderService**: Manages interactions between actors and Verifiable Credentials, orchestrating the entire OIDC workflow.
- **CryptoService**: Provides cryptographic operations, such as signing and verifying tokens.
- **KeyManagementService**: Manages cryptographic keys used by the actors for secure token handling.

## Integration of Canary Architecture with Simplified OIDC Interface

### 1. Dynamic Configuration Using Verifiable Credentials
- **Challenge**: Traditional OIDC configurations rely on static settings defined in `appsettings.json`.
- **Solution**: The Canary Architecture dynamically retrieves OIDC settings (`Authority`, `ClientId`, `ClientSecret`) from Verifiable Credentials. This makes the authentication flows adaptable and context-aware, ensuring they conform to the decentralized trust model.

### 2. Leveraging OIDC Middleware for Actor Interaction
- The `DecentralizedOIDCProviderService` uses the new simplified OIDC middleware to handle redirections, token exchanges, and callbacks.
- The service retrieves VCs and presents them to the appropriate actors (`OIDCClientActor` or `TokenIssuerActor`) to initialize their state dynamically.

### 3. Enhanced Event Handling for VC Validation
- Canary can utilize the middleware's event hooks to inject custom logic for VC validation:
  - `OnTokenValidated`: Validates the token against VCs to ensure the issuer and claims are trusted.
  - `OnUserInformationReceived`: Retrieves additional user information from the VC and validates it against trusted data sources.

### 4. Decentralized Token Issuance and Validation
- The `TokenIssuerActor` dynamically issues and validates tokens using settings extracted from VCs, ensuring that the tokens adhere to the decentralized trust framework.
- The actor integrates with the middleware to leverage built-in mechanisms for automatic token renewal and caching.

### 5. Flexible Actor Framework Integration
- The use of the `IActorSystemProvider` interface allows the Canary Architecture to switch between different actor frameworks (e.g., Orleans, Akka, Dapr) seamlessly.
- Actors like `OIDCClientActor` and `TokenIssuerActor` interact with the OIDC middleware through a unified abstraction layer, enabling flexible deployment scenarios.

## Potential Enhancements and Future Directions

1. **Decentralized Trust Management**:
   - Extend the OIDC middleware to support dynamic discovery of trusted issuers and credentials through decentralized ledgers or identity networks.

2. **Advanced Token Management**:
   - Use custom token issuance strategies in the `TokenIssuerActor` to support multi-factor authentication (MFA) and other advanced security features.

3. **Interoperability Across Frameworks**:
   - Further enhance the `IActorSystemProvider` to support more complex scenarios and actor interactions across different frameworks.

4. **Improved Scalability**:
   - Optimize the architecture to handle high-volume OIDC flows, ensuring scalability and resilience.

## Conclusion

By leveraging the simplified OIDC interface in .NET 8, the Canary Architecture can provide a secure, decentralized, and flexible identity management solution. The integration with Verifiable Credentials allows for dynamic and context-aware authentication flows, while the abstracted Virtual Actor design ensures compatibility with various actor frameworks. This approach positions Canary as a robust, scalable, and adaptable architecture for modern identity management challenges.
