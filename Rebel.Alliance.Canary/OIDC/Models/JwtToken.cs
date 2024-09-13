using System.IdentityModel.Tokens.Jwt;

namespace Rebel.Alliance.Canary.OIDC.Models
{
    public class JwtToken
    {
        public JwtHeader Header { get; set; }
        public TokenPayload Payload { get; set; }
        public byte[] Signature { get; set; }

        public override string ToString()
        {
            // Serialize the JWT as a compact string (Base64Url encoded parts)
            var headerBase64 = Base64UrlEncode(System.Text.Json.JsonSerializer.Serialize(Header));
            var payloadBase64 = Base64UrlEncode(Payload.ToString());
            var signatureBase64 = Base64UrlEncode(Signature);

            return $"{headerBase64}.{payloadBase64}.{signatureBase64}";
        }

        public static JwtToken Parse(string token)
        {
            // Split the token into parts
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid JWT token format.");
            }

            var header = System.Text.Json.JsonSerializer.Deserialize<JwtHeader>(Base64UrlDecode(parts[0]));
            var payload = System.Text.Json.JsonSerializer.Deserialize<TokenPayload>(Base64UrlDecode(parts[1]));
            var signature = Convert.FromBase64String(Base64UrlDecode(parts[2]));

            return new JwtToken { Header = header, Payload = payload, Signature = signature };
        }

        private string Base64UrlEncode(string input)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string Base64UrlDecode(string input)
        {
            input = input.Replace('-', '+').Replace('_', '/');
            switch (input.Length % 4)
            {
                case 0: break;
                case 2: input += "=="; break;
                case 3: input += "="; break;
                default: throw new ArgumentException("Invalid base64url string.");
            }
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }
    }
}
