using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote;

internal class RemoteMediatorCommandHandler : IRequestHandler<RemoteMediatorCommand, RemoteMediatorResult?>,
    INotificationHandler<RemoteMediatorCommand>
{
    private readonly ILogger<RemoteMediatorCommandHandler> _logger;
    private readonly IMediatorInvoker _mediatorInvoker;
    private readonly IOptionsMonitor<RemoteMediatorOptions> _remoteMediatorOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RemoteMediatorCommandHandler(IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
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

    public async Task<RemoteMediatorResult?> HandleInternalAsync(RemoteMediatorCommand request,
        CancellationToken cancellationToken)
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));
        _ = request.Object ?? throw new NullReferenceException(nameof(request.Object));

        if (request.Object is IRemoteCommand remoteCommand)
        {
            var options = _remoteMediatorOptions.CurrentValue;
            var roles = remoteCommand.SpanRoles ?? Array.Empty<string>();
            var spans = request.Spans ?? Enumerable.Empty<string>();
            var myRoleNames = options.MyRoleNames;
            var nextSpans = spans.Concat(myRoleNames).ToArray();
            var excepted = roles.Except(nextSpans);

            var targetRoleName = excepted.FirstOrDefault();
            if (targetRoleName != null)
            {
                using var _ = _logger.BeginScope(nameof(RemoteMediatorCommandHandler));
                _logger.LogBeginHandler(myRoleNames, targetRoleName, request.Object.GetType().Name);

                if (options.RemoteStrategies.TryGetValue(targetRoleName, out var remoteStrategyType))
                {
                    var serviceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
                    var command = new RemoteMediatorCommand(request.Object, nextSpans);
                    var remoteResult = await InvokeRemoteAsync(serviceProvider, myRoleNames, targetRoleName, nextSpans,
                        command, remoteStrategyType, cancellationToken);

                    return remoteResult;
                }

                throw new InvalidOperationException($"'{targetRoleName}' is not contains the remote strategies.");
            }
        }

        return await _mediatorInvoker.InvokeAsync(request, cancellationToken);
    }

    private Task<RemoteMediatorResult?> InvokeRemoteAsync(IServiceProvider serviceProvider,
        IEnumerable<string> myRoleNames, string targetRoleName, IEnumerable<string> nextSpans,
        RemoteMediatorCommand command, StrategyTypes strategyTypes, CancellationToken cancellationToken)
    {
        IRemoteStrategy remoteStrategy;

        switch (command.Object)
        {
            case IRemoteRequest:
                remoteStrategy = (IRemoteStrategy)serviceProvider.GetRequiredService(strategyTypes.RequestStrategyType);
                break;

            case IRemoteNotification:
                remoteStrategy =
                    (IRemoteStrategy)serviceProvider.GetRequiredService(strategyTypes.NotificationStrategyType);
                break;

            default:
                throw new InvalidOperationException(
                    $"MediatorRemote is supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)}");
        }

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