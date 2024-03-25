using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote;

/// <summary>
///     A mediator stream handler implementation that can handle remote requests.
/// </summary>
internal class RemoteMediatorStreamCommandHandler : RemoteMediatorCommandHandlerBase,
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

        if (request.Object is not IRemoteStreamRequest remoteCommand)
        {
            return _mediatorInvoker.InvokeStreamAsync(request, cancellationToken);
        }

        var options = _remoteMediatorOptions.CurrentValue;
        var myRoleNames = options.MyRoleNames;
        var requestSpans = request.Spans;
        var protocolName = request.ProtocolName;
        var (nextSpans, targetRoleName) = GetNextSpans(remoteCommand, requestSpans, myRoleNames);

        if (targetRoleName == null)
        {
            return _mediatorInvoker.InvokeStreamAsync(request, cancellationToken);
        }

        using var disposable = _logger.BeginScope(nameof(RemoteMediatorCommandHandler));
        _logger.LogBeginHandler(myRoleNames, targetRoleName, request.Object.GetType().Name);

        var roleName = new ProtocolRoleName(protocolName, targetRoleName);
        if (!options.RemoteStrategies.TryGetValue(roleName, out var remoteStrategyType))
        {
            throw new InvalidOperationException($"'{targetRoleName}' is not contains the remote strategies.");
        }

        var serviceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
        var command = new RemoteMediatorStreamCommand(request.Object, request.ProtocolName, nextSpans);
        var remoteResult = InvokeRemoteStreamAsync(serviceProvider, myRoleNames, targetRoleName, nextSpans,
            command, remoteStrategyType, cancellationToken);

        return remoteResult;
    }

    private IAsyncEnumerable<RemoteMediatorStreamResult?> InvokeRemoteStreamAsync(IServiceProvider serviceProvider,
        IEnumerable<string> myRoleNames, string targetRoleName, IEnumerable<string> nextSpans,
        RemoteMediatorStreamCommand command, StrategyTypes strategyTypes, CancellationToken cancellationToken)
    {
        var remoteStrategy = (IRemoteStrategy)serviceProvider.GetRequiredService(strategyTypes.StreamStrategyType);

        return remoteStrategy.InvokeStreamAsync(myRoleNames, targetRoleName, nextSpans, command, cancellationToken);
    }
}
