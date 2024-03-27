// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MediatR;
using Messages;

namespace InternalApi2.Handlers;

public class HelloQueueNotificationHandler : INotificationHandler<HelloQueueNotification>
{
    public Task Handle(HelloQueueNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(HelloQueueNotificationHandler)}: {notification.Message}");

        return Task.CompletedTask;
    }
}
