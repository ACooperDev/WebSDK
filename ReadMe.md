# Cognex REST API Implementations (Unofficial)
TODO: standardize on event names and method names

JS
- methods

Python
- events
- methods

This is an independent project not affiliated with, endorsed by, or supported by Cognex Corporation.

Implementations are in Python, JavaScript, and .NET with the goal of creating a standarized object, cognex_camera, across all three programming languages with a common set of properties, methods, and events.

[Python](#python-overview)
  - Ideal for scripts and automation tasks.

[JavaScript](#javascript-overview)
  - Optimized for lightweight control panels and dashboards.

[C# .NET](#net-c-getting-started)
  - Perfect for industiral HMI apps, SCADA, and WinForms/WPF.

# cognex_camera (.py, .js, .cs)
## Camera Properties
```bash
# Python
camera = CognexCamera(ip, optionalProperties)

// JavaScript
camera = new CognexCamera(ip, optionalProperties)
````
- Required to create a new camera
  - ip (string)
     - Camera IP address
  - port (int, optional)
    - Camera web port. Defaulted: 80
  - username (string, optional)
    - Camera username. Defaulted: "admin"
  - password (string, optional)
    - Camera password. Defaulted: "" 
- Get or set after a camera is created
  - cogsock
    - A new CogSocket("ws://ip:port/ws") 
  - session_id
    - The unique session represnting a connection to a cmaera.  Example: cam0/hmi/hs/~152028e7 
  - keep-alive_task
    - A request to keep the current session alive and availble.  Default sessions are only kept alive for 30 seconds.  Limits [3, 30000]
  - root
    - Defaulted: 'cam0/hmi'
  - cells
    - Defaulted 'A0:Z100' 
  
## Camera Methods
```bash
# Python
camera = CognexCamera(ip)
await camera.selectedMethod
# or
value = await camera.selectedMethod

// JavaScript
camera = new Camera(ip)
await camera.selectedMethod
// or
const value = await camera.selectedMethod

````
- connect()
  - Connects to the camera and creates a session.   
- disconnect()
  - Disconnects from the camera and closes the session. 
- manual_trigger()
  - Manually triggers an acquisition. 
- live_mode(bool)
  - Enables or disables live mode.
- online_offline()
  - Toggles to the opposite online state. 
- query_check_cell_results(cell: str)
  - Query cells results
- get_cell_expressions(cells: str)
  - Gets cell expressions.
- set_cell_expression(cell: str, function: str)
  - Sets a cell expression.
- set_cell_value(cell: str, value)
  - Sets a cell value. 
- list_camera_files()
  - Lists all files on a camera.
- get_info()
  - Returns info not limited to cameara name, model, firmware, MAC, and serial.
- find_state()
  - Returns online state for discrete online, ffp online, live mode online, native online, online, and soft online. 
- go_online()
  - Sets soft online to true 
- go_offline()
  - Sets soft online to false.
- get_jobinfo()
  - Returns job information including job name.
- save_job(job: str)
  - Saves the current job with the chosen job name.
- load_job(job: str)
  - Loads a job by name.
- ready()
  - Updates the result for the session when one is available from a camera.  A on_result_changed event is raised when the result has changed.
- session_IDs()
  - Gets all camera sessions by ID.
- job_validation_state()
  - Returns the state of validation.
- system_validation_flag_asyn()
  - Gets the state of system validation.
- run_job_validation()
  - Runs the job validation set.
- cancel_job_validation()
  - Cancels a started job validation run.
- get_keep_alive_interval()
  - Returns the session timeout interval.
- set_keep_alive_interval(value)
  - Sets the session timeout interval.
- load_image(imagePath: str, imageName: str)
  - Loads an image from disk to the camera. 

## Camera Events
```bash
# Python
camera = CognexCamera(ip)
camera.selectedEvent.append(yourCustomMethod)

// JavaScript
camera = new CognexCamera(ip)
camera.selectedEvent.push(async (state) =>{
  console.log(state);
});
````
- StateChanged
  - Fired when the camera state changes. 
- ResultsChanged
  - Fired when the session is ready and there is a new result.
- LiveModeChanged
  - Fired when the cameras live mode is toggled.
- JobInfoChanged
  - Fired after any of the job values have changed in cam0/hmi/job. 
- EditorAttachedChanged
  - Fired when ISVS editor has connected to a camera. 
- JobLoadingChanged
  - Fired after the job loading flag has changed. 
- SettingsChanged
  - Fired after HMI value settings have changed. 
- JobLoadFailed
  - Fired after a job load has failed.
- JobValidationDone
  - Fired after job validation has completed. 
- SessionDisposed
  - Fired when the HMI session has been disposed due to timeout or another disconnection type.

# Python Overview

- `cognex_camera.py` defines two classes:
  - `CogSocket`
  - `CognexCamera`
- `async_example.py` is an example implementation of `cognex_camera.py`
- `event_subscription_example.py` is an example implementation of `cognex_camera.py` event subscription

## Python Requirements
Python 3.14.0

Install required packages:
```bash
pip install websockets
```
```bash
pip install httpx
```
  
## Python Getting Started

```python
import asyncio
import json
from cognex_camera import CognexCamera

async def main():
    # Define a camera
    camera = CognexCamera(ip='192.168.0.5')

    # Connect to the camera
    await camera.connect()
    await camera.ready()

    # Trigger the camera
    await camera.manual_trigger()

    # Disconnect from the camera
    await camera.disconnect()

if __name__ == "__main__":
    asyncio.run(main())
```
## Python asyncio Options

```python
import asyncio
import json
from cognex_camera import CognexCamera

async def main():
    # Define a camera
    camera = CognexCamera(ip='192.168.0.5')
    await camera.ready()

    # Connect to the camera

    # Option 1: Wait for response.  Application will not continue until the response has arrived or times out.
    info = await camera.get_info()
    print(f"Camera Info: {info}")

    # Option 2: Fire and forget.  Application will continue without waiting for the respone to arrive.
    asyncio.create_task(camera.get_info())
    print("Immediately continue without confirming the action is complete")

    # Option 3: Fire and sort of forget.  Application will continue immediately up until the await is requested.
    # If it is not complete by that point, the application will not continue until the response has arrived.
    # If it has completed by that point the application will continue on immediately.
    task = asyncio.create_task(camera.get_info())
    # Run some logic
    value = await task
    print(f"Camera Info: {value}")
    
    # Disconnect from the camera

if __name__ == "__main__":
    asyncio.run(main())
```
## Python Recommendation
```python
# Add Try/Except to all method calls.
# Example
try:
    await camera.someMethod()
except Exception as e:
    print(f"Error occurred while running xMethod: {e}")
```


## Python Example Event Subscription
```python
import asyncio
import json
from cognex_camera import CognexCamera

class CameraState:
    def __init__(self):
        self.state_changed = False
        self.last_state = None

state = CameraState()

# Prevents race conditions
state_lock = asyncio.Lock()

# Event handler
async def state_changed_handler(*args):
    async with state_lock:
        #print(f"STATE EVENT: {args}")
        state.state_changed = True
        state.last_state = args

async def main():
    # Create camera
    camera = CognexCamera(ip='192.168.0.74')

    # Subscribe to events
    camera.on_state_changed.append(state_changed_handler)

    try:
        # Connect
        await camera.connect()
        await camera.ready()

        # Loop
        while True:
            async with state_lock:
                
                if state.settings_changed:
                    # print("MAIN saw settings change:", state.last_settings_result)
                    state.settings_changed = False

            # Don't hammer your CPU, a sleep will not miss events, no matter how long it is.
            # All events will be queued and dequeued in order.
            await asyncio.sleep(0.1)
            
    finally:
        print("Disconnecting...")
        await camera.disconnect()
        print("Disconnected")

if __name__ == "__main__":
    asyncio.run(main())

````
# JavaScript Overview
- `cognex_camera.js` defines two classes:
  - `CogSocket`
  - `CognexCamera`
- `example.html` is an example emplementation of `cognex_camera.js`

## JavaScript Getting Started
```html
<html>
    <body>
            <script src="cognex_camera.js"></script>
            <script>
                        const ROOT = 'cam0/hmi';
                        const CELLS = 'A0:Z100';
                        const ip = '192.168.0.74';
                        const USER = 'admin';
                        const PASS = '';

                        let camera = null;

                        camera = new CognexCamera(ip, 80, USER, PASS);
                        camera.cells = CELLS;

                        camera.StateChanged.push(async (state) => {
                            //console.log('[EVENT] Camera State Changed:', state);
                        });
            
                        camera.ResultsChanged.push(async (results) => {
                            //console.log('[EVENT] Results Changed:', results);
                        });
            
                        camera.LiveModeChanged.push(async (isLive) => {
                            //console.log('[EVENT] Live Mode Changed:', isLive);
                        });
            
                        camera.JobInfoChanged.push(async (jobName) => {
                            //console.log('[EVENT] Job Changed:', jobName);
                        });
            
                        camera.EditorAttachedChanged.push(async () => {
                            //console.log('[EVENT] Editor Attached');
                        });
            
                        camera.JobValidationDone.push(async () => {
                            //console.log('[EVENT] Job Validation Done');
                        });

                        //Example calling function
                        connectTriggerGetInfoDisconnect();

                        //Example function
                        async function connectTriggerGetInfoDisconnect(){
                            try{
                                await camera.connect();
                                await camera.manual_trigger();
                                const files = await camera.list_camera_files()
                                console.log(files)
                                await camera.disconnect();
                            }catch (e) {
                                console.log('Failure: ', e);
                            }
                        }
            </script>
    </body>
</html>
```
# .NET C# Overview
- `Cognex.InSight.Web.dll` defines two classes:
  - `CogSocket`
  - `CvsInSight`
- `Program.cs` is an example emplementation of `Cognex.InSight.Web.dll`

## .NET C# Requirements
.NET Framework or .NET

Install required packages:
```bash
dotnet add package Newtonsoft.Json
```
```bash
dotnet add package WebSocketSharp.Standard --version 1.0.3
```

## .NET C# Getting Started
```csharp
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
            camera.ResultsChanged += ResultsChanged;

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

            Console.WriteLine("Sending trigger");
            await camera.ManualAcquire();
            Console.WriteLine("Trigger sent");

            // Keep console app alive so events can happen
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();

            // Unsubscribe and disconnect
            camera.ResultsChanged -= ResultsChanged;

            await camera.Disconnect();
        }

        // Event handler
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
    }
}

```
