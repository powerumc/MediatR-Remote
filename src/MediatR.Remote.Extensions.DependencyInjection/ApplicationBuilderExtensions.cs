namespace MediatR.Remote.Extensions.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Use the mediator remote style of middleware.
    /// It needs <see cref="RoutingBuilderExtensions.UseRouter(Microsoft.AspNetCore.Builder.IApplicationBuilder,Microsoft.AspNetCore.Routing.IRouter)"/>.
    /// </summary>
    public static IApplicationBuilder UseMediatRemote(this IApplicationBuilder app,
        Action<RemoteMediatorApplicationBuilder> builder)
    {
        builder?.Invoke(new RemoteMediatorApplicationBuilder(app));

        return app;
    }

    /// <summary>
    /// Use the mediator remote style of .NET 6 or higher.
    /// </summary>
    public static IEndpointRouteBuilder UseMediatRemote(this IEndpointRouteBuilder app, 
        Action<RemoteMediatorEndpointRouteBuilder> builder)
    {
        builder?.Invoke(new RemoteMediatorEndpointRouteBuilder(app));

        return app;
    }
}
