using CognexCameraSdk;
using System.Text.Json;

string cameraIp = "192.168.0.106";
int cameraPort = 80;
string username = "admin";
string password = "";

CognexCamera camera = new CognexCamera(cameraIp, cameraPort, username, password);

try
{
    await camera.Connect();
    await camera.SendReady();

    //await camera.ManualAcquire();

    //await camera.SetLiveMode(false);

    //var resp = await camera.GetLiveMode();

    //await camera.ToggleOnlineOffline();

    /*
    var resp = await camera.QueryCellResults("A1");
    using JsonDocument doc = JsonDocument.Parse(resp);
    string data = doc.RootElement[0].GetProperty("data").ToString();
    */

    //var resp = await camera.GetCellExpression("A1");

    //await camera.SetCellExpression("A3", "EditInt(0,255)");

    //await camera.SetCellValue("A9",2);

    //var resp = await camera.ListFiles();

    //var resp = await camera.Info();

    //var resp = await camera.FindState();

    //await camera.SetSoftOnline(true);

    //var resp = await camera.GetJobInfo();

    //await camera.SaveJob("Test.jobx");

    //await camera.LoadJob("test.jobx");

    //await camera.SendReady();

    //var resp = await camera.GetSessionIDs();

    //var resp = await camera.JobValidationState();

    //var resp = await camera.SystemValidationFlag();

    //await camera.RunJobValidation();

    //await camera.CancelJobValidation();

    //var resp = await camera.GetKeepAliveTimeout();
    
    //await camera.SetKeepAliveTimeout(45);

    //await camera.LoadImage(@"C:\temp\1.bmp");

    //var resp = await camera.NMC("GI", 10, 23, cameraIp, username, password);

    //var resp = await camera.GetStartupJob();

    //await camera.SetStartupJob("test.jobx");

    //var resp = await camera.GetAllCellNames();

    //await camera.SetCellName("I16", "TestCell");

    //await camera.CreateNewJob();

    //await camera.StartupOnline(true);

    //var resp = await camera.StartupOnlineStatus();

    //var resp = await camera.GetCellCondition("A1");

    //await camera.SetCellCondition("A1","0");

    await camera.Disconnect();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}