using MediatR.Remote.Extensions.DependencyInjection;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Redis;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRedisListener(this RemoteMediatorApplicationBuilder builder)
    {
        var serviceProvider = builder.WebApplication.Services;
        var endpoint = serviceProvider.GetRequiredService<MediatorRemoteEndpoint>();
        var remoteOptions = serviceProvider.GetRequiredService<IOptionsMonitor<RemoteMediatorOptions>>().Get("redis");
        var protocolName = ProtocolRoleName.Generate("redis", remoteOptions.MyRoleNames.First());
        var redisOptions = serviceProvider.GetRequiredService<IOptionsMonitor<RedisMediatorOptions>>()
            .Get(protocolName);

        var channel = redisOptions.ChannelSelector(serviceProvider, remoteOptions.MyRoleNames.First());
        var subscriber = redisOptions.SubscriberSelector(serviceProvider, redisOptions.ConnectionMultiplexer);
        subscriber.SubscribeAsync(channel, async (_, value) =>
        {
            var command = await remoteOptions.Serializer.DeserializeFromStringAsync<RemoteMediatorCommand>(value);
            await endpoint.InvokeAsync(command!, CancellationToken.None);
        });

        return builder.WebApplication;
    }
}
