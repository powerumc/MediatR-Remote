using MediatR.Remote.Kafka.RemoteStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Remote.Kafka;

public static class RemoteMediatorBuilderExtensions
{
    /// <summary>
    ///     Add Redis remote mediator.
    /// </summary>
    /// <param name="builder">RemoteMediatorBuilder</param>
    /// <param name="name">Role name</param>
    /// <param name="configure">Configure options</param>
    /// <param name="serviceLifetime">ServiceLifetime</param>
    /// <returns></returns>
    public static RemoteMediatorBuilder AddKafkaStrategy(this RemoteMediatorBuilder builder, string name,
        Action<KafkaMediatorOptions> configure, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
    {
        builder.AddQueueMessageProcessor<KafkaMessageProcessor>();
        builder.Services.AddHostedService<QueueBackgroundService>();

        var protocolRoleName = new ProtocolRoleName("kafka", name);
        builder.Add<RemoteKafkaStrategy, RemoteKafkaStrategy, RemoteKafkaStrategy>(protocolRoleName, serviceLifetime);

        configure(new KafkaMediatorOptions());
        builder.Services.Configure(protocolRoleName.ToString(), configure);

        return builder;
    }

    /// <summary>
    ///     Add queue message processor.
    /// </summary>
    /// <param name="builder">A Builder object</param>
    /// <typeparam name="TMessageProcessor">Custom queue message processor</typeparam>
    public static RemoteMediatorBuilder AddQueueMessageProcessor<TMessageProcessor>(this RemoteMediatorBuilder builder)
        where TMessageProcessor : KafkaMessageProcessor
    {
        builder.Services.AddSingleton<TMessageProcessor>();

        return builder;
    }
}
