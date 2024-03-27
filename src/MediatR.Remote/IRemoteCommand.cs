namespace MediatR.Remote;

public interface IRemoteCommand
{
    IEnumerable<string>? SpanRoles { get; }
}