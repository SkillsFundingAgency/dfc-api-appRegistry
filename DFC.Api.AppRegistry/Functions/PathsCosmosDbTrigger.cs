using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.Legacy;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class PathsCosmosDbTrigger
    {
        private const string DatabaseName = "%LegacyDatabaseName%";
        private const string CollectionName = "%RegionsCollectionName%";
        private const string LeaseCollectionName = "%PathsLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%PathsLeaseCollectionNamePrefix%";

        private readonly ILegacyDataLoadService legacyDataLoadService;
        private readonly ILogger<PathsCosmosDbTrigger> triggerLogger;

        public PathsCosmosDbTrigger(ILogger<PathsCosmosDbTrigger> logger, ILegacyDataLoadService legacyDataLoadService)
        {
            this.triggerLogger = logger;
            this.legacyDataLoadService = legacyDataLoadService;
        }

        [FunctionName("PathsChangeFeedTrigger")]
        public async Task Run(
           [CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            ConnectionStringSetting = "LegacyCosmosDbConnectionString",
            LeaseCollectionName = LeaseCollectionName,
            LeaseCollectionPrefix = LeaseCollectionPrefix,
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> documents)
        {
            try
            {
                if (documents != null)
                {
                    foreach (var document in documents)
                    {
                        triggerLogger.LogInformation($"Updating Document with Id: {document.Id}");

                        LegacyPathModel legacyPathModel = (dynamic)document;
                        await legacyDataLoadService.UpdatePathAsync(legacyPathModel).ConfigureAwait(false);

                        triggerLogger.LogInformation($"Updated Document with Id: {document.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                triggerLogger.LogError(ex.ToString());
                throw;
            }
        }
    }
}