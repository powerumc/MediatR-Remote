namespace MediatR.Remote.Extensions.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMediatRemote(this IApplicationBuilder app,
        Action<RemoteMediatorApplicationBuilder> builder)
    {
        builder?.Invoke(new RemoteMediatorApplicationBuilder(app));

        return app;
    }
}