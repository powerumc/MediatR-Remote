using StackExchange.Redis;

namespace MediatR.Remote.Redis;

/// <summary>
///     Redis mediator options.
/// </summary>
public class RedisMediatorOptions
{
    /// <summary>
    ///     Connection multiplexer.
    /// </summary>
    public ConnectionMultiplexer ConnectionMultiplexer { get; set; }

    /// <summary>
    ///     Subscriber selector.
    /// </summary>
    public Func<IServiceProvider, ConnectionMultiplexer, ISubscriber> SubscriberSelector { get; set; }
        = (provider, multiplexer) => multiplexer.GetSubscriber();

    /// <summary>
    ///     Channel selector. Default is to use the role name.
    /// </summary>
    public Func<IServiceProvider, string, RedisChannel> ChannelSelector { get; set; }
        = (provider, roleName) => roleName;
}
