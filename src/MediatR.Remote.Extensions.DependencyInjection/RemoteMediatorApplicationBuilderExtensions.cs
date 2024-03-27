using System.Text.Json.Nodes;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="RemoteMediatorApplicationBuilder" />.
/// </summary>
public static class RemoteMediatorApplicationBuilderExtensions
{
    /// <summary>
    /// Use the mediator remote style of middleware.
    /// </summary>
    public static RemoteMediatorApplicationBuilder UseHttpListener(this RemoteMediatorApplicationBuilder builder)
    {
        var serviceProvider = builder.ApplicationBuilder.ApplicationServices;
        var remoteMediatorOptions = serviceProvider.GetRequiredService<IOptions<RemoteMediatorOptions>>().Value;

        builder.ApplicationBuilder.UseEndpoints(endpointRouteBuilder =>
        {
            endpointRouteBuilder.MapPost(remoteMediatorOptions.MediatorRemoteEndpoint,
                ([FromServices] MediatorRemoteEndpoint endpoint,
                    HttpContext httpContext,
                    JsonObject jsonObject) => endpoint.InvokeAsync(httpContext, jsonObject));
        });

        return builder;
    }

    /// <summary>
    /// Use the mediator remote style of .NET 6 or higher.
    /// </summary>
    public static RouteHandlerBuilder MapHttpListener(this RemoteMediatorEndpointRouteBuilder builder)
    {
        var routeBuilder = builder.EndpointRouteBuilder;
        var options = routeBuilder.ServiceProvider.GetRequiredService<IOptions<RemoteMediatorOptions>>().Value;
        return routeBuilder.MapPost(options.MediatorRemoteEndpoint, ([FromServices] MediatorRemoteEndpoint endpoint,
            HttpContext httpContext,
            JsonObject jsonObject) => endpoint.InvokeAsync(httpContext, jsonObject));
    }
}
