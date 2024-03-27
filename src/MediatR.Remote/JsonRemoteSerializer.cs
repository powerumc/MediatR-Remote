// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;

namespace MediatR.Remote;

public class JsonRemoteSerializer(JsonSerializerOptions jsonSerializerOptions)
    : IRemoteSerializer
{
    public async Task<Stream> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, jsonSerializerOptions, cancellationToken);

        stream.Position = 0;
        return stream;
    }

    public ValueTask<T?> DeserializeAsync<T>(Stream value, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsync<T>(value, jsonSerializerOptions, cancellationToken);
    }

    public IAsyncEnumerable<T?> DeserializeAsyncEnumerable<T>(Stream value,
        CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsyncEnumerable<T>(value, jsonSerializerOptions, cancellationToken);
    }
}
