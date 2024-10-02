namespace Rebel.Alliance.Canary.Verfiable.Credentials
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using NBitcoin;
    using NBitcoin.Crypto;


    public interface IHdCryptoService
    {
        // Key Management Methods
        Task<(string MasterKey, string MasterPubKey)> CreateMasterKeyAsync();
        Task<string> DeriveKeyAsync(string masterKeyId, string derivationPath);
        Task<string> GetMasterKeyAsync(string keyId);
        Task<string> GetDerivedKeyAsync(string keyId);

        // Cryptographic Operations
        Task<byte[]> SignDataAsync(string privateKeyIdentifier, string data);
        Task<bool> VerifyDataAsync(string publicKeyHex, string data, byte[] signature);
    }

    public class HdCryptoService : IHdCryptoService
    {
        private readonly IKeyStore _keyStore;

        public HdCryptoService(IKeyStore keyStore)
        {
            _keyStore = keyStore;
        }

        // Key Management Methods

        /// <summary>
        /// Creates a new master HD key and stores both the private (WIF format) and public keys (xpub format).
        /// </summary>
        public async Task<(string MasterKey, string MasterPubKey)> CreateMasterKeyAsync()
        {
            return await Task.Run(async () =>
            {
                var masterKey = new ExtKey(); // Generate a new master key
                var masterPubKey = masterKey.Neuter(); // Get the public key

                var masterKeyId = Guid.NewGuid().ToString();

                // Store the private key in the key store
                await _keyStore.StoreKeyAsync(masterKeyId, masterKey.PrivateKey.ToBytes());

                return (masterKey.ToString(Network.Main), masterPubKey.ToString(Network.Main));
            });
        }

        /// <summary>
        /// Generates a derived HD key from the specified master key ID and derivation path.
        /// </summary>
        public async Task<string> DeriveKeyAsync(string masterKeyId, string derivationPath)
        {
            return await Task.Run(async () =>
            {
                // Retrieve the master private key from the key store
                var masterPrivateKeyBytes = await _keyStore.RetrievePrivateKeyAsync(masterKeyId);
                var masterKey = ExtKey.Parse(Encoding.UTF8.GetString(masterPrivateKeyBytes), Network.Main);

                var derivedKey = masterKey.Derive(new KeyPath(derivationPath)); // Derive key using the path
                var derivedKeyId = Guid.NewGuid().ToString();

                // Store the derived private key in the key store
                await _keyStore.StoreKeyAsync(derivedKeyId, derivedKey.PrivateKey.ToBytes());

                return derivedKeyId;
            });
        }

        /// <summary>
        /// Retrieves the master key using the specified key ID.
        /// </summary>
        public async Task<string> GetMasterKeyAsync(string keyId)
        {
            var privateKeyBytes = await _keyStore.RetrievePrivateKeyAsync(keyId);
            var key = ExtKey.Parse(Encoding.UTF8.GetString(privateKeyBytes), Network.Main);
            return key.ToString(Network.Main);
        }

        /// <summary>
        /// Retrieves the derived key using the specified key ID.
        /// </summary>
        public async Task<string> GetDerivedKeyAsync(string keyId)
        {
            var privateKeyBytes = await _keyStore.RetrievePrivateKeyAsync(keyId);
            var key = ExtKey.Parse(Encoding.UTF8.GetString(privateKeyBytes), Network.Main);
            return key.ToString(Network.Main);
        }

        // Cryptographic Operations

        /// <summary>
        /// Signs data using the specified private key identifier.
        /// </summary>
        public async Task<byte[]> SignDataAsync(string privateKeyIdentifier, string data)
        {
            var privateKeyBytes = await _keyStore.RetrievePrivateKeyAsync(privateKeyIdentifier);
            var key = new Key(privateKeyBytes);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return key.Sign(new uint256(Hashes.SHA256(dataBytes))).ToDER();
        }

        /// <summary>
        /// Verifies the signature using the specified public key (hex format).
        /// </summary>
        public async Task<bool> VerifyDataAsync(string publicKeyHex, string data, byte[] signature)
        {
            return await Task.Run(() =>
            {
                var pubKey = new PubKey(publicKeyHex);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hash = new uint256(Hashes.SHA256(dataBytes));
                return pubKey.Verify(hash, new ECDSASignature(signature));
            });
        }
    }
}