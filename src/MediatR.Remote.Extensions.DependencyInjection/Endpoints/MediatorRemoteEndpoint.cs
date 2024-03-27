namespace MediatR.Remote.Extensions.DependencyInjection.Endpoints;

/// <summary>
///     Invokes the Mediator to handle the <see cref="RemoteMediatorCommand" /> and
///     <see cref="RemoteMediatorStreamCommand" />
/// </summary>
public class MediatorRemoteEndpoint(
    IMediator mediator,
    ILogger<MediatorRemoteEndpoint> logger)
{
    /// <summary>
    ///     Invokes the Mediator to handle the <see cref="IRemoteCommand" />
    /// </summary>
    public Task<RemoteMediatorResult> InvokeAsync(RemoteMediatorCommand command, CancellationToken cancellationToken)
    {
        using var disposable = logger.BeginScope(nameof(InvokeAsync));
        logger.LogReceivedMessage(command.Object?.GetType()!, command.Spans!);

        return InvokeInternalAsync(command, cancellationToken);
    }

    private async Task<RemoteMediatorResult> InvokeInternalAsync(RemoteMediatorCommand command,
        CancellationToken cancellationToken)
    {
        switch (command.Object)
        {
            case IRemoteRequest:
                var result = await mediator.Send(command, cancellationToken);
                return result;

            case IRemoteNotification:
                await mediator.Publish(command, cancellationToken);
                return new RemoteMediatorResult(null);

            default:
                throw new InvalidOperationException(
                    $"{nameof(IRemoteMediator)} is only supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)} and {nameof(IRemoteStreamRequest)}");
        }
    }
}

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Received RemoteCommand: type:{type}, spans:{spans}")]
    internal static partial void LogReceivedMessage(this ILogger logger, Type type, IEnumerable<string> spans);
}
