using MediatR;
using MediatR.Remote;

namespace Messages;

public class HelloRemoteStreamRequest : IStreamRequest<HelloRemoteStreamResponse>, IRemoteStreamRequest
{
    public HelloRemoteStreamRequest(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public IEnumerable<string> SpanRoles => new[] { "internal-api1", "internal-api2" };
}

public class HelloRemoteStreamResponse
{
    public HelloRemoteStreamResponse(string message)
    {
        Message = message;
    }

    public string Message { get; }
}