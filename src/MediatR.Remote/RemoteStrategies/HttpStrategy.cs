using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.RemoteStrategies;

/// <summary>
///     A remote strategy that uses HTTP to send the request to the target role.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RemoteHttpStrategy(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions)
    : RemoteStrategyBase
{
    protected override async Task<RemoteMediatorResult?> SendInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand, CancellationToken cancellationToken)
    {
        using var httpClient = CreateHttpClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var stream = await options.Serializer.SerializeAsync(nextCommand, cancellationToken);
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

        using var response = await httpClient.PostAsync(options.MediatorRemoteEndpoint, streamContent,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await options.Serializer.DeserializeAsync<RemoteMediatorResult>(responseStream, cancellationToken);
        return result;
    }

    protected override async Task NotificationInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand, CancellationToken cancellationToken)
    {
        using var httpClient = CreateHttpClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var stream = await options.Serializer.SerializeAsync(nextCommand, cancellationToken);
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

        using var response =
            await httpClient.PostAsync(options.MediatorRemoteEndpoint, streamContent, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    protected override async IAsyncEnumerable<RemoteMediatorStreamResult?> StreamInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var httpClient = CreateHttpClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var requestStream = await options.Serializer.SerializeAsync(nextCommand, cancellationToken);
        using var streamContent = new StreamContent(requestStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, options.MediatorStreamRemoteEndpoint);
        httpRequestMessage.Content = streamContent;

        using var response = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        var results =
            options.Serializer.DeserializeAsyncEnumerable<RemoteMediatorStreamResult>(stream, cancellationToken);

        await foreach (var result in results)
        {
            yield return result;
        }
    }

    private HttpClient CreateHttpClient(string targetRoleName, RemoteMediatorCommand nextCommand)
    {
        var httpClientName = ProtocolRoleName.Generate(nextCommand.ProtocolName, targetRoleName);
        var httpClient = httpClientFactory.CreateClient(httpClientName);
        return httpClient;
    }
}
