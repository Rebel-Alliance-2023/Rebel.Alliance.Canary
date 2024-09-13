using Rebel.Alliance.Canary.Abstractions;
public class ValidateTokenMessage : IActorMessage
{
    public string Token { get; }

    public ValidateTokenMessage(string token)
    {
        Token = token;
    }

    public string MessageType => nameof(ValidateTokenMessage);
}

