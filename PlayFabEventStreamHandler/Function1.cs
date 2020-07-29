using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace PlayFabEventStreamHandler
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([QueueTrigger("login-events", Connection = "EventStreamHandlerQueueTrigger")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
