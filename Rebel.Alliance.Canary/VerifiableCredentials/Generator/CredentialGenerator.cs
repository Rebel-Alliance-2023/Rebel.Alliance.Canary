using Rebel.Alliance.Canary.Models.Rebel.Alliance.Canary.Models;
using Rebel.Alliance.Canary.VerifiableCredentials;
using System;
using System.Collections.Generic;

namespace Rebel.Alliance.Canary.VerifiableCredentials.Generator
{
    public static class CredentialGenerator
    {
        public static VerifiableCredential GenerateUserCredential(string userId, Dictionary<string, string> claims)
        {
            return new VerifiableCredential
            {
                Id = Guid.NewGuid().ToString(),
                Issuer = "Issuer",
                Subject = userId,
                IssuanceDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                Claims = claims,
                Proof = new Proof() // Assuming Proof is a class with a default constructor
            };
        }

        public static VerifiableCredential GenerateExpiredCredential(string userId, Dictionary<string, string> claims)
        {
            return new VerifiableCredential
            {
                Id = Guid.NewGuid().ToString(),
                Issuer = "Issuer",
                Subject = userId,
                IssuanceDate = DateTime.UtcNow.AddYears(-2),
                ExpirationDate = DateTime.UtcNow.AddYears(-1), // Set to a past date to mark as expired
                Claims = claims,
                Proof = new Proof() // Assuming Proof is a class with a default constructor
            };
        }

        // Other methods as needed
    }
}