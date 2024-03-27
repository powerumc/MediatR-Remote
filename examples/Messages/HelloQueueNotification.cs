// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MediatR;
using MediatR.Remote;

namespace Messages;

public class HelloQueueNotification(string message) : INotification, IRemoteNotification
{
    public string Message { get; } = message;

    public IEnumerable<string>? SpanRoles => ["internal-api2"];
}
