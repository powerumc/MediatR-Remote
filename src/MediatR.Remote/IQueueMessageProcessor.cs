// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MediatR.Remote;

/// <summary>
///     SQS message processor.
/// </summary>
public interface IQueueMessageProcessor<in TOptions, in TMessage>
{
    /// <summary>
    ///     Create queue if not exists.
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task CreateQueueIfNotExistsAsync(string roleName, CancellationToken cancellationToken);

    /// <summary>
    ///     Acknowledge message.
    ///     If message is processed successfully, acknowledge the message.(delete message)
    /// </summary>
    /// <param name="options">Options</param>
    /// <param name="message">Message</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task AcknowledgeMessageAsync(TOptions options, TMessage message, CancellationToken cancellationToken);

    /// <summary>
    ///     Message processing.
    /// </summary>
    /// <param name="command">Command</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task OnMessageAsync(RemoteMediatorCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Message processing exception.
    /// </summary>
    /// <param name="options">Options</param>
    /// <param name="message">Message</param>
    /// <param name="exception">Exception</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task OnMessageExceptionAsync(TOptions options, TMessage message, Exception exception,
        CancellationToken cancellationToken);
}
