using MediatR.Remote.RemoteStrategies;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MediatR.Remote.Redis.RemoteStrategies;

public class RemoteRedisStrategy(
    IServiceProvider serviceProvider,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
    IOptionsMonitor<RedisMediatorOptions> redisOptions) : RemoteStrategyBase
{
    protected override Task<RemoteMediatorResult?> SendInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override async Task NotificationInternalAsync(string targetRoleName, RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        var mediatorOptions = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var json = await mediatorOptions.Serializer.SerializeAsStringAsync(nextCommand, cancellationToken);
        var protocolRoleName = ProtocolRoleName.Generate(nextCommand.ProtocolName, targetRoleName);
        var options = redisOptions.Get(protocolRoleName);
        var channel = options.ChannelSelector(serviceProvider, targetRoleName);
        var subscriber = options.SubscriberSelector(serviceProvider, options.ConnectionMultiplexer);
        await subscriber.PublishAsync(channel, json, CommandFlags.FireAndForget);
    }

    protected override IAsyncEnumerable<RemoteMediatorStreamResult?> StreamInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
