using System.Runtime.CompilerServices;

namespace MediatR.Remote;

/// <summary>
///     A mediator implementation that can handle remote requests.
/// </summary>
internal class RemoteMediator(IMediator mediator) : IRemoteMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = new())
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteRequest)
        {
            var response = await mediator.Send(new RemoteMediatorCommand(request), cancellationToken);
            return (TResponse)response.Object!;
        }

        return await mediator.Send(request, cancellationToken);
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = new())
        where TRequest : IRequest
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteRequest)
        {
            await mediator.Send(new RemoteMediatorCommand(request), cancellationToken);
        }

        await mediator.Send(request, cancellationToken);
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = new())
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteRequest)
        {
            var response = await mediator.Send(new RemoteMediatorCommand(request), cancellationToken);
            return response.Object;
        }

        return await mediator.Send(request, cancellationToken);
    }

    public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteStreamRequest)
        {
            var command = new RemoteMediatorStreamCommand(request);
            var stream = mediator.CreateStream(command, cancellationToken).WithCancellation(cancellationToken);

            await foreach (var item in stream)
            {
                if (item.Object is null)
                {
                    continue;
                }

                yield return (TResponse)item.Object!;
            }

            yield break;
        }

        await foreach (var item in mediator.CreateStream(request, cancellationToken))
        {
            yield return item;
        }
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = new())
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteStreamRequest)
        {
            return mediator.CreateStream(new RemoteMediatorStreamCommand(request), cancellationToken);
        }

        return mediator.CreateStream(request, cancellationToken);
    }

    public async Task Publish(object notification, CancellationToken cancellationToken = new())
    {
        _ = notification ?? throw new ArgumentNullException(nameof(notification));

        if (notification is IRemoteNotification)
        {
            await mediator.Publish(new RemoteMediatorCommand(notification), cancellationToken);
            return;
        }

        await mediator.Publish(notification, cancellationToken);
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new())
        where TNotification : INotification
    {
        _ = notification ?? throw new ArgumentNullException(nameof(notification));

        if (notification is IRemoteNotification)
        {
            await mediator.Publish(new RemoteMediatorCommand(notification), cancellationToken);
            return;
        }

        await mediator.Publish(notification, cancellationToken);
    }
}
