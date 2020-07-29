using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.AdminModels;
using PlayFab.Samples;
using System;
using System.Threading.Tasks;

namespace PlayFabEventStreamHandler
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task Run([QueueTrigger("login-events", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(myQueueItem);
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            PlayFabSettings.staticSettings.TitleId = context.TitleAuthenticationContext.Id;
            PlayFabSettings.staticSettings.DeveloperSecretKey = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

            try
            {
                AddNewsRequest addNewsRequest = new AddNewsRequest
                {
                    Title = $"New Player {context.PlayerProfile.PlayerId} joined",
                    Body = $"Created: {context.PlayerProfile.Created}\nDisplay Name: {context.PlayerProfile.DisplayName}"
                };
                await PlayFabAdminAPI.AddNewsAsync(addNewsRequest);
            } catch (Exception e)
            {
                log.LogError(e.Message);
            }
        }
    }
}
