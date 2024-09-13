
namespace Rebel.Alliance.Canary.Security
{
    public interface ICryptoService1
    {
        Task<string> DecryptDataAsync(byte[] privateKey, byte[] data);
        Task<byte[]> EncryptDataAsync(byte[] publicKey, string data);
        Task<(byte[] PublicKey, byte[] PrivateKey)> GenerateKeyPairAsync();
        Task<string> GenerateMnemonicAsync();
        Task<byte[]> SignDataAsync(byte[] privateKey, string data);
        Task<(byte[] Signature, byte[] PublicKey)> SignDataUsingIdentifierAsync(string identifier, string data);
        Task<bool> VerifyDataAsync(byte[] publicKey, string data, byte[] signature);
    }
}