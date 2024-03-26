namespace MediatR.Remote;

/// <summary>
/// Marker interface for remote commands.
/// </summary>
public interface IRemoteCommand
{
    IEnumerable<string>? SpanRoles { get; }
}
