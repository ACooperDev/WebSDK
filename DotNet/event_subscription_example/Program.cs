using System;
using System.Threading.Tasks;
using CognexCameraSdk;
using System.Linq;

var camera = new CognexCamera("192.168.0.106",80,"admin","");

// Subscribe to events
camera.StateChanged += body =>
{
    Console.WriteLine($"[StateChanged] {body?.GetRawText()}");
    return Task.CompletedTask;
};

camera.ResultsChanged += body =>
{
    string? value = null;

    if (body.HasValue && body.Value.TryGetProperty("cells", out var cells))
    {
        foreach (var cell in cells.EnumerateArray())
        {
            if (cell.TryGetProperty("location", out var location) &&
                location.GetString() == "B16")
            {
                if (cell.TryGetProperty("data", out var data))
                {
                    value = data.ToString();
                }

                break;
            }
        }
    }

    Console.WriteLine($"Value at B16: {value ?? "None"}");

    return Task.CompletedTask;
};

camera.LiveModeChanged += body =>
{
    Console.WriteLine($"[LiveModeChanged] {body?.GetRawText()}");
    return Task.CompletedTask;
};

camera.JobInfoChanged += body =>
{
    Console.WriteLine($"[JobInfoChanged] {body?.GetRawText()}");
    return Task.CompletedTask;
};

camera.EditorAttachedChanged += body =>
{
    Console.WriteLine($"[EditorAttachedChanged] {body?.GetRawText()}");
    return Task.CompletedTask;
};

camera.JobLoadingChanged += body =>
{
    Console.WriteLine($"[JobLoadingChanged] {body?.GetRawText()}");
    return Task.CompletedTask;
};

camera.SettingsChanged += body =>
{
    Console.WriteLine($"[SettingsChanged] {body?.GetRawText()}");
    return Task.CompletedTask;
};

camera.JobLoadFailed += body =>
{
    Console.WriteLine($"[JobLoadFailed] {body?.GetRawText()}");
    return Task.CompletedTask;
};

camera.JobValidationDone += body =>
{
    Console.WriteLine($"[JobValidationDone] {body?.GetRawText()}");
    return Task.CompletedTask;
};

camera.SessionDisposed += body =>
{
    Console.WriteLine($"[SessionDisposed] {body?.GetRawText()}");
    return Task.CompletedTask;
};

// Ctrl+C stops the application
var exit = new TaskCompletionSource();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    exit.TrySetResult();
};

try
{
    Console.WriteLine("Connecting...");

    await camera.Connect();
    Console.WriteLine("Connected.");

    await camera.SendReady();
    Console.WriteLine("Ready.");

    Console.WriteLine("Waiting for events...");
    Console.WriteLine("Press Ctrl+C to exit.");

    await exit.Task;
}
finally
{
    await camera.Disconnect();
    Console.WriteLine("Disconnected.");
}