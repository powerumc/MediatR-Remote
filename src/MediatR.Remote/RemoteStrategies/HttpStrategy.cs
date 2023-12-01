using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.RemoteStrategies;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RemoteHttpStrategy : IRemoteStrategy
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<RemoteMediatorOptions> _remoteMediatorOptions;

    public RemoteHttpStrategy(IHttpClientFactory httpClientFactory,
        IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions)
    {
        _httpClientFactory = httpClientFactory;
        _remoteMediatorOptions = remoteMediatorOptions;
    }

    public async Task<RemoteMediatorResult?> InvokeAsync(IEnumerable<string> myRoleNames, string targetRoleName,
        IEnumerable<string> nextSpans, RemoteMediatorCommand command, CancellationToken cancellationToken = default)
    {
        _ = myRoleNames ?? throw new ArgumentNullException(nameof(myRoleNames));
        _ = targetRoleName ?? throw new ArgumentNullException(nameof(targetRoleName));
        _ = nextSpans ?? throw new ArgumentNullException(nameof(nextSpans));
        _ = command ?? throw new ArgumentNullException(nameof(command));

        var options = _remoteMediatorOptions.CurrentValue;
        var httpClient = _httpClientFactory.CreateClient(targetRoleName);
        var newCommand = new RemoteMediatorCommand(command.Object, nextSpans);
        var json = JsonSerializer.Serialize(newCommand, options.JsonSerializerOptions);
        var stringContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        switch (command.Object)
        {
            case IRemoteRequest:
                var result = await SendInternalAsync(httpClient, stringContent, options, cancellationToken);
                return result;

            case IRemoteNotification:
                await NotificationInternalAsync(httpClient, stringContent, options, cancellationToken);
                return null;

            default:
                throw new InvalidOperationException(
                    $"MediatorRemote does only supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)}");
        }
    }

    public async IAsyncEnumerable<RemoteMediatorStreamResult?> InvokeStreamAsync(IEnumerable<string> myRoleNames,
        string targetRoleName, IEnumerable<string> nextSpans, RemoteMediatorStreamCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = myRoleNames ?? throw new ArgumentNullException(nameof(myRoleNames));
        _ = targetRoleName ?? throw new ArgumentNullException(nameof(targetRoleName));
        _ = nextSpans ?? throw new ArgumentNullException(nameof(nextSpans));
        _ = command ?? throw new ArgumentNullException(nameof(command));

        var options = _remoteMediatorOptions.CurrentValue;
        var httpClient = _httpClientFactory.CreateClient(targetRoleName);
        var newCommand = new RemoteMediatorCommand(command.Object, nextSpans);
        var json = JsonSerializer.Serialize(newCommand, options.JsonSerializerOptions);
        var stringContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, options.MediatorRemoteEndpoint);
        httpRequestMessage.Content = stringContent;
        var response = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var stream = await response.Content.ReadAsStreamAsync();
        var newOptions = new JsonSerializerOptions(options.JsonSerializerOptions)
        {
            DefaultBufferSize = 1
        };
        var asyncEnumerable = JsonSerializer.DeserializeAsyncEnumerable<RemoteMediatorStreamResult?>(stream,
            newOptions, cancellationToken);

        await foreach (var item in asyncEnumerable)
        {
            yield return item;
        }
    }

    private static async Task<RemoteMediatorResult?> SendInternalAsync(HttpClient httpClient,
        HttpContent stringContent, RemoteMediatorOptions options, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync(options.MediatorRemoteEndpoint, stringContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RemoteMediatorResult>(
            cancellationToken: cancellationToken,
            options: options.JsonSerializerOptions);
        return result;
    }

    private static async Task NotificationInternalAsync(HttpClient httpClient,
        StringContent stringContent, RemoteMediatorOptions options, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync(options.MediatorRemoteEndpoint, stringContent, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
