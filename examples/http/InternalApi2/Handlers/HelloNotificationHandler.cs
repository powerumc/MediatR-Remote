using MediatR;
using Messages;

namespace InternalApi2.Handlers;

public class HelloNotificationHandler : INotificationHandler<HelloRemoteNotification>
{
    public Task Handle(HelloRemoteNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine(notification.Text);

        return Task.CompletedTask;
    }
}