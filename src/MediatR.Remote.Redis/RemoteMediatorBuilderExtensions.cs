// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MediatR.Remote.Redis.RemoteStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Remote.Redis;

public static class RemoteMediatorBuilderExtensions
{
    public static RemoteMediatorBuilder AddRedisStrategy(this RemoteMediatorBuilder builder, string name,
        Action<RedisMediatorOptions> configure, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
    {
        var protocolRoleName = new ProtocolRoleName("redis", name);
        builder.Add<RemoteRedisStrategy, RemoteRedisStrategy, RemoteRedisStrategy>(protocolRoleName, serviceLifetime);

        configure(new RedisMediatorOptions());
        builder.Services.Configure(protocolRoleName.ToString(), configure);

        return builder;
    }
}
