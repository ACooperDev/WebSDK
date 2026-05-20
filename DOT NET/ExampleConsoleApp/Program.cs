using Cognex.InSight.Web;
using Cognex.InSight.Remoting.Serialization;
using Newtonsoft.Json.Linq;


namespace myConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CognexCamera camera = new CognexCamera();

            camera.EditorAttachedChanged += InSight_EditorAttached;

            HmiSessionInfo sessionInfo = new HmiSessionInfo
            {
                SheetName = "Inspection",
                CellNames = new[] { "A0:Z599" },
                EnableQueuedResults = true,
                IncludeCustomView = true
            };

            await camera.Connect("192.168.0.74:80", "admin", "", sessionInfo);
            
            Console.WriteLine("Connected");
            
            await camera.SendReady();

            // Example methods
            await camera.ManualAcquire();

            // Keep console app alive so events can happen
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();

            camera.EditorAttachedChanged -= InSight_EditorAttached;
            await camera.Disconnect();
        }

        private static void InSight_EditorAttached(object? sender, EventArgs e)
        {
            Console.WriteLine("Editor Attached Event");
            CognexCamera inSight = sender as CognexCamera;
            Console.WriteLine(inSight.EditorAttached.ToString());
         
        }
    }
}