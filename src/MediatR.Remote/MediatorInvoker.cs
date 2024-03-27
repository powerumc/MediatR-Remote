using System.Runtime.CompilerServices;

namespace MediatR.Remote;

/// <summary>
///     Invokes the Mediator to handle the <see cref="RemoteMediatorCommand" /> and
///     <see cref="RemoteMediatorStreamCommand" />
/// </summary>
internal class MediatorInvoker(IMediator mediator) : IMediatorInvoker
{
    /// <summary>
    ///     Invokes the Mediator to handle the <see cref="RemoteMediatorCommand" />
    /// </summary>
    /// <param name="command">Command object</param>
    /// <param name="cancellationToken">CancellationToken object</param>
    /// <returns>Invoked command result</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="command" /> is null</exception>
    /// <exception cref="NullReferenceException">If <paramref name="command" /> property value is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///     If <paramref name="command" /> is not <see cref="IRemoteRequest" /> or
    ///     <see cref="IRemoteNotification" />
    /// </exception>
    public async Task<RemoteMediatorResult?> InvokeAsync(RemoteMediatorCommand command,
        CancellationToken cancellationToken = default)
    {
        _ = command ?? throw new ArgumentNullException(nameof(command));
        _ = command.Object ?? throw new ArgumentException(nameof(command.Object));

        switch (command.Object)
        {
            case IRemoteRequest:
                var result = await mediator.Send(command.Object, cancellationToken);
                var commandResult = new RemoteMediatorResult(result);
                return commandResult;

            case IRemoteNotification:
                await mediator.Publish(command.Object, cancellationToken);
                return null;

            default:
                throw new InvalidOperationException(
                    $"MediatorRemote is supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)}");
        }
    }

    /// <summary>
    ///     Invokes the Mediator to handle the <see cref="RemoteMediatorStreamCommand" />
    /// </summary>
    /// <param name="command">Stream object</param>
    /// <param name="cancellationToken">CancellationToken object</param>
    /// <returns>Invoked stream result</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="command" /> is null</exception>
    /// <exception cref="NullReferenceException">If <paramref name="command" /> property value is null</exception>
    public async IAsyncEnumerable<RemoteMediatorStreamResult?> InvokeStreamAsync(RemoteMediatorStreamCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = command ?? throw new ArgumentNullException(nameof(command));
        _ = command.Object ?? throw new ArgumentException(nameof(command.Object));

        var stream = mediator.CreateStream(command.Object, cancellationToken);

        await foreach (var item in stream)
        {
            yield return new RemoteMediatorStreamResult(item);
        }
    }
}
