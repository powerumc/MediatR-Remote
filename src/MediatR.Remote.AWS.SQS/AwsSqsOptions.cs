// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Amazon.SQS;

namespace MediatR.Remote.AWS.SQS;

public class AwsSqsOptions
{
    public AmazonSQSClient Client { get; set; }
    public string QueueUrl { get; set; }

    public Func<IServiceProvider, RemoteMediatorCommand, string> MessageGroupIdGenerator { get; set; } =
        (_, _) => string.Empty;

    public Func<IServiceProvider, RemoteMediatorCommand, string> MessageDeduplicationIdGenerator { get; set; } =
        (_, _) => Guid.NewGuid().ToString();
}
