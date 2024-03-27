namespace MediatR.Remote.Extensions.DependencyInjection;

public class RemoteMediatorApplicationBuilder
{
    public RemoteMediatorApplicationBuilder(IApplicationBuilder applicationBuilder)
    {
        ApplicationBuilder = applicationBuilder;
    }

    public IApplicationBuilder ApplicationBuilder { get; }
}
