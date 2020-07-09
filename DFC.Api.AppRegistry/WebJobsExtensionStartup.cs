using AutoMapper;
using AzureFunctions.Extensions.Swashbuckle;
using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Extensions;
using DFC.Api.AppRegistry.HttpClientPolicies;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Services;
using DFC.Compui.Cosmos;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[assembly: FunctionsStartup(typeof(DFC.Api.AppRegistry.WebJobsExtensionStartup))]

namespace DFC.Api.AppRegistry
{
    [ExcludeFromCodeCoverage]
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        private const string CosmosDbConfigAppSettings = "Configuration:CosmosDbConnections:AppRegistry";
        private const string AppSettingsPolicies = "Policies";

        public void Configure(IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddHttpClient();

            var cosmosDbConnection = configuration.GetSection(CosmosDbConfigAppSettings).Get<CosmosDbConnection>();
            var policyOptions = configuration.GetSection(AppSettingsPolicies).Get<PolicyOptions>() ?? new PolicyOptions();
            var policyRegistry = builder.Services.AddPolicyRegistry();

            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddAutoMapper(typeof(WebJobsExtensionStartup).Assembly);
            builder.Services.AddDocumentServices<AppRegistrationModel>(cosmosDbConnection, false);
            builder.Services.AddSingleton(configuration.GetSection(nameof(PathClientOptions)).Get<PathClientOptions>() ?? new PathClientOptions());
            builder.Services.AddSingleton(configuration.GetSection(nameof(RegionClientOptions)).Get<RegionClientOptions>() ?? new RegionClientOptions());
            builder.Services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<ILegacyDataLoadService, LegacyDataLoadService>();
            builder.Services.AddTransient<IApiDataService, ApiDataService>();
            builder.Services.AddTransient<IApiService, ApiService>();
            builder.Services.AddTransient<IModelMappingService, ModelMappingService>();
            builder.Services.AddTransient<IModelValidationService, ModelValidationService>();

            builder.Services
                .AddPolicies(policyRegistry, nameof(PathClientOptions), policyOptions)
                .AddHttpClient<ILegacyPathService, LegacyPathService, PathClientOptions>(configuration, nameof(PathClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            builder.Services
                .AddPolicies(policyRegistry, nameof(RegionClientOptions), policyOptions)
                .AddHttpClient<ILegacyRegionService, LegacyRegionService, RegionClientOptions>(configuration, nameof(RegionClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));
        }
    }
}
