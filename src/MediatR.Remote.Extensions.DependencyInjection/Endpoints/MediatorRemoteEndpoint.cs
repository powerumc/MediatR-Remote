namespace MediatR.Remote.Extensions.DependencyInjection.Endpoints;

/// <summary>
///     Invokes the Mediator to handle the <see cref="RemoteMediatorCommand" /> and
///     <see cref="RemoteMediatorStreamCommand" />
/// </summary>
public class MediatorRemoteEndpoint
{
    private readonly ILogger<MediatorRemoteEndpoint> _logger;
    private readonly IMediator _mediator;

    public MediatorRemoteEndpoint(
        IMediator mediator,
        ILogger<MediatorRemoteEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Invokes the Mediator to handle the <see cref="IRemoteCommand" />
    /// </summary>
    public Task<RemoteMediatorResult> InvokeAsync(RemoteMediatorCommand command)
    {
        using var disposable = _logger.BeginScope(nameof(InvokeAsync));
        _logger.LogReceivedMessage(command.Object?.GetType()!, command.Spans!);

        return InvokeInternalAsync(_mediator, command);
    }

    private async Task<RemoteMediatorResult> InvokeInternalAsync(IMediator mediator,
        RemoteMediatorCommand command)
    {
        switch (command.Object)
        {
            case IRemoteRequest:
                var result = await mediator.Send(command);
                return result;

            case IRemoteNotification:
                await mediator.Publish(command);
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
