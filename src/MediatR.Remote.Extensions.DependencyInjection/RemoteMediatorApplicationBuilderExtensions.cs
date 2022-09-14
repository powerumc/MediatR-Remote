using System.Text.Json.Nodes;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Extensions.DependencyInjection;

public static class RemoteMediatorApplicationBuilderExtensions
{
    public static RemoteMediatorApplicationBuilder UseHttpListener(this RemoteMediatorApplicationBuilder builder)
    {
        var remoteMediatorOptions = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<IOptions<RemoteMediatorOptions>>().Value;

        builder.ApplicationBuilder.UseEndpoints(builder =>
        {
            builder.MapPost(remoteMediatorOptions.MediatorRemoteEndpoint,
                ([FromServices] MediatorRemoteEndpoint endpoint,
                    HttpContext httpContext,
                    JsonObject jsonObject) => endpoint.InvokeAsync(httpContext, jsonObject));
        });

        return builder;
    }
}
