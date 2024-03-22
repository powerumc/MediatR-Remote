using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote;

/// <summary>
///     A mediator command handler implementation that can handle remote requests.
/// </summary>
internal class RemoteMediatorCommandHandler : RemoteMediatorCommandHandlerBase,
    IRequestHandler<RemoteMediatorCommand, RemoteMediatorResult?>,
    INotificationHandler<RemoteMediatorCommand>
{
    private readonly ILogger<RemoteMediatorCommandHandler> _logger;
    private readonly IMediatorInvoker _mediatorInvoker;
    private readonly IOptionsMonitor<RemoteMediatorOptions> _remoteMediatorOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RemoteMediatorCommandHandler(
        IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
        IServiceScopeFactory serviceScopeFactory,
        IMediatorInvoker mediatorInvoker,
        ILogger<RemoteMediatorCommandHandler> logger)
    {
        _remoteMediatorOptions = remoteMediatorOptions;
        _serviceScopeFactory = serviceScopeFactory;
        _mediatorInvoker = mediatorInvoker;
        _logger = logger;
    }

    Task INotificationHandler<RemoteMediatorCommand>.Handle(RemoteMediatorCommand notification,
        CancellationToken cancellationToken)
    {
        return HandleInternalAsync(notification, cancellationToken);
    }

    public Task<RemoteMediatorResult?> Handle(RemoteMediatorCommand request, CancellationToken cancellationToken)
    {
        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<RemoteMediatorResult?> HandleInternalAsync(RemoteMediatorCommand request,
        CancellationToken cancellationToken)
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));
        _ = request.Object ?? throw new ArgumentException(nameof(request.Object));

        if (request.Object is not IRemoteCommand remoteCommand)
        {
            return await _mediatorInvoker.InvokeAsync(request, cancellationToken);
        }

        var options = _remoteMediatorOptions.CurrentValue;
        var myRoleNames = options.MyRoleNames;
        var requestSpans = request.Spans;
        var (nextSpans, targetRoleName) = GetNextSpans(remoteCommand, requestSpans, myRoleNames);

        if (targetRoleName is null)
        {
            return await _mediatorInvoker.InvokeAsync(request, cancellationToken);
        }

        using var disposable = _logger.BeginScope(nameof(RemoteMediatorCommandHandler));
        _logger.LogBeginHandler(myRoleNames, targetRoleName, request.Object.GetType().Name);

        if (!options.RemoteStrategies.TryGetValue(targetRoleName, out var remoteStrategyType))
        {
            throw new InvalidOperationException($"'{targetRoleName}' is not contains the remote strategies.");
        }

        var serviceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
        var command = new RemoteMediatorCommand(request.Object, nextSpans);
        var remoteResult = await InvokeRemoteAsync(serviceProvider, myRoleNames, targetRoleName, nextSpans,
            command, remoteStrategyType, cancellationToken);

        return remoteResult;
    }

    private static Task<RemoteMediatorResult?> InvokeRemoteAsync(
        IServiceProvider serviceProvider,
        IEnumerable<string> myRoleNames,
        string targetRoleName,
        IEnumerable<string> nextSpans,
        RemoteMediatorCommand command,
        StrategyTypes strategyTypes,
        CancellationToken cancellationToken)
    {
        var remoteStrategy = command.Object switch
        {
            IRemoteRequest => (IRemoteStrategy)serviceProvider.GetRequiredService(strategyTypes.RequestStrategyType),
            IRemoteNotification =>
                (IRemoteStrategy)serviceProvider.GetRequiredService(strategyTypes.NotificationStrategyType),
            _ => throw new InvalidOperationException(
                $"MediatorRemote is supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)}")
        };

        return remoteStrategy.InvokeAsync(myRoleNames, targetRoleName, nextSpans, command, cancellationToken);
    }
}

public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Trace,
        Message = "Handle: myRoleName:{myRoleNames}, targetRoleName:{targetRoleName}, type:{type}")]
    internal static partial void LogBeginHandler(this ILogger logger, IEnumerable<string> myRoleNames,
        string targetRoleName,
        string type);
}
