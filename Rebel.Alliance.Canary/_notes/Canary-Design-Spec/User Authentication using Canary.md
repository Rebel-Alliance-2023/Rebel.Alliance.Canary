### Scenario: User Authentication using Canary 

To walk through a scenario where the **BlazorCanary Web App** authenticates a user using the **Canary Architecture** with the new .NET 8 OIDC interface, we will outline the steps from the moment the user tries to log in until the authentication is completed. This scenario will leverage the **Verifiable Credentials (VC)** and the **OIDC actors** that we have implemented in the architecture. The main goal is to demonstrate how the architecture components interact with each other to authenticate the user dynamically using the decentralized approach.

### Scenario: User Authentication in BlazorCanary Web App

#### **Step-by-Step Workflow:**

1. **User Initiates Authentication:**
   - The user opens the **BlazorCanary Web App** and clicks the "Login" button.
   - The web app uses the .NET 8 OIDC authentication interface to initiate the login process.
   - The OIDC middleware triggers an authentication request to the **DecentralizedOIDCProviderService**.

2. **DecentralizedOIDCProviderService Handles Authentication Request:**
   - The **DecentralizedOIDCProviderService** receives the authentication request and generates an **`InitiateAuthenticationMessage`**.
   - This message includes the `redirectUri` and other client-specific information required to initiate the OIDC workflow.
   - The service sends the `InitiateAuthenticationMessage` to the **OIDCClientActor** using the **`SendMessageAsync`** method.

3. **OIDCClientActor Processes Authentication:**
   - The **OIDCClientActor** receives the `InitiateAuthenticationMessage` and checks if it has a stored verifiable credential (VC) for the client.
   - If the VC exists, it verifies the integrity of the VC using the **`VerifiableCredentialActor`** to ensure it has not been tampered with.
   - If the VC is valid, the **OIDCClientActor** generates an **`AuthorizationCode`** and associates it with the client credential.
   - The **OIDCClientActor** sends a response back to the **DecentralizedOIDCProviderService** with the generated `AuthorizationCode`.

4. **DecentralizedOIDCProviderService Returns Authorization Code:**
   - The **DecentralizedOIDCProviderService** receives the response from the **OIDCClientActor** and provides the `AuthorizationCode` back to the **BlazorCanary Web App** via the OIDC middleware.

5. **User is Redirected to Consent Page:**
   - The user is redirected to a consent page where they review and approve the scope of access requested by the application.
   - Upon user consent, the **BlazorCanary Web App** sends a request to the **DecentralizedOIDCProviderService** to exchange the `AuthorizationCode` for tokens.

6. **DecentralizedOIDCProviderService Exchanges Authorization Code:**
   - The **DecentralizedOIDCProviderService** receives the request to exchange the `AuthorizationCode` for an access token and ID token.
   - It creates a **`TokenRequestMessage`** containing the `AuthorizationCode`, `redirectUri`, and the client’s credential.
   - This message is sent to the **TokenIssuerActor**.

7. **TokenIssuerActor Issues Tokens:**
   - The **TokenIssuerActor** receives the **`TokenRequestMessage`** and validates the `AuthorizationCode`.
   - It verifies the associated VC using **VerifiableCredentialActor**.
   - Upon successful validation, it generates a JWT **access token** and **ID token**. These tokens include claims, issuer, subject, issued-at time, expiration, and cryptographic signatures.
   - The **TokenIssuerActor** responds with a **`TokenResponse`** containing the tokens.

8. **DecentralizedOIDCProviderService Returns Tokens:**
   - The **DecentralizedOIDCProviderService** receives the **`TokenResponse`** from the **TokenIssuerActor**.
   - It returns the tokens to the **BlazorCanary Web App** via the OIDC middleware.

9. **User is Authenticated:**
   - The **BlazorCanary Web App** receives the tokens and stores them in the browser (e.g., in cookies or local storage).
   - The app uses the tokens to authenticate the user for subsequent requests, such as accessing protected resources.

10. **CredentialVerifierActor Validates Tokens on Resource Access:**
    - When the user attempts to access a protected resource, the web app includes the access token in the request header.
    - The API receiving the request uses **DecentralizedOIDCProviderService** to validate the token.
    - The **DecentralizedOIDCProviderService** sends a **`ValidateTokenMessage`** to the **CredentialVerifierActor**.
    - The **CredentialVerifierActor** verifies the token's signature, checks the revocation status using **RevocationManagerActor**, and confirms the token’s validity.

11. **Access Granted or Denied:**
    - If the token is valid, the API grants access to the requested resource.
    - If the token is invalid, expired, or revoked, the API denies access and may return an appropriate error message.

#### **Summary of Component Interactions:**

- **BlazorCanary Web App** uses the .NET 8 OIDC interface to communicate with the **DecentralizedOIDCProviderService**.
- **DecentralizedOIDCProviderService** acts as the bridge between the web app and the decentralized actor-based architecture.
- **OIDCClientActor** manages client-related credentials and the initial authentication workflow.
- **TokenIssuerActor** issues JWT tokens after validating the client’s credentials and authorization code.
- **VerifiableCredentialActor** ensures the integrity and validity of VCs.
- **CredentialVerifierActor** validates tokens upon subsequent resource access requests.
- **RevocationManagerActor** manages the status of revoked credentials.
  
### Key Points:

- The use of actors allows for decentralized, flexible credential management.
- Dynamic VC-based workflows enhance security and compliance.
- The architecture leverages the .NET 8 OIDC interface for seamless integration with modern web applications like BlazorCanary.

This step-by-step walkthrough ensures the main components of the Canary Architecture interact correctly to authenticate users dynamically using verifiable credentials in a decentralized manner.