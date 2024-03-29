# MediatR.Remote [[English](README.md)]

## MediatR.Remote 는 무엇입니까?

MediatR.Remote 라이브러리는 원격 개체를 분산하여 중계하는 `IMediator` 구현의 확장 라이브러리 입니다.
[Akka.NET](https://getakka.net/) 또는 [Orleans](https://github.com/dotnet/orleans) 과 같은 분산 클러스터링 프레임워크에서 메시지를 통한 분산 방법에서 영감을 받았습니다.

최근 소프트웨어 개발은 작은 규모로 시작하지만 점점 더 복잡해지고 빠른 속도로 확장해야 합니다. 
그러나 규모의 확장을 항상 염두하고 개발하지만 때론 기존 아키텍처가 새로운 아키텍처 설계에 장애물이 되기도 합니다.
`MediatR.Remote` 는 `IMediator` 의 Mediator Pattern 을 원격/분산하도록 고안된 라이브러리 입니다.

`MediatR.Remote` 는 `Role` 기반으로 각 서비스의 역할을 의미 합니다.
각 역할(Role)은 하나의 `In-Process` 서비스에 위치할 수도 있고, 물리적으로 분산될 수도 있으며, 
구조를 축소하거나 확장하는 것에 초점이 맞추어져 있습니다.

## 설치하기

```bash
dotnet add package MediatR.Remote

# ASP.NET Core
dotnet add package MediatR.Remote.Extensions.DependencyInjection
```

## 시작하기

### 1. 메시지 정의

`IRemoteRequest` 인터페이스를 메시지 클래스에 구현합니다.
또는 `IRemoteNotification`, `IRemoteStreamRequest` 인터페이스 구현이 필요합니다.

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

### 2. 서비스 구성하기

`AddRemoteMediatR()` 메서드를 통해 현재 실행하는 서비스의 역할(Role) 을 정의합니다.

만약 한 서비스에 모든 역할을 하나의 In-Process 에서 동작하려면 `builder.Services.AddRemoteMediatR(new[] {"public-api", "internal-api1", "internal-api2"}, ...);` 과 같이 서비스의 역할을 정의하면 됩니다.

```csharp
builder.Services.AddMediatR(/* ..생략.. */);
builder.Services.AddRemoteMediatR("public-api", remoteBuilder =>
{
    remoteBuilder.AddHttpStrategy("public-api", client => client.BaseAddress = new Uri("http://localhost:5000"));
    remoteBuilder.AddHttpStrategy("internal-api1", client => client.BaseAddress = new Uri("http://localhost:5010"));
    remoteBuilder.AddHttpStrategy("internal-api2", client => client.BaseAddress = new Uri("http://localhost:5020"));
});
```

### 3. Middleware 적용하기

```csharp
app.UseRouting();      // <-- Routing 미들웨어가 필수로 필요함
app.UseRemoteMediatR(mediatorApplicationBuilder => mediatorApplicationBuilder.UseHttpListener());
```

### 4. 메시지 전송

```csharp
class Test
{
    private IRemoteMediator mediator; // or private IMediator mediator;

    public Test(IRemoteMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task Call()
    {
        var request = new HelloRemoteRequest("HELLO WORLD");
        var response = await _mediator.Send(request);
    }
}
```

## More Communication Strategies

### HTTP

```csharp
// Configure Service
services.AddRemoteMediatR("public-api", remoteBuilder =>
{
    remoteBuilder.AddHttpStrategy("internal-api1", client => client.BaseAddress = new Uri("http://localhost:5010"));
});

// Configure Middleware
app.UseRemoteMediatR(routeBuilder => routeBuilder.MapHttpListener().AllowAnonymous());
```

### gRPC

실험적인 기능

```csharp
// Define Custom IMediator Interface
public interface IGrpcMediator : IRemoteMediator;

public class GrpcMediator(IMediator mediator) : RemoteMediator(mediator), IGrpcMediator
{
    public override string ProtocolName => "grpc";
}

// Configure Service
services.AddGrpc();
services.AddRemoteMediatR<IGrpcMediator, GrpcMediator>("public-api", "grpc", remoteBuilder =>
{
    remoteBuilder.AddGrpcStrategy("internal-api1", client => client.Address = new Uri("http://localhost:5011"));
});

// Configure Middleware
app.UseRemoteMediatR(applicationBuilder => applicationBuilder.UseGrpcListener());
```

## 더 많은 예제

* [메시지 통신 예제](examples)
