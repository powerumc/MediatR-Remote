using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Extensions.DependencyInjection;

/// <summary>
///     Extension methods for <see cref="RemoteMediatorApplicationBuilder" />.
/// </summary>
public static class RemoteMediatorApplicationBuilderExtensions
{
    /// <summary>
    ///     Use the mediator remote style of middleware.
    /// </summary>
    public static RemoteMediatorApplicationBuilder UseHttpListener(this RemoteMediatorApplicationBuilder builder)
    {
        var serviceProvider = builder.WebApplication.Services;
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<RemoteMediatorOptions>>().Get("http");
        builder.WebApplication.UseEndpoints(endpointRouteBuilder =>
        {
            endpointRouteBuilder.MapPost(options.MediatorRemoteEndpoint,
                async (
                    [FromServices] MediatorRemoteEndpoint endpoint,
                    [FromBody] JsonObject jsonObject,
                    CancellationToken cancellationToken) =>
                {
                    var jsonSerializerOptions = options.JsonSerializerOptions;
                    var result = await InvokeAsync(jsonObject, jsonSerializerOptions, endpoint, cancellationToken);
                    return Results.Json(result, jsonSerializerOptions);
                });

            endpointRouteBuilder.MapPost(options.MediatorStreamRemoteEndpoint, async (
                [FromServices] MediatorRemoteEndpoint endpoint,
                [FromBody] JsonObject jsonObject,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var jsonSerializerOptions = options.JsonSerializerOptions;
                var result = InvokeStreamAsync(jsonObject, jsonSerializerOptions, endpoint, cancellationToken);
                await new ResultAsyncEnumerable<RemoteMediatorStreamResult>(result, jsonSerializerOptions)
                    .ExecuteStream(context.Response, cancellationToken);
            });
        });

        return builder;
    }

    /// <summary>
    ///     Use the mediator remote style of .NET 6 or higher.
    /// </summary>
    public static RouteHandlerBuilder MapHttpListener(this RemoteMediatorEndpointRouteBuilder builder)
    {
        var routeBuilder = builder.EndpointRouteBuilder;
        var options = routeBuilder.ServiceProvider.GetRequiredService<IOptionsMonitor<RemoteMediatorOptions>>()
            .Get("http");
        routeBuilder.MapPost(options.MediatorRemoteEndpoint, async (
            [FromServices] MediatorRemoteEndpoint endpoint,
            JsonObject jsonObject,
            CancellationToken cancellationToken) =>
        {
            var jsonSerializerOptions = options.JsonSerializerOptions;
            var result = await InvokeAsync(jsonObject, jsonSerializerOptions, endpoint, cancellationToken);
            return Results.Json(result, jsonSerializerOptions);
        });

        return routeBuilder.MapPost(options.MediatorStreamRemoteEndpoint, async (
            [FromServices] MediatorRemoteEndpoint endpoint,
            [FromBody] JsonObject jsonObject,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var jsonSerializerOptions = options.JsonSerializerOptions;
            var result = InvokeStreamAsync(jsonObject, jsonSerializerOptions, endpoint, cancellationToken);
            await new ResultAsyncEnumerable<RemoteMediatorStreamResult>(result, jsonSerializerOptions)
                .ExecuteStream(context.Response, cancellationToken);
        });
    }

    private static async Task<RemoteMediatorResult> InvokeAsync(JsonObject jsonObject,
        JsonSerializerOptions jsonSerializerOptions,
        MediatorRemoteEndpoint endpoint, CancellationToken cancellationToken)
    {
        var command = jsonObject.Deserialize<RemoteMediatorCommand>(jsonSerializerOptions);
        ArgumentNullException.ThrowIfNull(command);

        var result = await endpoint.InvokeAsync(command, cancellationToken);
        return result;
    }

    private static IAsyncEnumerable<RemoteMediatorStreamResult> InvokeStreamAsync(JsonObject jsonObject,
        JsonSerializerOptions jsonSerializerOptions,
        MediatorRemoteEndpoint endpoint, CancellationToken cancellationToken)
    {
        var command = jsonObject.Deserialize<RemoteMediatorStreamCommand>(jsonSerializerOptions);
        ArgumentNullException.ThrowIfNull(command);

        var result = endpoint.InvokeStreamAsync(command, cancellationToken);
        return result;
    }
}
