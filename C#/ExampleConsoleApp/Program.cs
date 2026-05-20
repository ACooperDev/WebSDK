//dotnet add package Newtonsoft.Json
//dotnet add package WebSocketSharp.Standard --version 1.0.3

using Cognex.InSight.Remoting.Serialization;
using Cognex.InSight.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace myConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CvsInSight camera = new CvsInSight();

            // Event subscription
            camera.StateChanged += StateChanged;
            camera.ResultsChanged += ResultsChanged;
            camera.LiveModeChanged += LiveModeChanged;
            camera.JobInfoChanged += JobChanged;
            camera.EditorAttachedChanged += EditorAttached;
            camera.JobLoadingChanged += JobLoading;
            camera.SettingsChanged += SettingsChanged;
            camera.JobLoadFailed += JobLoadFailed;
            camera.JobValidationDone += JobValidationComplete;
            camera.SessionDisposed += SessionDisposed;
            camera.EditorAttachedChanged += InSight_EditorAttached;

            // Camera setup
            HmiSessionInfo sessionInfo = new HmiSessionInfo
            {
                SheetName = "Inspection",
                CellNames = new[] { "A0:Z599" },
                EnableQueuedResults = true,
                IncludeCustomView = true
            };

            Console.WriteLine("Connecting...");
            await camera.Connect("192.168.0.74:80", "admin", "", sessionInfo);
            Console.WriteLine("Connected");
            
            await camera.SendReady();

            // Example methods
            /*
            Console.WriteLine("Sending trigger");
            await camera.ManualAcquire();
            Console.WriteLine("Trigger sent");
            */

            /*
            Console.WriteLine("Camera info");
            JToken infoObject = await camera.Info();
            Console.WriteLine(infoObject.ToString());
            */

            /*
            Console.WriteLine("Listing files");
            JObject[] result = await camera.ListFiles();
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            */

            /*
            Console.WriteLine("Setting cell expression");
            await camera.SetCellExpression("A12", "EditInt(0,100)");
            */

            /*
            Console.WriteLine("Setting cell value");
            await camera.SetCellValue("A12", 42);
            */

            /*
            Console.WriteLine("Querying cell result");
            object[] cells = await camera.QueryCellResults("A12");
            Console.WriteLine(JsonConvert.SerializeObject(cells, Formatting.Indented));
            */

            /*
            Console.WriteLine("Getting cell expression");
            string exp = await camera.GetCellExpression("A12");
            Console.WriteLine(exp);
            */

            /*
            Console.WriteLine("Turning on live mode");
            await camera.SetLiveModeAsync(true);
            Console.WriteLine("Live mode on");
            Console.WriteLine("Turning off live mode");
            await camera.SetLiveModeAsync(false);
            Console.WriteLine("Live mode off");
            */

            /*
            Console.WriteLine("Going offline");
            await camera.SetSoftOnlineAsync(false);
            Console.WriteLine("Offline");

            Console.WriteLine("Going online");
            await camera.SetSoftOnlineAsync(true);
            Console.WriteLine("Online");
            */

            /*
            Console.WriteLine("Getting job info");
            Console.WriteLine(camera.JobInfo);
            JToken jobInfo = await camera.GetJobInfo();
            Console.WriteLine(jobInfo);
            */

            /*
            Console.WriteLine("Saving job");
            await camera.SaveJob("myJob.jobx");
            Console.WriteLine("Job saved");
            */

            /*
            Console.WriteLine("Loading job");
            await camera.LoadJob("MyJob.jobx");
            Console.WriteLine("Job loaded");
            */

            /*
            Console.WriteLine("Sending ready");
            await camera.SendReady();
            Console.WriteLine("Sent ready");
            */

            /*
            Console.WriteLine("Getting session ID's");
            object result = await camera.GetSessionIDs();
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            */

            /*
            Console.WriteLine("Getting job validation state");
            string state = await camera.JobValidationState();
            Console.WriteLine(state);
            */

            /*
            Console.WriteLine("Getting system validation state");
            int valState = await camera.SystemValidationFlag();
            Console.WriteLine(valState.ToString());
            */

            /*
            Console.WriteLine("Running job validation");
            await camera.RunJobValidation();
            */

            /*
            Console.WriteLine("Cancelling job validation");
            await camera.CancelJobValidation();
            */

            /*
            Console.WriteLine("Set and get keep alive timeout");
            int timeout = await camera.GetKeepAliveTimeout();
            Console.WriteLine(timeout.ToString());
            await camera.SetKeepAliveTimeout(50);
            */

            /*
            Console.WriteLine("Loading image");
            camera.LoadImage(@"C:\test\myImage.bmp");
            */

            // Keep console app alive so events can happen
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();

            // Unsubscribe and disconnect
            camera.StateChanged -= StateChanged;
            camera.ResultsChanged -= ResultsChanged;
            camera.LiveModeChanged -= LiveModeChanged;
            camera.JobInfoChanged -= JobChanged;
            camera.EditorAttachedChanged -= EditorAttached;
            camera.JobLoadingChanged -= JobLoading;
            camera.SettingsChanged -= SettingsChanged;
            camera.JobLoadFailed -= JobLoadFailed;
            camera.JobValidationDone -= JobValidationComplete;
            camera.SessionDisposed -= SessionDisposed;
            camera.EditorAttachedChanged -= InSight_EditorAttached;

            await camera.Disconnect();
        }

        // Event handlers

        private async static void StateChanged(object? sender, EventArgs e)
        {
            /*
            Console.WriteLine("State Changed Event");
            CvsInSight inSight = sender as CvsInSight;
            JToken myState = await inSight.FindState();
            Console.WriteLine(myState);
            */
        }

        private async static void ResultsChanged(object? sender, EventArgs e)
        {
            
            Console.WriteLine("Results Changed Event");
            CvsInSight camera = sender as CvsInSight;
            await camera.SendReady();
            JToken results = camera.Results;
            //Console.WriteLine(results);

            //Get a particular cell value by name or location
            JArray cells = (JArray)results["cells"];
            JToken myCell = cells.FirstOrDefault(c => (string)c["location"] == "B3");
            if (myCell != null)
            {
                int value = myCell["data"].Value<int>();
                Console.WriteLine(value.ToString());
            }
            
        }

        private async static void LiveModeChanged(object? sencder, EventArgs e)
        {
            //Console.WriteLine("Live Mode Changed Event"); 
        }

        private async static void JobChanged(object? sender, EventArgs e)
        {
            //Console.WriteLine("Job Changed Event");
        }

        private async static void EditorAttached(object? sencder, EventArgs e)
        {
            //Console.WriteLine("Editor Attached Event");
        }

        private async static void JobLoading(object? sencder, EventArgs e)
        {
            //Console.WriteLine("Job Loading Event");
        }

        private async static void SettingsChanged(object? sencder, EventArgs e)
        {
            //Console.WriteLine("Settings Changed Event");
        }

        private async static void JobLoadFailed(object? sencder, EventArgs e)
        {
            //Console.WriteLine("Job Load Failed Event");
        }

        private async static void JobValidationComplete(object? sencder, EventArgs e)
        {
            //Console.WriteLine("Job Validation Complete Event");
        }

        private async static void SessionDisposed(object? sencder, EventArgs e)
        {
            //Console.WriteLine("SessionDisposed Event");
        }

        private static void InSight_EditorAttached(object? sender, EventArgs e)
        {
            /*
            Console.WriteLine("Editor Attached Event");
            CvsInSight inSight = sender as CvsInSight;
            Console.WriteLine(inSight.EditorAttached.ToString());
            */
        }
    }
}