namespace Rebel.Alliance.Canary.Models
{
    public class JwtHeader
    {
        public string Alg { get; set; } = "RS256"; // Default to RS256
        public string Typ { get; set; } = "JWT"; // Token type JWT
        public string Kid { get; set; } // Key ID

        public override string ToString()
        {
            return $"{{\"alg\":\"{Alg}\",\"typ\":\"{Typ}\",\"kid\":\"{Kid}\"}}";
        }
    }
}
