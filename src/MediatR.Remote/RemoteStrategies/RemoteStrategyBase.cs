// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MediatR.Remote.RemoteStrategies;

/// <summary>
///     A remote strategy that uses a protocol to send the request to the target role.
/// </summary>
public abstract class RemoteStrategyBase : IRemoteStrategy
{
    /// <summary>
    ///     Invokes command the remote mediator.
    /// </summary>
    /// <param name="myRoleNames">My role name</param>
    /// <param name="targetRoleName">Target role name</param>
    /// <param name="nextSpans">Next role names</param>
    /// <param name="command">Remote mediator command object</param>
    /// <param name="cancellationToken">CancellationToken object</param>
    /// <returns>Invoked command result</returns>
    /// <exception cref="ArgumentNullException">If any parameters is null</exception>
    /// <exception cref="InvalidOperationException">
    ///     If
    ///     <param name="command"> is not <see cref="IRemoteRequest" /> or <see cref="IRemoteNotification" /></param>
    /// </exception>
    public async Task<RemoteMediatorResult?> InvokeAsync(IEnumerable<string> myRoleNames, string targetRoleName,
        IEnumerable<string> nextSpans, RemoteMediatorCommand command, CancellationToken cancellationToken = default)
    {
        _ = myRoleNames ?? throw new ArgumentNullException(nameof(myRoleNames));
        _ = targetRoleName ?? throw new ArgumentNullException(nameof(targetRoleName));
        _ = nextSpans ?? throw new ArgumentNullException(nameof(nextSpans));
        _ = command ?? throw new ArgumentNullException(nameof(command));

        var nextCommand = new RemoteMediatorCommand(command.Object, command.ProtocolName, nextSpans);

        switch (command.Object)
        {
            case IRemoteRequest:
                var result =
                    await SendInternalAsync(targetRoleName, nextCommand, cancellationToken);
                return result;

            case IRemoteNotification:
                await NotificationInternalAsync(targetRoleName, nextCommand, cancellationToken);
                return null;

            default:
                throw new InvalidOperationException(
                    $"MediatorRemote does only supports {nameof(IRemoteRequest)} and {nameof(IRemoteNotification)}");
        }
    }

    public IAsyncEnumerable<RemoteMediatorStreamResult?> InvokeStreamAsync(IEnumerable<string> myRoleNames,
        string targetRoleName, IEnumerable<string> nextSpans,
        RemoteMediatorStreamCommand command, CancellationToken cancellationToken = default)
    {
        _ = myRoleNames ?? throw new ArgumentNullException(nameof(myRoleNames));
        _ = targetRoleName ?? throw new ArgumentNullException(nameof(targetRoleName));
        _ = nextSpans ?? throw new ArgumentNullException(nameof(nextSpans));
        _ = command ?? throw new ArgumentNullException(nameof(command));

        var nextCommand = new RemoteMediatorCommand(command.Object, command.ProtocolName, nextSpans);

        switch (command.Object)
        {
            case IRemoteStreamRequest:
                var result = StreamInternalAsync(targetRoleName, nextCommand, cancellationToken);
                return result;

            default:
                throw new InvalidOperationException(
                    $"{nameof(InvokeStreamAsync)} is only supports {nameof(IRemoteStreamRequest)})");
        }
    }

    /// <summary>
    ///     Send the request to the target role.
    /// </summary>
    /// <param name="targetRoleName">Target role name</param>
    /// <param name="nextCommand">Command of a object and next span</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Invoke result</returns>
    protected abstract Task<RemoteMediatorResult?> SendInternalAsync(
        string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Notify the request to the target role.
    /// </summary>
    /// <param name="targetRoleName">Target role name</param>
    /// <param name="nextCommand">Command of a object and next span</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Invoke result</returns>
    protected abstract Task NotificationInternalAsync(
        string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Notify the request to the target role.
    /// </summary>
    /// <param name="targetRoleName">Target role name</param>
    /// <param name="nextCommand">Command of a object and next span</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Invoke result</returns>
    protected abstract IAsyncEnumerable<RemoteMediatorStreamResult?> StreamInternalAsync(
        string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken);
}
