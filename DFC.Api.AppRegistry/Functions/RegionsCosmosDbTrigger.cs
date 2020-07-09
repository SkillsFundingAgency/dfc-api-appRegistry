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
        private const string DatabaseName = "composition";
        private const string CollectionName = "regions";
        private const string ConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string LeaseCollectionName = "regions_lease";
        private const string LeaseCollectionPrefix = "";

        [FunctionName("RegionsChangeFeedTrigger")]
        public async Task Run(
           [CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            ConnectionStringSetting = ConnectionString,
            LeaseCollectionName = LeaseCollectionName,
            LeaseCollectionPrefix = LeaseCollectionPrefix,
            CreateLeaseCollectionIfNotExists = true
            )] IReadOnlyList<Document> documents,
           ILogger<RegionsCosmosDbTrigger> log)
        {
            try
            {
                foreach (var document in documents)
                {
                    Console.WriteLine(document);
                }
            }
            catch (Exception ex)
            {
               // _loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to send message to service bus queue", ex);
            }
        }
    }
}