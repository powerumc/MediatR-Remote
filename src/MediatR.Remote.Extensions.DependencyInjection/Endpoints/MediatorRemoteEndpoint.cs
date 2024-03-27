using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Extensions.DependencyInjection.Endpoints;

internal class MediatorRemoteEndpoint
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

    public async Task InvokeAsync(HttpContext httpContext, JsonObject jsonObject)
    {
        var options = _remoteMediatorOptions.CurrentValue;
        var jsonSerializerOptions = options.JsonSerializerOptions;
        var myRoleNames = options.MyRoleNames;

        if (!myRoleNames.Any(o => options.RemoteStrategies.ContainsKey(o)))
        {
            throw new InvalidOperationException(
                $"'{string.Join(',', myRoleNames)}' is not contains the remote strategies.");
        }

        var command = jsonObject.Deserialize<RemoteMediatorCommand>(jsonSerializerOptions)
                      ?? throw new InvalidOperationException(
                          $"Deserialized {nameof(RemoteMediatorCommand)} value must be not null.");

        using var disposable = _logger.BeginScope(nameof(InvokeAsync));
        _logger.LogReceivedMessage(command.Object?.GetType()!, command.Spans!);

        await InvokeInternalAsync(_mediator, command, httpContext, jsonSerializerOptions);
    }

    private static async Task InvokeInternalAsync(IMediator mediator, RemoteMediatorCommand command,
        HttpContext httpContext,
        JsonSerializerOptions jsonSerializerOptions)
    {
        switch (command.Object)
        {
            case IRemoteRequest:
                var result = await mediator.Send(command);

                httpContext.Response.ContentType = MediaTypeNames.Application.Json;
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, result, jsonSerializerOptions);
                break;

            case IRemoteNotification:
                await mediator.Publish(command);
                break;

            case IRemoteStreamRequest:
                httpContext.Response.ContentType = MediaTypeNames.Application.Json;

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
                    $"MediatorRemote is supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)}");
        }
    }
}

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Received RemoteCommand: type:{type}, spans:{spans}")]
    internal static partial void LogReceivedMessage(this ILogger logger, Type type, IEnumerable<string> spans);
}