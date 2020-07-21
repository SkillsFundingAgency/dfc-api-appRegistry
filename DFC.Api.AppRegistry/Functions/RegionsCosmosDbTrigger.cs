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
    public class RegionsCosmosDbTrigger
    {
        private const string DatabaseName = "%LegacyDatabaseName%";
        private const string CollectionName = "%RegionsCollectionName%";
        private const string LeaseCollectionName = "%RegionsLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%RegionsLeaseCollectionNamePrefix%";

        private readonly ILegacyDataLoadService legacyDataLoadService;
        private readonly ILogger<RegionsCosmosDbTrigger> triggerLogger;

        public RegionsCosmosDbTrigger(ILogger<RegionsCosmosDbTrigger> logger, ILegacyDataLoadService legacyDataLoadService)
        {
            this.triggerLogger = logger;
            this.legacyDataLoadService = legacyDataLoadService;
        }

        [FunctionName("RegionsChangeFeedTrigger")]
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
                triggerLogger.LogInformation($"RegionsChangeFeedTrigger executed with {(documents != null ? documents.Count : 0)} documents");

                if (documents != null)
                {
                    foreach (var document in documents)
                    {
                        triggerLogger.LogInformation($"Updating Document with Id: {document.Id}");

                        LegacyRegionModel legacyRegionModel = (dynamic)document;
                        await legacyDataLoadService.UpdateRegionAsync(legacyRegionModel).ConfigureAwait(false);

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