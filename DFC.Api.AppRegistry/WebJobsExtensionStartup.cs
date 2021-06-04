using AutoMapper;
using AzureFunctions.Extensions.Swashbuckle;
using DFC.Api.AppRegistry;
using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Extensions;
using DFC.Api.AppRegistry.HttpClientPolicies;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Services;
using DFC.Compui.Cosmos;
using DFC.Compui.Cosmos.Contracts;
using DFC.Compui.Subscriptions.Pkg.Netstandard.Extensions;
using DFC.Compui.Subscriptions.Pkg.Webhook.Extensions;
using DFC.Swagger.Standard;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using IApiService = DFC.Api.AppRegistry.Contracts.IApiService;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "Web Jobs Extension Startup")]

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
            var cosmosRetryOptions = new RetryOptions { MaxRetryAttemptsOnThrottledRequests = 20, MaxRetryWaitTimeInSeconds = 60 };

            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAutoMapper(typeof(WebJobsExtensionStartup).Assembly);
            builder.Services.AddDocumentServices<AppRegistrationModel>(cosmosDbConnection, false, cosmosRetryOptions);
            builder.Services.AddSingleton(configuration.GetSection(nameof(UpdateScriptHashCodeClientOptions)).Get<UpdateScriptHashCodeClientOptions>() ?? new UpdateScriptHashCodeClientOptions());
            builder.Services.AddSingleton(configuration.GetSection(nameof(PagesClientOptions)).Get<PagesClientOptions>() ?? new PagesClientOptions());
            builder.Services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<ILegacyDataLoadService, LegacyDataLoadService>();
            builder.Services.AddTransient<IApiDataService, ApiDataService>();
            builder.Services.AddTransient<IApiService, ApiService>();
            builder.Services.AddTransient<IModelMappingService, ModelMappingService>();
            builder.Services.AddTransient<IModelValidationService, ModelValidationService>();
            builder.Services.AddTransient<IUpdateScriptHashCodeService, UpdateScriptHashCodeService>();
            builder.Services.AddWebhookSupport<PagesWebhookService>();
            builder.Services.AddSubscriptionService(configuration);

            builder.Services
                .AddPolicies(policyRegistry, nameof(UpdateScriptHashCodeClientOptions), policyOptions)
                .AddHttpClient<IUpdateScriptHashCodeService, UpdateScriptHashCodeService, UpdateScriptHashCodeClientOptions>(configuration, nameof(UpdateScriptHashCodeClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            builder.Services
                .AddPolicies(policyRegistry, nameof(PagesClientOptions), policyOptions)
                .AddHttpClient<IPagesDataLoadService, PagesDataLoadService, PagesClientOptions>(configuration, nameof(PagesClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));
        }
    }
}
