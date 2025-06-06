// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.

using Amazon;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.AWS;

namespace Aspire.Hosting;

/// <summary>
/// Extension methods for configuring the AWS SDK for .NET
/// </summary>
public static class SDKResourceExtensions
{
    /// <summary>
    /// Add a configuration for resolving region and credentials for the AWS SDK for .NET.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> instance.</param>
    /// <returns></returns>
    public static IAWSSDKConfig AddAWSSDKConfig(this IDistributedApplicationBuilder builder)
    {
        var config = new AWSSDKConfig();

        return config;
    }

    /// <summary>
    /// Assign the AWS credential profile to the IAWSSDKConfigResource.
    /// </summary>
    /// <param name="config">An <see cref="IAWSSDKConfig"/> instance.</param>
    /// <param name="profile">The name of the AWS credential profile.</param>
    /// <returns></returns>
    public static IAWSSDKConfig WithProfile(this IAWSSDKConfig config, string profile)
    {
        config.Profile = profile;
        return config;
    }

    /// <summary>
    /// Assign the region for the IAWSSDKConfigResource.
    /// </summary>
    /// <param name="config">An <see cref="IAWSSDKConfig"/> instance.</param>
    /// <param name="region">The AWS region.</param>
    public static IAWSSDKConfig WithRegion(this IAWSSDKConfig config, RegionEndpoint region)
    {
        config.Region = region;
        return config;
    }

    /// <summary>
    /// Set validation preference for profile in sdk.
    /// </summary>
    /// <param name="config">An <see cref="IAWSSDKConfig"/> instance.</param>
    /// <param name="sdkValidationEnabled">option for toggle sdk validation</param>
    public static IAWSSDKConfig WithSdkValidation(this IAWSSDKConfig config, bool sdkValidationEnabled)
    {
        config.SDKValidationEnabled = sdkValidationEnabled;

        return config;
    }

    /// <summary>
    /// Add a reference to an AWS SDK configuration to the resource.
    /// </summary>
    /// <param name="builder">An <see cref="IResourceBuilder{T}"/> for <see cref="IResourceWithEnvironment"/></param>
    /// <param name="awsSdkConfig">The AWS SDK configuration</param>
    /// <returns></returns>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IAWSSDKConfig awsSdkConfig)
        where TDestination : IResourceWithEnvironment
    {
        builder.WithAnnotation(new SDKResourceAnnotation(awsSdkConfig));

        builder.WithEnvironment(context =>
        {
            if (context.ExecutionContext.IsPublishMode)
            {
                return;
            }

            SdkUtilities.ApplySDKConfig(context, awsSdkConfig, true);
        });

        return builder;
    }
}
