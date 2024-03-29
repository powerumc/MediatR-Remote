using MediatR.Remote.AWS.SQS.RemoteStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Remote.AWS.SQS;

/// <summary>
///     Extensions Grpc remote mediator for <see cref="RemoteMediatorBuilder" />.
/// </summary>
public static class RemoteMediatorBuilderExtensions
{
    /// <summary>
    ///     Add Grpc remote mediator.
    /// </summary>
    /// <param name="builder">A Builder object</param>
    /// <param name="name">Role name</param>
    /// <param name="configure">Configure <see cref="HttpClient" /></param>
    /// <param name="serviceLifetime">Service lifetime</param>
    public static RemoteMediatorBuilder AddSqsStrategy(this RemoteMediatorBuilder builder, string name,
        Action<AwsSqsOptions> configure, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        builder.AddQueueMessageProcessor<AwsSqsMessageProcessor>();
        builder.Services.AddHostedService<QueueBackgroundService>();

        var protocolRoleName = new ProtocolRoleName("aws-sqs", name);
        builder.Add<RemoteAwsSqsStrategy, RemoteAwsSqsStrategy, RemoteAwsSqsStrategy>(protocolRoleName,
            serviceLifetime);
        configure(new AwsSqsOptions());
        builder.Services.Configure(protocolRoleName.ToString(), configure);

        return builder;
    }

    /// <summary>
    ///     Add queue message processor.
    /// </summary>
    /// <param name="builder">A Builder object</param>
    /// <typeparam name="TMessageProcessor">Custom queue message processor</typeparam>
    public static RemoteMediatorBuilder AddQueueMessageProcessor<TMessageProcessor>(this RemoteMediatorBuilder builder)
        where TMessageProcessor : AwsSqsMessageProcessor
    {
        builder.Services.AddSingleton<TMessageProcessor>();

        return builder;
    }
}
