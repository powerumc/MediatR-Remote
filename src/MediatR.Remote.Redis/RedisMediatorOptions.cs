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
    public Func<IServiceProvider, ConnectionMultiplexer, ISubscriber> Subscriber { get; set; }
        = (provider, multiplexer) => multiplexer.GetSubscriber();

    public Func<IServiceProvider, RemoteMediatorCommand, string, RedisChannel> MessageChannelGenerator { get; set; }
        = (provider, command, targetRoleName) => targetRoleName;

    /// <summary>
    ///     Channel selector. Default is to use the role name.
    /// </summary>
    public Func<IServiceProvider, string, IEnumerable<RedisChannel>> SubscribeChannels { get; set; }
        = (provider, roleName) => new[] { new RedisChannel(roleName, RedisChannel.PatternMode.Auto) };
}
