# MediatR.Remote [[한국어](README.ko.md)]

## What is MediatR.Remote?

The MediatR.Remote library is an extension library of the `IMediatR` implementation that distributes and relays remote objects.
Inspired by a distributed environment via messages from distributed clustering frameworks such as [Akka.NET](https://getakka.net/) or [Orleans](https://github.com/dotnet/orleans).

Modern software development starts small, but it becomes increasingly complex and must scale at a rapid pace.
However, although we always develop with scale expansion in mind, sometimes existing architectures become obstacles in designing new architectures.
`MediatR.Remote` is a library designed to remote/distribute Mediator Pattern of `IMediator`.

`MediatR.Remote` means the role of each service based on `Role`.
Each role may be located in one ‘In-Process’ service, or it may be physically distributed,
The focus is on shrinking or expanding the structure.

## Install

```bash
dotnet add package MediatR.Remote
```

## Getting Started

1. **Message Definition**

Implement the `IRemoteRequest` interface in your message class.
or `IRemoteNotification`, which requires an implementation of the `IRemoteStreamRequest` interface.

```csharp
public class HelloRemoteRequest : IRequest<HelloResponse>, IRemoteRequest
{
    public HelloRemoteRequest(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public IEnumerable<string> SpanRoles => new[] { "internal-api1", "internal-api2" };
}

public class HelloResponse
{
    public HelloResponse(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
```

2. **Configuring Services**

Define the role of the currently running service through the `AddRemoteMediatR()` method.

If you want to run all roles in one service in one In-Process, `builder.Services.AddRemoteMediatR(new[] {"public-api", "internal-api1", "internal-api2"}, ...`) ; You can define the role of the service like this:

```csharp
builder.Services.AddMediatR(/* ..omitted.. */);
builder.Services.AddRemoteMediatR("public-api", remoteBuilder =>
{
    remoteBuilder.AddHttpStrategy("public-api", client => client.BaseAddress = new Uri("http://localhost:5000"));
    remoteBuilder.AddHttpStrategy("internal-api1", client => client.BaseAddress = new Uri("http://localhost:5010"));
    remoteBuilder.AddHttpStrategy("internal-api2", client => client.BaseAddress = new Uri("http://localhost:5020"));
});
```

3. **Applying Middleware**

```csharp
app.UseRouting(); // <-- Routing middleware is required
app.UseMediatRemote();
```

## Examples

* [HTTP Message Communication Example](examples/http)