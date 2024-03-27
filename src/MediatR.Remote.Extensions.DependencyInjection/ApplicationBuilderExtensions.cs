using System.Text.Json.Nodes;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
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
                ([FromServices] MediatorRemoteEndpoint endpoint,
                    HttpContext httpContext,
                    JsonObject jsonObject) => endpoint.InvokeAsync(httpContext, jsonObject));
        });

        return app;
    }
}