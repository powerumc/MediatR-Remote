namespace MediatR.Remote;

/// <summary>
///     Marker interface for remote mediators. You can inject these into your handlers or services.
/// </summary>
public interface IRemoteMediator : IMediator
{
    /// <summary>
    ///     Protocol name for the remote mediator.
    /// </summary>
    string ProtocolName { get; }
}
