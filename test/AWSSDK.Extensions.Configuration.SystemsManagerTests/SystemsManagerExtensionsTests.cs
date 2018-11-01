﻿using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace AWSSDK.Extensions.Configuration.SystemsManagerTests
{
    public class SystemsManagerExtensionsTests
    {
        [Theory]
        [MemberData(nameof(SourceExtensionData))]
        [MemberData(nameof(WithAWSOptionsExtensionData))]
        [MemberData(nameof(NoAWSOptionsExtensionData))]
        public void AddSystemsManagerInlineTest(Func<IConfigurationBuilder, IConfigurationBuilder> configurationBuilder, Type exceptionType, string exceptionMessage)
        {
            var builder = new ConfigurationBuilder();

            IConfigurationBuilder ExecuteBuilder() => configurationBuilder(builder);

            if (exceptionType != null)
            {
                var ex = Assert.Throws(exceptionType, ExecuteBuilder);
                Assert.Contains(exceptionMessage, ex.Message, StringComparison.Ordinal);
            }
            else
            {
                var result = ExecuteBuilder();
                Assert.Equal(builder, result);
            }
        }

        public static TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string> SourceExtensionData => new TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string>
        {
            {builder => builder.AddSystemsManager(CreateSource(null, null, false, null, null)), typeof(ArgumentNullException), "Parameter name: Path"},
            {builder => builder.AddSystemsManager(CreateSource(null, null, true, null, null)), typeof(ArgumentNullException), "Parameter name: Path"},
            {builder => builder.AddSystemsManager(CreateSource("/path", null, false, null, null)), null, null},
            {builder => builder.AddSystemsManager(CreateSource("/aws/reference/secretsmanager/somevalue", null, false, null, null)), typeof(ArgumentException), "Secrets Manager paths are not supported"}
        };

        public static TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string> WithAWSOptionsExtensionData => new TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string>
        {
            {builder => builder.AddSystemsManager(null, null), typeof(ArgumentNullException), "Parameter name: path"},
            {builder => builder.AddSystemsManager("/path", null), typeof(ArgumentNullException), "Parameter name: awsOptions"},
            {builder => builder.AddSystemsManager(null, new AWSOptions()), typeof(ArgumentNullException), "Parameter name: path"},
            {builder => builder.AddSystemsManager("/aws/reference/secretsmanager/somevalue", new AWSOptions()), typeof(ArgumentException), "Secrets Manager paths are not supported"},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), true), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), false), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), true, TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", new AWSOptions(), false, TimeSpan.Zero), null, null}
        };

        public static TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string> NoAWSOptionsExtensionData => new TheoryData<Func<IConfigurationBuilder, IConfigurationBuilder>, Type, string>
        {
            {builder => builder.AddSystemsManager(null as string), typeof(ArgumentNullException), "Parameter name: path"},
            {builder => builder.AddSystemsManager("/path"), null, null},
            {builder => builder.AddSystemsManager("/aws/reference/secretsmanager/somevalue"), typeof(ArgumentException), "Secrets Manager paths are not supported"},
            {builder => builder.AddSystemsManager("/path", true), null, null},
            {builder => builder.AddSystemsManager("/path", false), null, null},
            {builder => builder.AddSystemsManager("/path", TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", true, TimeSpan.Zero), null, null},
            {builder => builder.AddSystemsManager("/path", false, TimeSpan.Zero), null, null}
        };

        private static Action<SystemsManagerConfigurationSource> CreateSource(string path, AWSOptions awsOptions, bool optional, TimeSpan? reloadAfter, Action<SystemsManagerExceptionContext> onLoadException)
        {
            return source =>
            {
                source.Path = path;
                source.AwsOptions = awsOptions;
                source.Optional = optional;
                source.ReloadAfter = reloadAfter;
                source.OnLoadException = onLoadException;
            };
        }
    }
}
