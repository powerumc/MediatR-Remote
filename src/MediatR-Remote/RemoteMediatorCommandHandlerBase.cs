namespace MediatR.Remote;

internal class RemoteMediatorCommandHandlerBase
{
    protected (string[] nextSpans, string? targetRoleName) GetNextSpans(
        IRemoteCommand remoteCommand, IEnumerable<string>? requestSpans, IEnumerable<string> myRoleNames)
    {
        var roles = remoteCommand.SpanRoles ?? Array.Empty<string>();
        var spans = requestSpans?.ToArray() ?? Array.Empty<string>();
        var nextSpans = spans.Concat(myRoleNames).ToArray();
        var excepted = roles.Except(nextSpans);
        var targetRoleName = excepted.FirstOrDefault();

        return (nextSpans, targetRoleName);
    }
}