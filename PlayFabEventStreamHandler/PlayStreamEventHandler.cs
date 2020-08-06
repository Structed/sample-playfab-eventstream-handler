using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.AdminModels;
using PlayFab.Samples;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlayFabEventStreamHandler
{
    public static class PlayStreamEventHandler
    {
        [FunctionName("PlayStreamEventHandler")]
        public static async Task Run([QueueTrigger("login-events", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(myQueueItem);
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            PlayFabSettings.staticSettings.TitleId = context.TitleAuthenticationContext.Id;
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable("DeveloperSecretKey");

            await UpdatePlayerData(log, context);
            await CreateNews(log, context);
            await CreateItem(log, context);
        }
        
        private static async Task UpdatePlayerData(ILogger log, PlayerPlayStreamFunctionExecutionContext<dynamic> context)
        {
            var updateUserDataRequest = new PlayFab.ServerModels.UpdateUserDataRequest {
                PlayFabId = context.PlayerProfile.PlayerId,
                Data = new Dictionary<string, string> {
                    {"foo", "bar"}
                }
            };
            PlayFabResult<PlayFab.ServerModels.UpdateUserDataResult> result = await PlayFabServerAPI.UpdateUserDataAsync(updateUserDataRequest);
            HandleError(log, result);
        }

        private static void HandleError(ILogger log, PlayFabBaseResult playFabResult)
        {
            if (playFabResult.Error != null)
            {
                log.LogError($"Code: {playFabResult.Error.Error}\n{playFabResult.Error.ErrorMessage}");
            }
        }

        private static async Task CreateNews(ILogger log, PlayerPlayStreamFunctionExecutionContext<dynamic> context)
        {
            try
            {
                AddNewsRequest addNewsRequest = new AddNewsRequest
                {
                    Title = $"New Player {context.PlayerProfile.PlayerId} joined",
                    Body = $"Created: {context.PlayerProfile.Created}\nDisplay Name: {context.PlayerProfile.DisplayName}"
                };
                await PlayFabAdminAPI.AddNewsAsync(addNewsRequest);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }
        }

        private static async Task CreateItem(ILogger log, PlayerPlayStreamFunctionExecutionContext<dynamic> context)
        {
            var catalogItems = new List<CatalogItem>
            {
                new CatalogItem()
                {
                    ItemId = Guid.NewGuid().ToString(),
                    DisplayName = $"Star of {context.PlayerProfile.PlayerId}",
                    IsStackable = false,
                    IsTradable = true,
                    IsLimitedEdition = true,
                    InitialLimitedEditionCount = 1,
                    ItemClass = "PersonalUnique"
                }
            };

            try
            {
                UpdateCatalogItemsRequest updateCatalogItemsRequest = new UpdateCatalogItemsRequest()
                {
                    Catalog = catalogItems,
                    CatalogVersion = "0.1",
                };
                await PlayFabAdminAPI.UpdateCatalogItemsAsync(updateCatalogItemsRequest);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }
        }
    }
}
