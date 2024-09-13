using System.Threading.Tasks;

namespace Rebel.Alliance.Canary.Security
{


    public class MasterKey
    {
        public string Id { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public List<DerivedKey> DerivedKeys { get; set; } = new();
    }

    public class DerivedKey
    {
        public string Id { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public string MasterKeyId { get; set; }
    }
}
