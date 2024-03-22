## Run

```bash
dotnet run --project PublicApi
dotnet run --project InternalApi1
dotnet run --project InternalApi2
```

## Tests

### Test `IRequest`

```bash
curl http://localhost:5000/test/request

# Response
{"message":"OK - HELLO REQUEST"}
```

### Test `INotification`
```bash
curl http://localhost:5000/test/notification

# No Response
# Print on Internal2 console
HELLO NOTIFICATION
```

## Test `IStreamRequest`

It works like an `IAsyncEnumerable<T>`.
```bash
curl -N http://localhost:5000/test/stream

[{"message":"OK - HELLO STREAM 0"},{"message":"OK - HELLO STREAM 1"},{"message":"OK - HELLO STREAM 2"},{"message":"OK - HELLO STREAM 3"},{"message":"OK - HELLO STREAM 4"},{"message":"OK - HELLO STREAM 5"},{"message":"OK - HELLO STREAM 6"},{"message":"OK - HELLO STREAM 7"},{"message":"OK - HELLO STREAM 8"},{"message":"OK - HELLO STREAM 9"}]
```
![Stream Response](../../assets/examples/http/stream-response.gif)