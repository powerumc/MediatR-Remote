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
    public async Task<RemoteMediatorResult> InvokeAsync(RemoteMediatorCommand command,
        CancellationToken cancellationToken)
    {
        using var disposable = logger.BeginScope(nameof(InvokeAsync));
        logger.LogReceivedMessage(command.Object?.GetType()!, command.Spans!);

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
                    $"{nameof(InvokeAsync)} is only supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)}");
        }
    }

    public IAsyncEnumerable<RemoteMediatorStreamResult> InvokeStreamAsync(RemoteMediatorStreamCommand command,
        CancellationToken cancellationToken)
    {
        using var disposable = logger.BeginScope(nameof(InvokeStreamAsync));
        logger.LogReceivedMessage(command.Object?.GetType()!, command.Spans!);

        switch (command.Object)
        {
            case IRemoteStreamRequest:
                var result = mediator.CreateStream(command, cancellationToken);
                return result;

            default:
                throw new InvalidOperationException(
                    $"{nameof(InvokeStreamAsync)} is only supports {nameof(IRemoteStreamRequest)}");
        }
    }
}

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Received RemoteCommand: type:{type}, spans:{spans}")]
    internal static partial void LogReceivedMessage(this ILogger logger, Type type, IEnumerable<string> spans);
}
