using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class RegionsCosmosDbTrigger
    {
        [FunctionName("RegionsChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
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
                    var changeFeedMessageModel = new ChangeFeedMessageModel()
                    {
                        Document = document,
                        IsAddress = true
                    };

                    _loggerHelper.LogInformationMessage(log, Guid.NewGuid(), string.Format("Attempting to send document id: {0} to service bus queue", document.Id));
                    await _serviceBusClient.SendChangeFeedMessageAsync(document, changeFeedMessageModel);
                }
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to send message to service bus queue", ex);
            }
        }
    }
}
}
