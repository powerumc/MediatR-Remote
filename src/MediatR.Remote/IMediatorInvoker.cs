namespace MediatR.Remote;

/// <summary>
/// Defines a mediator invoker.
/// </summary>
public interface IMediatorInvoker
{
    Task<RemoteMediatorResult?> InvokeAsync(RemoteMediatorCommand command,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<RemoteMediatorStreamResult?> InvokeStreamAsync(RemoteMediatorStreamCommand command,
        CancellationToken cancellationToken = default);
}
