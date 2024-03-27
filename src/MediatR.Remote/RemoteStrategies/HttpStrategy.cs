using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.RemoteStrategies;

/// <summary>
/// A remote strategy that uses HTTP to send the request to the target role.
/// </summary>
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

    /// <summary>
    /// Invokes command the remote mediator.
    /// </summary>
    /// <param name="myRoleNames">My role name</param>
    /// <param name="targetRoleName">Target role name</param>
    /// <param name="nextSpans">Next role names</param>
    /// <param name="command">Remote mediator command object</param>
    /// <param name="cancellationToken">CancellationToken object</param>
    /// <returns>Invoked command result</returns>
    /// <exception cref="ArgumentNullException">If any parameters is null</exception>
    /// <exception cref="InvalidOperationException">If <param name="command"> is not <see cref="IRemoteRequest"/> or <see cref="IRemoteNotification"/></param></exception>
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

    /// <summary>
    /// Invokes stream the remote mediator.
    /// </summary>
    /// <param name="myRoleNames">My role name</param>
    /// <param name="targetRoleName">Target role name</param>
    /// <param name="nextSpans">Next role names.</param>
    /// <param name="command">Remote mediator stream object</param>
    /// <param name="cancellationToken">CancellationToken object</param>
    /// <returns>Invoked stream result</returns>
    /// <exception cref="ArgumentNullException">If any parameters is null</exception>
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
