using MediatR;
using Messages;

namespace InternalApi2.Handlers;

public class HelloRequestHandler : IRequestHandler<HelloRemoteRequest, HelloResponse>
{
    public Task<HelloResponse> Handle(HelloRemoteRequest remoteRequest, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HelloResponse($"OK - {remoteRequest.Message}"));
    }
}