using AutoMapper;
using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.Legacy;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class RegionsCosmosDbTrigger
    {
        private const string DatabaseName = "composition";
        private const string CollectionName = "%RegionsCollectionName%";
        private const string LeaseCollectionName = "regions_lease";
        private const string LeaseCollectionPrefix = "";

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
            CreateLeaseCollectionIfNotExists = true
            )] IReadOnlyList<Document> documents)
        {
            try
            {
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