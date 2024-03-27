using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Extensions.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMediatRemote(this IApplicationBuilder app)
    {
        var remoteMediatorOptions = app.ApplicationServices.GetRequiredService<IOptions<RemoteMediatorOptions>>().Value;

        app.UseEndpoints(builder =>
        {
            builder.MapPost(remoteMediatorOptions.MediatorRemoteEndpoint,
                async ([FromServices] IServiceScopeFactory serviceScopeFactory,
                    [FromServices] IMediator mediator,
                    [FromServices] IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
                    [FromServices] ILoggerFactory loggerFactory,
                    HttpContext httpContext,
                    JsonObject jsonObject) =>
                {
                    // TODO: 로그가 찍히지 않는 부분 검토
                    var logger = loggerFactory.CreateLogger(nameof(UseMediatRemote));
                    var options = remoteMediatorOptions.CurrentValue;
                    var jsonSerializerOptions = options.JsonSerializerOptions;
                    var myRoleNames = options.MyRoleNames;

                    if (!myRoleNames.Any(o => options.RemoteStrategies.ContainsKey(o)))
                    {
                        throw new InvalidOperationException(
                            $"'{string.Join(',', myRoleNames)}' is not contains the remote strategies.");
                    }

                    using var disposable = logger.BeginScope(nameof(UseMediatRemote));
                    logger.LogReceivedMessage();

                    var command = jsonObject.Deserialize<RemoteMediatorCommand>(jsonSerializerOptions)
                                  ?? throw new InvalidOperationException(
                                      $"Deserialized {nameof(RemoteMediatorCommand)} value must be not null.");

                    await InvokeAsync(mediator, command, httpContext, jsonSerializerOptions);
                });
        });

        return app;
    }

    private static async Task InvokeAsync(IMediator mediator, RemoteMediatorCommand command, HttpContext httpContext,
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
                // httpContext.Response.ContentType = "text/event-stream";
                httpContext.Response.ContentType = MediaTypeNames.Application.Json;

                var streamCommand = new RemoteMediatorStreamCommand(command.Object, command.Spans);
                var stream = mediator.CreateStream(streamCommand);

                await httpContext.Response.WriteAsync("[", Encoding.UTF8);
                await httpContext.Response.Body.FlushAsync();

                var count = 0;
                await foreach (var item in stream)
                {
                    Console.WriteLine(JsonSerializer.Serialize(item, jsonSerializerOptions));

                    if (count != 0)
                    {
                        await httpContext.Response.WriteAsync(",", Encoding.UTF8);
                    }

                    await httpContext.Response.WriteAsync("" + JsonSerializer.Serialize(item, jsonSerializerOptions),
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
    [LoggerMessage(Level = LogLevel.Trace, Message = "UseMediatRemote: ")]
    internal static partial void LogReceivedMessage(this ILogger logger);
}