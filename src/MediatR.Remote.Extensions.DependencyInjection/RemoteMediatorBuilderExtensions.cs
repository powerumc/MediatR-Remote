using System.Text.Json;
using MediatR.Remote.RemoteStrategies;

namespace MediatR.Remote.Extensions.DependencyInjection;

public interface IRemoteJsonSerializer
{
    string Serialize<T>(T value) where T : IRemoteRequest;
    T? Deserialize<T>(string value) where T : IRemoteRequest;
}

public class RemoteJsonSerializer(JsonSerializerOptions? jsonSerializerOptions) : IRemoteJsonSerializer
{
    public string Serialize<T>(T value) where T : IRemoteRequest
    {
        return JsonSerializer.Serialize(value, jsonSerializerOptions);
    }

    public T? Deserialize<T>(string value) where T : IRemoteRequest
    {
        return JsonSerializer.Deserialize<T>(value, jsonSerializerOptions);
    }
}

/// <summary>
///     Extensions HTTP remote mediator for <see cref="RemoteMediatorBuilder" />.
/// </summary>
public static class RemoteMediatorBuilderExtensions
{
    public static RemoteMediatorBuilder AddJsonSerializer(this RemoteMediatorBuilder builder,
        Action<JsonSerializerOptions> configure)
    {
        var jsonSerializerOptions = new JsonSerializerOptions();
        configure(jsonSerializerOptions);

        builder.Services.AddSingleton<IRemoteJsonSerializer>(new RemoteJsonSerializer(jsonSerializerOptions));

        return builder;
    }

    /// <summary>
    ///     Add HTTP remote mediator.
    /// </summary>
    /// <param name="builder">A Builder object</param>
    /// <param name="name">Role name</param>
    /// <param name="configureClient">Configure <see cref="HttpClient" /></param>
    /// <param name="serviceLifetime">Service lifetime</param>
    public static RemoteMediatorBuilder AddHttpStrategy(this RemoteMediatorBuilder builder, string name,
        Action<HttpClient> configureClient, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var protocolRoleName = new ProtocolRoleName("http", name);
        builder.Add<RemoteHttpStrategy, RemoteHttpStrategy, RemoteHttpStrategy>(protocolRoleName, serviceLifetime);
        builder.Services.AddHttpClient(protocolRoleName.ToString(), configureClient);

        return builder;
    }
}
