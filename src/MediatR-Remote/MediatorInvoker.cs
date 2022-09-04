using System.Runtime.CompilerServices;

namespace MediatR.Remote;

internal class MediatorInvoker : IMediatorInvoker
{
    private readonly IMediator _mediator;

    public MediatorInvoker(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<RemoteMediatorResult?> InvokeAsync(RemoteMediatorCommand command,
        CancellationToken cancellationToken = default)
    {
        _ = command ?? throw new ArgumentNullException(nameof(command));
        _ = command.Object ?? throw new NullReferenceException(nameof(command.Object));

        switch (command.Object)
        {
            case IRemoteRequest:
                var result = await _mediator.Send(command.Object, cancellationToken);
                var commandResult = new RemoteMediatorResult(result);
                return commandResult;

            case IRemoteNotification:
                await _mediator.Publish(command.Object, cancellationToken);
                return null;

            default:
                throw new InvalidOperationException(
                    $"MediatorRemote is supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)}");
        }
    }

    public async IAsyncEnumerable<RemoteMediatorStreamResult?> InvokeStreamAsync(RemoteMediatorStreamCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = command ?? throw new ArgumentNullException(nameof(command));
        _ = command.Object ?? throw new NullReferenceException(nameof(command.Object));

        var stream = _mediator.CreateStream(command.Object, cancellationToken).WithCancellation(cancellationToken);

        await foreach (var item in stream)
        {
            yield return new RemoteMediatorStreamResult(item);
        }
    }
}