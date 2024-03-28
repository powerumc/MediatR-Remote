using StackExchange.Redis;

namespace MediatR.Remote.Redis;

public class RedisMediatorOptions
{
    public ConnectionMultiplexer ConnectionMultiplexer { get; set; }

    public Func<IServiceProvider, ConnectionMultiplexer, ISubscriber> SubscriberSelector { get; set; }
        = (provider, multiplexer) => multiplexer.GetSubscriber();

    public Func<IServiceProvider, string, RedisChannel> ChannelSelector { get; set; }
        = (provider, roleName) => roleName;
}
