using MediatR.Remote.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MediatR.Remote.Redis;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRedisListener(this RemoteMediatorApplicationBuilder builder)
    {
        var serviceProvider = builder.WebApplication.Services;
        var remoteOptions = serviceProvider.GetRequiredService<IOptionsMonitor<RemoteMediatorOptions>>().Get("redis");
        var protocolName = ProtocolRoleName.Generate("redis", remoteOptions.MyRoleNames.First());
        var redisOptions = serviceProvider.GetRequiredService<IOptionsMonitor<RedisMediatorOptions>>()
            .Get(protocolName);

        var processor = serviceProvider.GetRequiredService<RedisPubSubMessageProcessor>();
        var subscriber = redisOptions.Subscriber(serviceProvider, redisOptions.ConnectionMultiplexer);
        var subscribeChannels =
            redisOptions.SubscribeChannels(serviceProvider, remoteOptions.MyRoleNames.First());

        foreach (var channel in subscribeChannels)
        {
            subscriber.Subscribe(channel, async (_, value) =>
            {
                await HandleAsync(value, remoteOptions, processor, redisOptions);
            });
        }

        var lifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() =>
        {
            subscriber.UnsubscribeAll();
        });

        return builder.WebApplication;
    }

    private static async Task HandleAsync(RedisValue value, RemoteMediatorOptions remoteOptions,
        RedisPubSubMessageProcessor processor, RedisMediatorOptions redisOptions)
    {
        try
        {
            if (!value.HasValue)
            {
                return;
            }

            var command = await remoteOptions.Serializer.DeserializeFromStringAsync<RemoteMediatorCommand>(value!);
            if (command is null)
            {
                return;
            }

            await processor.OnMessageAsync(command, default);
        }
        catch (Exception e)
        {
            await processor.OnMessageExceptionAsync(redisOptions, value!, e, default);
        }
    }
}
