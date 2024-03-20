using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Grpc.Endpoints;

/// <summary>
///     Invokes the Mediator to handle the <see cref="RemoteMediatorCommand" /> and
///     <see cref="RemoteMediatorStreamCommand" />
/// </summary>
public class MediatorRemoteEndpoint
{
    private readonly ILogger<MediatorRemoteEndpoint> _logger;
    private readonly IMediator _mediator;
    private readonly IOptionsMonitor<RemoteMediatorOptions> _remoteMediatorOptions;

    public MediatorRemoteEndpoint(IMediator mediator,
        IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
        ILogger<MediatorRemoteEndpoint> logger)
    {
        _mediator = mediator;
        _remoteMediatorOptions = remoteMediatorOptions;
        _logger = logger;
    }

    /// <summary>
    ///     Invokes the Mediator to handle the <see cref="IRemoteCommand" />
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext" /> object</param>
    /// <param name="jsonObject">Received json</param>
    /// <exception cref="InvalidOperationException">If <paramref name="jsonObject" /> is null</exception>
    public Task<RemoteMediatorResult> InvokeAsync(HttpContext httpContext, string obj)
    {
        var options = _remoteMediatorOptions.CurrentValue;
        var jsonSerializerOptions = options.JsonSerializerOptions;
        var command = JsonSerializer.Deserialize<RemoteMediatorCommand>(obj, jsonSerializerOptions)
                      ?? throw new InvalidOperationException(
                          $"Deserialized {nameof(RemoteMediatorCommand)} value must be not null.");

        using var disposable = _logger.BeginScope(nameof(InvokeAsync));
        _logger.LogReceivedMessage(command.Object?.GetType()!, command.Spans!);

        return InvokeInternalAsync(_mediator, command, httpContext, jsonSerializerOptions);
    }

    private static async Task<RemoteMediatorResult> InvokeInternalAsync(IMediator mediator,
        RemoteMediatorCommand command,
        HttpContext httpContext,
        JsonSerializerOptions jsonSerializerOptions)
    {
        switch (command.Object)
        {
            case IRemoteRequest:
                var result = await mediator.Send(command);
                //await JsonSerializer.SerializeAsync(httpContext.Response.Body, result, jsonSerializerOptions);
                return result;

            case IRemoteNotification:
                await mediator.Publish(command);
                return null;

            case IRemoteStreamRequest:
                var streamCommand = new RemoteMediatorStreamCommand(command.Object, command.Spans);
                var stream = mediator.CreateStream(streamCommand);

                await httpContext.Response.WriteAsync("[", Encoding.UTF8);
                await httpContext.Response.Body.FlushAsync();

                var count = 0;
                await foreach (var item in stream)
                {
                    if (count != 0)
                    {
                        await httpContext.Response.WriteAsync(",", Encoding.UTF8);
                    }

                    await httpContext.Response.WriteAsync(JsonSerializer.Serialize(item, jsonSerializerOptions),
                        Encoding.UTF8);
                    await httpContext.Response.Body.FlushAsync();

                    count++;
                }

                await httpContext.Response.WriteAsync("]");
                await httpContext.Response.Body.FlushAsync();
                break;

            default:
                throw new InvalidOperationException(
                    $"{nameof(IRemoteMediator)} is only supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)} and {nameof(IRemoteStreamRequest)}");
        }

        throw new NotImplementedException();
    }
}

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Received RemoteCommand: type:{type}, spans:{spans}")]
    internal static partial void LogReceivedMessage(this ILogger logger, Type type, IEnumerable<string> spans);
}
