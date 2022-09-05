using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote;

internal class RemoteMediatorStreamCommandHandler :
    IStreamRequestHandler<RemoteMediatorStreamCommand, RemoteMediatorStreamResult?>
{
    private readonly ILogger<RemoteMediatorStreamCommandHandler> _logger;
    private readonly IMediatorInvoker _mediatorInvoker;
    private readonly IOptionsMonitor<RemoteMediatorOptions> _remoteMediatorOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RemoteMediatorStreamCommandHandler(ILogger<RemoteMediatorStreamCommandHandler> logger,
        IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
        IServiceScopeFactory serviceScopeFactory,
        IMediatorInvoker mediatorInvoker)
    {
        _logger = logger;
        _remoteMediatorOptions = remoteMediatorOptions;
        _serviceScopeFactory = serviceScopeFactory;
        _mediatorInvoker = mediatorInvoker;
    }

    public IAsyncEnumerable<RemoteMediatorStreamResult?> Handle(RemoteMediatorStreamCommand request,
        CancellationToken cancellationToken)
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));
        _ = request.Object ?? throw new NullReferenceException(nameof(request.Object));

        if (request.Object is IRemoteStreamRequest remoteCommand)
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
                    var command = new RemoteMediatorStreamCommand(request.Object, nextSpans);
                    var remoteResult = InvokeRemoteStreamAsync(serviceProvider, myRoleNames, targetRoleName, nextSpans,
                        command, remoteStrategyType, cancellationToken);

                    return remoteResult;
                }

                throw new InvalidOperationException($"'{targetRoleName}' is not contains the remote strategies.");
            }
        }

        return _mediatorInvoker.InvokeStreamAsync(request, cancellationToken);
    }

    private IAsyncEnumerable<RemoteMediatorStreamResult?> InvokeRemoteStreamAsync(IServiceProvider serviceProvider,
        IEnumerable<string> myRoleNames, string targetRoleName, IEnumerable<string> nextSpans,
        RemoteMediatorStreamCommand command, StrategyTypes strategyTypes, CancellationToken cancellationToken)
    {
        var remoteStrategy = (IRemoteStrategy)serviceProvider.GetRequiredService(strategyTypes.StreamStrategyType);

        return remoteStrategy.InvokeStreamAsync(myRoleNames, targetRoleName, nextSpans, command, cancellationToken);
    }
}