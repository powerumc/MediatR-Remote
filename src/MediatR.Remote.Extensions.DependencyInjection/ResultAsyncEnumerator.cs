// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;

namespace MediatR.Remote.Extensions.DependencyInjection;

public class ResultAsyncEnumerable<T>(
    IAsyncEnumerable<T> results,
    JsonSerializerOptions jsonSerializerOptions) : IAsyncEnumerable<T>
{
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return results.GetAsyncEnumerator(cancellationToken);
    }

    public async Task ExecuteStream(HttpResponse response, CancellationToken cancellationToken = default)
    {
        response.ContentType = "application/json";
        response.StatusCode = StatusCodes.Status200OK;
        await response.WriteAsync("[", cancellationToken);

        var count = 0;
        await foreach (var item in new ResultAsyncEnumerable<T>(results, jsonSerializerOptions))
        {
            if (count != 0)
            {
                await response.WriteAsync(",", cancellationToken);
            }

            await response.WriteAsync(JsonSerializer.Serialize(item, jsonSerializerOptions), cancellationToken);
            await response.BodyWriter.FlushAsync(cancellationToken);
            count++;
        }

        await response.WriteAsync("]", cancellationToken);
    }
}
