using System.Text.Json.Nodes;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Extensions.DependencyInjection;

public static class RemoteMediatorApplicationBuilderExtensions
{
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
}
