using System.Text;

namespace MediatR.Remote;

public interface IRemoteSerializer
{
    Task<Stream> SerializeAsync<T>(T value, CancellationToken cancellationToken = default);
    ValueTask<T?> DeserializeAsync<T>(Stream value, CancellationToken cancellationToken = default);
    IAsyncEnumerable<T?> DeserializeAsyncEnumerable<T>(Stream value, CancellationToken cancellationToken = default);
}

public static class RemoteSerializerExtensions
{
    public static async Task<string> SerializeAsStringAsync<T>(this IRemoteSerializer serializer, T value,
        CancellationToken cancellationToken = default)
    {
        var stream = await serializer.SerializeAsync(value, cancellationToken);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public static async Task<T?> DeserializeFromStringAsync<T>(this IRemoteSerializer serializer, string value,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
        return await serializer.DeserializeAsync<T>(stream, cancellationToken);
    }
}
