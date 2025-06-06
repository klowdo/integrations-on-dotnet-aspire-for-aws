// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.

using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.AWS.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting.AWS.Provisioning;

internal sealed class CloudFormationStackResourceProvisioner(
    ResourceLoggerService loggerService,
    ResourceNotificationService notificationService)
    : CloudFormationResourceProvisioner<CloudFormationStackResource>(loggerService, notificationService)
{
    protected override async Task GetOrCreateResourceAsync(CloudFormationStackResource resource, CancellationToken cancellationToken)
    {
        var logger = LoggerService.GetLogger(resource);
        try
        {
            using var cfClient = GetCloudFormationClient(resource);

            var request = new DescribeStacksRequest { StackName = resource.StackName };
            var response = await cfClient.DescribeStacksAsync(request, cancellationToken).ConfigureAwait(false);

            // If the stack didn't exist then a StackNotFoundException would have been thrown.
            var stack = response.Stacks[0];

            // Capture the CloudFormation stack output parameters on to the Aspire CloudFormation resource. This
            // allows projects that have a reference to the stack have the output parameters applied to the
            // projects IConfiguration.
            resource.Outputs = stack!.Outputs ?? new List<Output>();
            logger.LogInformation("CloudFormation stack has {Count} output parameters", resource.Outputs.Count);
            if (logger.IsEnabled(LogLevel.Information))
            {
                foreach (var output in resource.Outputs)
                {
                    logger.LogInformation("Output Name: {Name}, Value {Value}", output.OutputKey, output.OutputValue);
                }
            }

            await PublishCloudFormationUpdatePropertiesAsync(resource, ConvertOutputToProperties(stack)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            if (e is AmazonCloudFormationException ce && string.Equals(ce.ErrorCode, "ValidationError"))
            {
                logger.LogError("Stack {StackName} does not exists to add as a resource", resource.Name);
            }
            else
            {
                logger.LogError(e, "Error reading {StackName}", resource.Name);
            }
            throw;
        }
    }
}
