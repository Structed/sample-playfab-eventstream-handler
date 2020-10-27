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
            PlayFabSettings.staticSettings.TitleId = "881B3"; // Please change this value to your own titleId from PlayFab Game Manager

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

