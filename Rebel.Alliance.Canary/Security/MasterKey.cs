namespace Rebel.Alliance.Canary.Security
{
    public class MasterKey
    {
        public string Id { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public List<DerivedKey> DerivedKeys { get; set; } = new();
    }
}
