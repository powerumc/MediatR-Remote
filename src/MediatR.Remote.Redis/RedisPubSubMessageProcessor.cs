using MediatR.Remote.Extensions.DependencyInjection.Endpoints;

namespace MediatR.Remote.Redis;

/// <summary>
///     Redis pub/sub message processor.
/// </summary>
/// <param name="endpoint">Remote Endpoint</param>
public class RedisPubSubMessageProcessor(MediatorRemoteEndpoint endpoint)
    : IQueueMessageProcessor<RedisMediatorOptions, string>
{
    public virtual Task CreateQueueIfNotExistsAsync(string roleName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task AcknowledgeMessageAsync(RedisMediatorOptions options, string message,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnMessageAsync(RemoteMediatorCommand command, CancellationToken cancellationToken)
    {
        return endpoint.InvokeAsync(command, cancellationToken);
    }

    public virtual Task OnMessageExceptionAsync(RedisMediatorOptions options, string message, Exception exception,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
