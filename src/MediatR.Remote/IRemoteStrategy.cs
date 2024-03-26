namespace MediatR.Remote;

public interface IRemoteStrategy
{
    Task<RemoteMediatorResult?> InvokeAsync(
        IEnumerable<string> myRoleNames,
        string targetRoleName,
        IEnumerable<string> nextSpans,
        RemoteMediatorCommand command,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<RemoteMediatorStreamResult?> InvokeStreamAsync(IEnumerable<string> myRoleNames,
        string targetRoleName,
        IEnumerable<string> nextSpans,
        RemoteMediatorStreamCommand command,
        CancellationToken cancellationToken = default);
}
