namespace Rebel.Alliance.Canary.OIDC.Models
{
    public class TokenPayload
    {
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
        public DateTime IssuedAt { get; set; }
        public DateTime Expiration { get; set; }

        public override string ToString()
        {
            // Convert the payload to a JSON string (using a simple approach, adjust for security)
            var claimsString = string.Join(",", Claims.Select(kv => $"\"{kv.Key}\":\"{kv.Value}\""));
            return $"{{\"iss\":\"{Issuer}\",\"sub\":\"{Subject}\",\"iat\":{new DateTimeOffset(IssuedAt).ToUnixTimeSeconds()},\"exp\":{new DateTimeOffset(Expiration).ToUnixTimeSeconds()},{claimsString}}}";
        }
    }
}
