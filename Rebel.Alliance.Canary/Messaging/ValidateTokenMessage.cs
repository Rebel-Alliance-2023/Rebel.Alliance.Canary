using Rebel.Alliance.Canary.Abstractions;
public class ValidateTokenMessage : IActorMessage
{
    public string ClientId { get; }
    public string Token { get; }
    public string MessageType { get; }

    public ValidateTokenMessage(string clientId, string token)
    {
        ClientId = clientId;
        Token = token;
        MessageType = "ValidateToken";
    }
}
