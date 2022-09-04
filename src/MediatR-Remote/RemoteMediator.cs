using System.Runtime.CompilerServices;

namespace MediatR.Remote;

internal class RemoteMediator : IRemoteMediator
{
    private readonly IMediator _mediator;

    public RemoteMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = new())
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteRequest)
        {
            var response = await _mediator.Send(new RemoteMediatorCommand(request), cancellationToken);
            return (TResponse)response.Object!;
        }

        return await _mediator.Send(request, cancellationToken);
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = new())
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteRequest)
        {
            var response = await _mediator.Send(new RemoteMediatorCommand(request), cancellationToken);
            return response.Object;
        }

        return await _mediator.Send(request, cancellationToken);
    }

    public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteStreamRequest)
        {
            var command = new RemoteMediatorStreamCommand(request);
            var stream = _mediator.CreateStream(command, cancellationToken).WithCancellation(cancellationToken);

            await foreach (var item in stream)
            {
                if (item.Object is null) continue;

                yield return (TResponse)item.Object!;
            }
            
            yield break;
        }

        await foreach (var item in _mediator.CreateStream(request, cancellationToken))
        {
            yield return item;
        }
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = new())
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        if (request is IRemoteStreamRequest)
        {
            return _mediator.CreateStream(new RemoteMediatorStreamCommand(request), cancellationToken);
        }

        return _mediator.CreateStream(request, cancellationToken);
    }

    public async Task Publish(object notification, CancellationToken cancellationToken = new())
    {
        _ = notification ?? throw new ArgumentNullException(nameof(notification));

        if (notification is IRemoteNotification)
        {
            await _mediator.Publish(new RemoteMediatorCommand(notification), cancellationToken);
            return;
        }

        await _mediator.Publish(notification, cancellationToken);
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new())
        where TNotification : INotification
    {
        _ = notification ?? throw new ArgumentNullException(nameof(notification));

        if (notification is IRemoteNotification)
        {
            await _mediator.Publish(new RemoteMediatorCommand(notification), cancellationToken);
            return;
        }

        await _mediator.Publish(notification, cancellationToken);
    }
}