using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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
        var httpClient = CreateHttpClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var json = JsonSerializer.Serialize(nextCommand, options.JsonSerializerOptions);
        var stringContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await httpClient.PostAsync(options.MediatorRemoteEndpoint, stringContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RemoteMediatorResult>(
            cancellationToken: cancellationToken,
            options: options.JsonSerializerOptions);
        return result;
    }

    protected override async Task NotificationInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand, CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var json = JsonSerializer.Serialize(nextCommand, options.JsonSerializerOptions);
        var stringContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await httpClient.PostAsync(options.MediatorRemoteEndpoint, stringContent, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    protected override async IAsyncEnumerable<RemoteMediatorStreamResult?> StreamInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var json = JsonSerializer.Serialize(nextCommand, options.JsonSerializerOptions);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, options.MediatorStreamRemoteEndpoint);
        httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var results = JsonSerializer.DeserializeAsyncEnumerable<RemoteMediatorStreamResult>(stream,
            options.JsonSerializerOptions, cancellationToken);

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
