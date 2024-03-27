using MediatR;
using MediatR.Remote;

namespace Messages;

public class HelloRemoteNotification : INotification, IRemoteNotification
{
    public HelloRemoteNotification(string text)
    {
        Text = text;
    }

    public string Text { get; }

    public IEnumerable<string> SpanRoles => new[] { "internal-api1", "internal-api2" };
}