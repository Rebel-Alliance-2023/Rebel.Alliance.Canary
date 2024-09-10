namespace Rebel.Alliance.Canary.Abstractions
{
    public interface IKeyStore
    {
        Task<string> StoreKeyAsync(string identifier, byte[] privateKey);
        Task<byte[]> RetrievePrivateKeyAsync(string identifier);
        Task<byte[]> RetrievePublicKeyAsync(string identifier);
        Task<byte[]> SignDataAsync(string identifier, string data);
        //byte[] SignData(string identifier, string data);
    }
}
