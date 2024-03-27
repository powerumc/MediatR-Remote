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

        builder.WebApplication.UseEndpoints(endpointRouteBuilder =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RemoteMediatorOptions>>().Value;
            endpointRouteBuilder.MapPost(options.MediatorRemoteEndpoint,
                async (
                    [FromServices] MediatorRemoteEndpoint endpoint,
                    [FromBody] JsonObject jsonObject) =>
                {
                    var jsonSerializerOptions = options.JsonSerializerOptions;
                    var command = jsonObject.Deserialize<RemoteMediatorCommand>(jsonSerializerOptions);
                    ArgumentNullException.ThrowIfNull(command);

                    var result = await endpoint.InvokeAsync(command);
                    return Results.Json(result, jsonSerializerOptions);
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
        var options = routeBuilder.ServiceProvider.GetRequiredService<IOptions<RemoteMediatorOptions>>().Value;
        return routeBuilder.MapPost(options.MediatorRemoteEndpoint, async (
            [FromServices] MediatorRemoteEndpoint endpoint,
            JsonObject jsonObject) =>
        {
            var jsonSerializerOptions = options.JsonSerializerOptions;
            var command = jsonObject.Deserialize<RemoteMediatorCommand>(jsonSerializerOptions);
            ArgumentNullException.ThrowIfNull(command);

            var result = await endpoint.InvokeAsync(command);
            return Results.Json(result, jsonSerializerOptions);
        });
    }
}
