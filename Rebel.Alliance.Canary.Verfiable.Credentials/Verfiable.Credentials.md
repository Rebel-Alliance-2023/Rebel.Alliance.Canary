# VerifiableCredential and Proof Class Descriptions

## VerifiableCredential Class

The `VerifiableCredential` class represents a digital credential that is signed cryptographically to ensure its integrity and authenticity. It supports standard JWT properties and custom claims for flexible identity management.

### Properties

- **Issuer (iss)**: `string`  
  The entity that issued the credential. Mapped to the standard JWT claim "iss".

- **Subject (sub)**: `string`  
  The subject of the credential, representing the entity to whom the credential pertains. Mapped to the standard JWT claim "sub".

- **Audience (aud)**: `string`  
  The intended audience for the credential. Mapped to the standard JWT claim "aud".

- **ExpirationDate (exp)**: `DateTime`  
  The date and time when the credential expires. Mapped to the standard JWT claim "exp".

- **NotBefore (nbf)**: `DateTime`  
  The date and time before which the credential is not valid. Mapped to the standard JWT claim "nbf".

- **IssuanceDate (iat)**: `DateTime`  
  The date and time when the credential was issued. Mapped to the standard JWT claim "iat".

- **Id (jti)**: `string`  
  A unique identifier for the credential. Mapped to the standard JWT claim "jti".

- **Claims**: `Dictionary<string, string>`  
  A collection of custom claims associated with the credential.

- **Proof**: `Proof`  
  The cryptographic proof that ensures the integrity and authenticity of the credential.

- **ParentCredentialId (vc_parent)**: `string?`  
  An optional identifier for the parent credential, if this credential is derived from another.

- **Authority**: `string`  
  [Ignored in JSON] Represents the authority issuing the credential in OIDC scenarios.

- **ClientId**: `string`  
  [Ignored in JSON] Represents the client ID in OIDC scenarios.

- **ClientSecret**: `string`  
  [Ignored in JSON] Represents the client secret in OIDC scenarios.

- **IsExpired**: `bool`  
  Indicates whether the credential is expired based on the `ExpirationDate`.

### Methods

- **IsValid()**: `bool`  
  Checks whether the credential is valid by ensuring required properties are set correctly and the proof is valid.

## Proof Class

The `Proof` class represents the cryptographic proof for the Verifiable Credential. It contains properties related to the signature and methods to sign and verify data.

### Properties

- **Type**: `string`  
  The type of cryptographic signature, default is "Ed25519Signature2018".

- **Created**: `DateTime`  
  The date and time when the proof was created.

- **VerificationMethod**: `string`  
  The method or key used for verification of the proof.

- **ProofPurpose**: `string`  
  The purpose of the proof, default is "assertionMethod".

- **Jws**: `string`  
  The JSON Web Signature (JWS) representing the proof.

- **Creator**: `string`  
  The entity that created the proof.

- **Domain**: `string`  
  The domain or scope of the proof.

### Methods

- **SignAsync(string privateKeyIdentifier, string data)**: `Task<bool>`  
  Asynchronously signs the provided data using the specified private key identifier from the HD key management system.

- **VerifyAsync(string publicKeyIdentifier, string data)**: `Task<bool>`  
  Asynchronously verifies the provided data using the specified public key identifier from the HD key management system.

- **IsValid()**: `bool`  
  Checks whether the proof object is valid by ensuring that all required properties are correctly set.
