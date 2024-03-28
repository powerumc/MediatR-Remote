// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Amazon.SQS;

namespace MediatR.Remote.AWS.SQS;

/// <summary>
///     AWS SQS options
/// </summary>
public class AwsSqsOptions
{
    /// <summary>
    ///     AWS SQS Client
    /// </summary>
    public AmazonSQSClient Client { get; set; }

    /// <summary>
    ///     Queue URL
    /// </summary>
    public string QueueUrl { get; set; }

    /// <summary>
    ///     SQS Message GroupId Generator
    ///     Default is to generate an empty string.
    /// </summary>
    public Func<IServiceProvider, RemoteMediatorCommand, string> MessageGroupIdGenerator { get; set; } =
        (_, _) => string.Empty;

    /// <summary>
    ///     SQS Message DeduplicationId Generator.
    ///     Default is to generate a new Guid.
    /// </summary>
    public Func<IServiceProvider, RemoteMediatorCommand, string> MessageDeduplicationIdGenerator { get; set; } =
        (_, _) => Guid.NewGuid().ToString();

    /// <summary>
    ///     When creating a queue, this function will be called to get the attributes to set on the queue.
    ///     Default is to create a FIFO queue.
    /// </summary>
    public Func<IServiceProvider, Dictionary<string, string>> AttributesOnCreateQueue { get; set; } =
        _ => new Dictionary<string, string> { { QueueAttributeName.FifoQueue, "True" } };
}
