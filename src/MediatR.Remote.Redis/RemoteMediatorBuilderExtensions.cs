// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MediatR.Remote.Redis.RemoteStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Remote.Redis;

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
    public static RemoteMediatorBuilder AddRedisStrategy(this RemoteMediatorBuilder builder, string name,
        Action<RedisMediatorOptions> configure, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
    {
        builder.AddQueueMessageProcessor<RedisPubSubMessageProcessor>();

        var protocolRoleName = new ProtocolRoleName("redis", name);
        builder.Add<RemoteRedisStrategy, RemoteRedisStrategy, RemoteRedisStrategy>(protocolRoleName, serviceLifetime);

        configure(new RedisMediatorOptions());
        builder.Services.Configure(protocolRoleName.ToString(), configure);

        return builder;
    }

    /// <summary>
    ///     Add queue message processor.
    /// </summary>
    /// <param name="builder">A Builder object</param>
    /// <typeparam name="TMessageProcessor">Custom queue message processor</typeparam>
    public static RemoteMediatorBuilder AddQueueMessageProcessor<TMessageProcessor>(this RemoteMediatorBuilder builder)
        where TMessageProcessor : RedisPubSubMessageProcessor
    {
        builder.Services.AddSingleton<TMessageProcessor>();

        return builder;
    }
}
