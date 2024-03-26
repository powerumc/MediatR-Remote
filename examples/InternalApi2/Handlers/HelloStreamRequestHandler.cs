using System.Runtime.CompilerServices;
using MediatR;
using Messages;

namespace InternalApi2.Handlers;

public class HelloStreamRequestHandler : IStreamRequestHandler<HelloRemoteStreamRequest, HelloRemoteStreamResponse>
{
    public async IAsyncEnumerable<HelloRemoteStreamResponse> Handle(HelloRemoteStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < 10; i++)
        {
            yield return new HelloRemoteStreamResponse($"OK - {request.Message} {i}");
            await Task.Delay(500, cancellationToken);
        }
    }
}