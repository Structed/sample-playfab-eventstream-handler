﻿# Getting Started with the PlayFab SDK
> by Johannes Ebner – Technical Specialist, Global Black Belt Gaming Team


# Introduction
Azure PlayFab is a really great product to help you build out your backend without creating all the systems like Item Catalogs, Shops etc. all by yourself. But PlayFab really evolves all the time with the **feedback you provide**! With such a broad product and Microsoft’s commitment of not breaking anything for you while products evolve, some complexities inevitably arise.

Evolving over time means, that you create new best practices, new concepts. For example, one of the newer additions in terms of APIs is the [Entity Programming Model](https://docs.microsoft.com/en-us/gaming/playfab/features/data/entities/), which is different to what most developers would characterize as a “traditional” web API. But it goes very much hand in hand with modern game engines like Unreal and Unity, which have an [Entity-Component-Systems (ECS)](https://en.wikipedia.org/wiki/Entity_component_system) at their heart.

Based on PlayFab’s desire to reach as many developers as possible and cope with the maintenance work required by steadily evolving APIs, PlayFab has opted to not hand-craft SDKSs, but auto-generate SDK for you to use in your projects.
However, an auto-generated SDK like PlayFab’s might at times appear a bit harder to use than your usual-hand crafted library.

## Goal
To help you with these complexities, this blog post shall give you a conceptual overview from a developer’s perspective and give a full end-to-end example of how a serverless backend in plain C# (not Unity, as the official examples) could look like.

We will be using C# & .NET Core to create an Azure Function which is getting called every time a new Player registers.

This Azure Function will then add an Item to this new Player’s Inventory.

## Prerequisites

### Editor/IDE
While you can follow along with basically any Editor/IDE, I will be using [Visual Studio 2019](https://visualstudio.microsoft.com/) (you may use the the free Visual Studio 2019 Community Edition!) because it has all the batteries included for our tasks ahead. Other IDEs/Editors might require the [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools).

### Azure Access
If you do not yet have used Azure, you can sign up for free and get some additional goodies with [Visual Studio Dev Essentials](https://visualstudio.microsoft.com/dev-essentials/) program.

Or you can just [sign up for free](https://azure.microsoft.com/en-us/free/gaming/).

### Azure PlayFab access
And, of course, since this is a Tutorial on how to use [Azure PlayFab](https://playfab.com/), you need an existing Title on PlayFab as well. You could use the same Microsoft Account you used for signing up to Azure.

# Many APIs, one SDK
Azure PlayFab has added many features over time and has evolved APIs to best cater for these features, or for new best-practices based on experience with customers.
As of today, Azure PlayFab’s APIs follow three rather different paradigms:

## Paradigm one: “Perspectives API”
(Perspectives API is a term I made up 😉)

The first paradigm takes a different perspective than most API’s I have encountered: the perspective of the user of the API – like a Player, an Admin etc. This means, you will find the same functionality duplicated or slightly changed under a different API. These end-user-perspective APIs are:

* AdminAPI
* ClientAPI
* ServerAPI

An example of this duplication is, for instance, the `ConsumeItem` request. It exists in both Client and Server APIs, does exactly the same, and uses the same request body and headers: except the authentication headers.
The Client variant wants a `SessionTicket`, while the Server variant wants a `SecretKey`.
Let’s look at these three API perspectives for a moment:

### Client
As the name implies, this is the “client side”, meaning it should be called within the context of a specific user, a player. Requires an authenticated Session.

### Server
This is a very broad API, which basically covers all operations which are supposed to be called from the server-side (or an Azure Function, if you do not want to implement a full server application)

### Admin
The Admin API is somewhat in the middle between Client and Server. The operations provided by this API is what you would typically see in an Admin Panel for a game.

## Paradigm Two: “Services API”
The second is the more common separation: an API per system, e.g. localization, groups, insights.

## Paradigm Three: “Entities API”
And lastly, there is the “Entity Programming Model”, which allows you to “attach” or “read” objects from specific Entity Types, which map to built-in functionality in Azure PlayFab, like a Player.

# Getting Started
As there are so many languages and platforms supported by Azure PlayFab, I had to decide for one and chose the language and platform I am best in: Pure C# & .NET Core. But there are great guides for various Game Engines like Unity and Unreal, or all the languages SDKs for Azure PlayFab are available.

## Project Setup
### Create an Azure Functions Project
To create an Azure Functions Project, either use the splash screen (see screenshot below) or use File --> New Project…

![Visual Studio - Splash Screen](images/VisualStudio-NewProject-1.png)

Select the project type “Azure Functions” and click “Next”

![Visual Studio - Create new Azure Functions Project](images/VisualStudio-NewProject-2.png)

Choose a name for your Project and select the folder for it to be placed in:

![Visual Studio - Fill out project details](images/VisualStudio-NewProject-3.png)

This will create a folder named according to your Solution name, in which it will create another folder for the project, with the Project’s name you chose.

Now, in the next dialog, you may either choose an `HttpTrigger` or a `QueueTrigger` for your Function.

> Please be aware that Azure PlayFab terminates PlayStream-invoked Functions after 1 second, thus QueueTrigger is the better choice here because it will immediately yield back to Azure PlayFab.

For the Storage Account drop-down, you may use Storage Emulator or click “Browse…” to select a storage account. As we will need to test it with Azure PlayFab anyways, let’s go ahead and use an Azure Storage Account.

![Select Functions Trigger](images/VisualStudio-NewProject-4.png)

> You can even create a Storage Account on the fly using the Dialog!

Once you are done setting up the Storage Account configuration, click the “Create” button and wait for the project to generate. Visual Studio will eventually open the project and show you the code of the generated Azure Function stub.

![View of scaffold code in Visual Studio](images/VisualStudio-NewProject-5.png)

While asking for a Connection string setting in the dialog, Visual Studio only adds this setting name as the Connection property for the `QueueTrigger`, but does not add it to the `local.settings.json`. This should usually be the connection string that appears as `AzureWebJobsStorage` in the `local.settings.json` file:

![Connection Strings](images/VisualStudio-local.settings.json.png)

### Add the Azure PlayFab package
The Azure PlayFab SDK for C# is available via NuGet. You can either add the latest version via the dotnet CLI

    dotnet add package PlayFabAllSDK

Or via the NuGet package manager in Visual Studio:
Right-click “Dependencies” and click “Manage NuGetPackages”

![Context menu right-click Solution Explorer](images/VisualStudio-ManageNugetPackagesContextMenu.png)

![NuGet browser: Install PlayFab SDK](images/VisualStudio-NuGetPackageManager.png)

Open the “Browse” tab and search for PlayFab. Select the `PlayFabAllSDK` and click “Install” on the details pane to install. You might need to accept licenses when you do.

> Please note: you should also update the installed packages because the Visual Studio template ships with a slightly older version of the SDKs for Azure Functions and WebJobs Storage.

### Test the Function
In Visual Studio, press F5 (or click Build --> Debug) to launch the local Azure Functions runtime in debug mode.

Now, let’s create a test message. To do this, go to the Azure Portal and open up your Storage Account’s Queue blade:

![Azure Storage Account - Queues overview](images/AzurePortal-StorageAccount-Queue.png)

If you haven’t already created a Queue here, do so now by clicking the “+ Queue” button. Make sure to use the same name you specified when creating the Azure Function with the Queue Trigger (see screenshot below).

![Code - Name of subscribed queue](images/VisualStudio-QueueTriggerAttribute.png)

Once you have created the Queue, click the Queue entry in the Portal to get to the detail Blade.
Here, click “+ Add message”. In the new Dialog, just add some text and click “OK”.

![Storage Queue: Add new message](images/AzurePortal-StorageAccount-AddQueue.png)

Now watch the console window of your function: it will log out how it processes the queue message you just added!

![Terminal shows running Function](images/Termina-FunctionFirstRun.png)

Checking back in the Azure Portal’s view of the Queue, you click the “Refresh” button to see how the message disappeared, because it got processed.

# Set up PlayFab
To call your Azure Function from Azure PlayFab, we need to register the Function binding via the Azure Storage Queue and set up an Event Trigger (aka *“Rule”*).

## Register the Function
Log in to [Azure PlayFab](https://playfab.com/) and click on the Title you want to register the Azure Function for.

Once you are in the Title’s overview, click “Automation” (1), which opens the “Cloud Script” tab. In this tab, select the sub-tab “Functions (Preview)” (2), which will list all the previously registered Azure Functions. Now click the big orange button in the top right labeled “REGISTER FUNCTION” (3) to open the registration dialog.

![PlayFab: Functions listing](images/PlayFab-RegisterFunction-1.png)

The Dialog is (sadly) not extremely informative, so let’s go through it.

![PlayFab: Registering an Azure Function with QueueTrigger](images/PlayFab-RegisterFunction-2.png)

* Trigger type: set to “Queue”
* Function name: This is not actually the name of the Azure Function you deployed, but the name of the binding we’re currently setting up – so it’s the identifier you will be working with from PlayFab. Choose a name that fits your needs!
* Queue name: this is the queue name the `QueueTrigger` of the Azure Function listens to. In the examples above, this was `login-events`.
* Connection string: The ConnectionString to the Azure Storage Account Queue.

To finish registering the Function registration, click *“REGISTER FUNCTION”*. The Registration should now appear in the overview.

## Set up a Rule
To set up a Rule, within a PlayFab Title, go to *“Automation”* (1) in the left pane, select the *“Rules”* Tab (2) and click *“NEW RULE”* (3).

![PlayFab: Adding a Rule](images/PlayFab-AddRule-1.png)

A Rule is a configuration that listens on a *PlayStream* event emitted by Azure PlayFab. When such an event occurs, the Rule checks whether the configured Conditions are met and if they are, triggers the configured *Action*.

In our case, we want no conditions, but want to “Execute Azure Function”. In fact, what it does, is calling its own internal binding we just set up and provides it the PlayStream event data and additional arguments you may specify here (in JSON format) as argument to that binding.

When that binding is called, it will either call the Function via HTTP trigger, or, as we configured it, by writing the data to the storage queue for the Azure Function to pick up via a `QueueTrigger`.

![PlayFab: Details of adding a Rule](images/PlayFab-AddRule-2.png)

Click “*SAVE ACTION*” to save (yes, this saves the Rule 😉). You should now see the Rule in the overview.

## Trigger the PlayFab Rule
Now we want to test the Rule and trigger the Azure Function via an Azure PlayFab PlayStream Event. To do this, we need to raise an event of type `com.playfab.player_logged_in`, as we specified when we created the Rule.

To achieve this, let’s create a small CLI application to log a Player in.

In Visual Studio, create a new .NET Core Application (preferably in the same Solution as the Function app) and then install the `PlayFabAllSDK`, just like above.

Now, you can paste the following C# code to your `Program.cs` to have a working program that signs into your game and if the Player does not yet exist, it will create a new one.

> Well, at least it is almost working. You need to substitute XXXX with your own Title ID. You can get the Title ID from your [Studio’s Overview Page](https://developer.playfab.com/en-US/my-games).

Below code listing as [GitHub Gist](https://gist.github.com/Structed/ff3190248833f40f611f0941f33793b9).

    using PlayFab;
    using PlayFab.ClientModels;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    namespace GameCli
    {
        class Program
        {
            private static bool running = true;
            static void Main(string[] args)
            {
                PlayFabSettings.staticSettings.TitleId = "XXXXX"; // Please change this value to your own titleId from PlayFab Game Manager
    
                var request = new LoginWithCustomIDRequest { CustomId = "Player-" + Guid.NewGuid(), CreateAccount = true };
                var loginTask = PlayFabClientAPI.LoginWithCustomIDAsync(request);
    
                while (running)
                {
                    if (loginTask.IsCompleted) // You would probably want a more sophisticated way of tracking pending async API calls in a real game
                    {
                        OnLoginComplete(loginTask);
                    }
    
                    // Presumably this would be your main game loop, doing other things
                    Thread.Sleep(1);
                }
    
                Console.WriteLine($"Done! Created new player named \"{request.CustomId}\". Press any key to close");
                Console.ReadKey(); // This halts the program and waits for the user
            }
    
            private static void OnLoginComplete(Task<PlayFabResult<LoginResult>> taskResult)
            {
                var apiError = taskResult.Result.Error;
                var apiResult = taskResult.Result.Result;
    
                if (apiError != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red; // Make the error more visible
                    Console.WriteLine("Something went wrong with your first API call.  :(");
                    Console.WriteLine("Here's some debug information:");
                    Console.WriteLine(PlayFabUtil.GenerateErrorReport(apiError));
                    Console.ForegroundColor = ConsoleColor.Gray; // Reset to normal color
                }
                else if (apiResult != null)
                {
                    Console.WriteLine($"Was newly created? {apiResult.NewlyCreated}");
                    Console.WriteLine("Congratulations, you made your first successful API call!");
                }
    
                running = false; // Because this is just an example, successful login triggers the end of the program
            }
        }
    }

Now execute the program and watch how the event is executed in PlayFab via the PlayStream Monitor!

![The function was executed!](images/PlayFab-PlayStream-ExecutedFunction.png)


# Backend implementation
It's time to start the actual backend implementation!

## Using the PlayFab Context
Before we even get started with writing backend code, you will need a [Helper classes file](https://github.com/PlayFab/PlayFab-Samples/blob/master/Samples/CSharp/AzureFunctions/CS2AFHelperClasses.cs), provided by PlayFab. [Download the file](https://github.com/PlayFab/PlayFab-Samples/blob/master/Samples/CSharp/AzureFunctions/CS2AFHelperClasses.cs) and save it to your project. Here is why:

Every time a Function is triggered, The `Context` in which it was executed is sent as a parameter, The Context includes the following when the Function is triggered via a PlayStream Event:

* The Entity Profile
* The PlayStream event which triggered the script.
* A boolean that indicates whether a PlayStream event is sent as part of the function being executed
* The Payload data you specified when setting up the binding in PlayFab

> For more information on the Context and what it contains in other environments, please see the [Using CloudScript context models Tutorial](https://docs.microsoft.com/en-us/gaming/playfab/features/automation/cloudscript-af/cloudscript-af-context).

### Parsing the Context
We already receive the Context in the Function we have created, it is the `myQueueItem` parameter:

        [FunctionName("Function1")]
        public static void Run([QueueTrigger("login-events", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }

However, it is just a JSON string, and we want it to be parsed to a C# POCO ("Plain Old C# Object").

To do this, we first need the Class to convert to. The easiest way to do this is to download the [Helper classes file](https://github.com/PlayFab/PlayFab-Samples/blob/master/Samples/CSharp/AzureFunctions/CS2AFHelperClasses.cs), provided by PlayFab. It will contain all the Context Objects you need for the various deserialization use-cases.

Now, we can actually deserialize the Context to `PlayerPlayStreamFunctionExecutionContext`:

        [FunctionName("Function1")]
        public static void Run([QueueTrigger("login-events", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(myQueueItem);
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }

## Authentication
Authentication in Azure PlayFab might seem a little bit overwhelming at first, but it enables high flexibility and best operational security by allowing you to only make requests in the relevant context.

For server-side actions, like we want to do here, this is particularly simple: we only need to set the `TitleId` and the `DeveloperSecretKey` on `PlayFabSettings.staticSettings`. Let's do that!

### Retrieving the `TitleId`
This one is the simplest - you can just get it from the context we have deserialized:

    PlayFabSettings.staticSettings.TitleId = context.TitleAuthenticationContext.Id;

### Retrieving the `DeveloperSecretKey`
The DeveloperSecretKey must be retrieved from the [Azure PlayFab Portal](https://developer.playfab.com/).

Log in and go to your Title you are working on.
Then, click the little cogwheel beneath your Title's name (1) and select "Title Settings" (2). Last, select the "Secret Keys" Tab (3)

![PlayFab - Add Secret Key (DeveloperSecretKey)](images/PlayFab-AddSecretKey.png)

On This page, go ahead and click "NEW SECRET KEY". Specify a name and click "SAVE SECRET KEY".

Now back on the Secret Key Overview, copy the secret key.

### Store the secret
Since we do not want to store the secret directly in the code, we will create a new setting `PlayFabDeveloperSecretKey` in `local.settings.json`. Substitute `<YOUR SECRET KEY>` with the secret you just copied from PlayFab:

    {
        "IsEncrypted": false,
        "Values": {
            "AzureWebJobsStorage": "<connection string to queue storage>"
            "PlayFabDeveloperSecretKey": "<YOUR SECRET KEY>",
            "FUNCTIONS_WORKER_RUNTIME": "dotnet"
        }
    }

### Authenticating using PlayFab Static Settings
Now that we have all the information, let's go ahead and put together the code.

Go to your Function's code, and paste the following code below your parsed context:

    PlayFabSettings.staticSettings.TitleId = context.TitleAuthenticationContext.Id;
    PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable("DeveloperSecretKey");
    
The function should now look like this:

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
                PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable("DeveloperSecretKey");
            }
        }
    }

This is already all we need to make requests using `PlayFabServerAPI` or `PlayFabAdminAPI` - now we only need to make a request!

## Updating Player Data
Create a new Method `UpdatePlayerData` as below:

    private static async Task UpdatePlayerData(ILogger log, PlayerPlayStreamFunctionExecutionContext<dynamic> context)
    {
        var updateUserDataRequest = new PlayFab.ServerModels.UpdateUserDataRequest {
            PlayFabId = context.PlayerProfile.PlayerId,
            Data = new Dictionary<string, string> {
                {"<KEY>", "<VALUE>"}
            }
        };
        PlayFabResult<PlayFab.ServerModels.UpdateUserDataResult> result = await PlayFabServerAPI.UpdateUserDataAsync(updateUserDataRequest);
        
        if (playFabResult.Error != null)
        {
            log.LogError($"Code: {playFabResult.Error.Error}\n{playFabResult.Error.ErrorMessage}");
        }
    }

an call it in your Function's `Run()` method:

    await UpdatePlayerData(log, context);
    
Your full function code should now look something like this:

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
        public static class Function1
        {
            [FunctionName("Function1")]
            public static async Task Run([QueueTrigger("login-events", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
            {
                var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(myQueueItem);
                log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
    
                PlayFabSettings.staticSettings.TitleId = context.TitleAuthenticationContext.Id;
                PlayFabSettings.staticSettings.DeveloperSecretKey =
                    Environment.GetEnvironmentVariable("DeveloperSecretKey");
    
                await UpdatePlayerData(log, context);
                await CreateNews(log, context);
                await CreateItem(log, context);
            }
            
            private static async Task UpdatePlayerData(ILogger log, PlayerPlayStreamFunctionExecutionContext<dynamic> context)
            {
                var updateUserDataRequest = new PlayFab.ServerModels.UpdateUserDataRequest {
                    PlayFabId = context.PlayerProfile.PlayerId,
                    Data = new Dictionary<string, string> {
                        {"<KEY>", "<VALUE>"}
                    }
                };
                PlayFabResult<PlayFab.ServerModels.UpdateUserDataResult> result = await PlayFabServerAPI.UpdateUserDataAsync(updateUserDataRequest);

                if (playFabResult.Error != null)
                {
                    log.LogError($"Code: {playFabResult.Error.Error}\n{playFabResult.Error.ErrorMessage}");
                }
            }
        }
    }

Ok, that's quite a chunk. Now what does `UpdatePlayerData()` do? Let's analyze the different blocks:

#### Creating a Request object

    var updateUserDataRequest = new PlayFab.ServerModels.UpdateUserDataRequest {
        PlayFabId = context.PlayerProfile.PlayerId,
        Data = new Dictionary<string, string> {
            {"<KEY>", "<VALUE>"}
        }
    };
    
Each PlayFab API request has a corresponding Request object, which is named accordingly and is located in the Namespace `PlayFab.<API TYPE>Models`, where `<API TYPE>` corresponds to one of the many APIs, like the Server, Admin, User or Authentication APIs, among others.

The `UpdateUserDataRequest` *requires* a `PlayerId`, so it knows which Player to update. You can retrieve that from the Context that we deserialized earlier.

Now to the core: Adding a `Dictionary<string, string>` as Data, containing Key/Value pairs for the Data we want to set on the Player- e.g. the amount of Mana.

#### Sending the Request
    PlayFabResult<PlayFab.ServerModels.UpdateUserDataResult> result = await PlayFabServerAPI.UpdateUserDataAsync(updateUserDataRequest);
    
Sending the request is as simple as calling the static Method `PlayFabServerAPI.UpdateUserDataAsync()` and passing it the request object we created before as a parameter. This method will return a Result object, we can then check for errors.

#### Checking for errors
    if (playFabResult.Error != null)
    {
        log.LogError($"Code: {playFabResult.Error.Error}\n{playFabResult.Error.ErrorMessage}");
    }
    
Basically, whenever the `Error` property on the Request object is non-null, an Error has occurred. There are a couple more Properties on the Error object which help you identify the problem before going further.

## Testing locally
Start the the Function locally, e.g. by pressing F5 in Visual Studio. Then, launch the `game-cli` to generate a new Player which then creates a PlayStream event, triggering the Rule which calls the Function. As we trigger the function using a Queue, we can again just read them locally.

Once the Function has processed the new trigger, go back to Azure PlayFab and see whether the newly created player is listed in Azure PlayFab's *Player* listing. Then go to the player's detail view and click "Player Data (Title)". You should now see the newly added key/value pair - in my case it's `foo` and `bar`:

![PlayFab: added PlayerData](images/PlayFab-AddedPlayerData.png)

## Deploying the Function
Now that we have a working Azure Function, let’s deploy it to Azure!

### Deploy with the "Publish" command
To get started and for the sake of this demo, we will be using “right click deploy”, the deployment mechanism integrated in Visual Studio. While this is convenient for a demo, please consider [Azure DevOps](https://dev.azure.com/) or [GitHub Actions](https://github.com/features/actions) to build your Continuous Integration & Continuous Deployment (CI/CD) pipelines for a real project – because *“Friends don’t let Friends right-click publish!”*

In Visual Studio, right-click the project and click “Publish”:

![Right-click publish](images/VisualStudio-RightClick-Publish.png)

This will open a dialog where you can configure to which Azure Function App you want to deploy – and you can even create a new one using this dialog!

While this deployment dialog also supports other deployment methods, please select “Azure”.

![Publish - select "Azure"](images/VisualStudio-Publish-1.png)

Then, “*Azure Functions App (Windows)*”.

![Publish - Select Azure Functions (Windows)](images/VisualStudio-Publish-2.png)

In the following Dialog you can choose the Azure Function App or slot beneath an app to deploy to.

If you haven’t already created an Azure Function App to deploy to, you can use the “*Create a new Azure Function…*” link to do so:

![Publish - Resources overview](images/VisualStudio-Publish-3.png)

![Publish - Create new Azure Functions App](images/VisualStudio-Publish-4.png)

Now select the Function App you want to deploy to:

![Publish - Resources overview - Select Function App](images/VisualStudio-Publish-5.png)

Once selected, Visual Studio will pull the “Publishing Profile” from Azure.

> If there is an exclamation mark warning symbol somewhere, hover over it to know why. In the screenshot below, my Storage configuration was not properly configured for the Publish wizard to work with, so I had to fix this.

Now click “Publish” to deploy your Azure Function App.

### Testing the online Function
Since you have now deployed the Azure Function, let's go ahead and test it.

First, make sure no local instance of the Function is running, so it is not competing against the online Azure Function when reading from the Azure Storage Queue.

Next, launch the `game-cli` again, which should log-in a new Account and trigger the Function via the Storage Queue.

You should now see the entry we created in the PlayFab data.

# Conclusion
In this article, we learned about the different PlayFab APIs and it's "perspectives". Then we learned how we could use these APIs on both the Client- and Backend-side, which are hosted in a very cost-effective serverless environment on Azure. Not forgetting to hook up PlayFab PlayStream events with Azure Functions using Rules in PlayFab.

# Apendix
## Notes
* It would be good practice to rename `Function1` to something sensible :-)
* You might want to use [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/) for secrets instead of AppSettings, or [Azure Managed Identities](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet), for even more security.