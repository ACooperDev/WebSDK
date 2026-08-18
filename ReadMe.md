# Cognex REST API Implementations (Unofficial)

This is an independent project not affiliated with, endorsed by, or supported by Cognex Corporation.

Implementations are in Python, JavaScript, and .NET with the goal of creating a standarized object, cognex_camera, across all three programming languages with a common set of properties, methods, and events.

TODO: 
- continue implementing useful methods. 
- get c# up to par with js and py.
- cam_scanner.py TODO .cs, (.js not possible)

[Python](#python-overview)
  - Ideal for scripts and automation tasks.

[JavaScript](#javascript-overview)
  - Optimized for lightweight control panels and dashboards.

[C# .NET](#net-c-getting-started)
  - Perfect for industrial HMI apps, SCADA, and WinForms/WPF.

[Utilities](#utilities)
  - Useful utilities outside of genreal scope of Rest API Implementation
    - cam_scanner 

# cognex_camera (.py, .js, .cs)
## Camera Properties
```python
# Python
camera = CognexCamera(ip, optionalProperties)
```
```javascript
// JavaScript
camera = new CognexCamera(ip, optionalProperties)
```
```csharp
// C#
CognexCamera camera = new CognexCamera(ip, optionalProperties)
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
```python
# Python
await camera.selectedMethod
# or
value = await camera.selectedMethod
```
```js
// JavaScript
await camera.selectedMethod
// or
const value = await camera.selectedMethod
```
```csharp
// C#
await camera.selectedMethod
// or
var value = await camera.selectedMethod
````

- Connect()
  - Connects to the camera and creates a session.   
- Disconnect()
  - Disconnects from the camera and closes the session. 
- ManualAcquire()
  - Manually triggers an acquisition. 
- SetLiveMode(bool)
  - Enables or disables live mode.
- GetLiveMode()
  - Gets the current live mode status.  
- ToggleOnlineOffline()
  - Toggles to the opposite online state. 
- QueryCellResults(cell: str)
  - Query cells results
- GetCellExpression(cells: str)
  - Gets cell expressions.
- SetCellExpression(cell: str, function: str)
  - Sets a cell expression.
- SetCellValue(cell: str, value)
  - Sets a cell value. 
- ListFiles()
  - Lists all files on a camera.
- Info()
  - Returns info not limited to cameara name, model, firmware, MAC, and serial.
- FindState()
  - Returns online state for discrete online, ffp online, live mode online, native online, online, and soft online. 
- SetLiveModeAsync(bool)
  - Sets soft online to true 
- GetJobInfo()
  - Returns job information including job name.
- SaveJob(job: str)
  - Saves the current job with the chosen job name.
- LoadJob(job: str)
  - Loads a job by name.
- SendReady()
  - Updates the result for the session when one is available from a camera.  A on_result_changed event is raised when the result has changed.
- GetSessionIDs()
  - Gets all camera sessions by ID.
- JobValidationState()
  - Returns the state of validation.
- SystemValidationFlag()
  - Gets the state of system validation.
- RunJobValidation()
  - Runs the job validation set.
- CancelJobValidation()
  - Cancels a started job validation run.
- GetKeepAliveTimeout()
  - Returns the session timeout interval.
- SetKeepAliveTimeout(value)
  - Sets the session timeout interval.
- LoadImage(imagePath: str, imageName: str)
  - Loads an image from disk to the camera.
- NMC(nmc: str, timeout: double, port: int, ip: str, username: str, password: str)
  - Sends a Native Mode Command.  Python and C# only.
- GetStartupJob()
  - Returns the current startup job.
- SetStartupJob(jobName: str)
  - Sets a job as the startup job.
- GetAllCellNames()
  - Returns all cell names with cell location.   
- SetCellName(cell: str, name: str)
  - Sets a cells name.
- CreateNewJob()
  - Creates a new job.
- StartupOnline(state: boolean)
  - Sets the camera startup online status to true or false.
- StartupOnlineStatus()
  - Returns the camera startup online status.
- GetCellCondition(cell: str)
  - Gets the condition of a cell.
- SetCellCondition(cell: str, condition: str)
  - Sets a cells condition.   

## Camera Events
```python
# Python
camera.selectedEvent.append(yourCustomMethod)
```
```js
// JavaScript
camera.selectedEvent.push(async (state) =>{
  console.log(state);
});
```
```csharp
// C#
camera.selectedEvent += yourEventHandler;
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
cognex_camera can be built/modified from [here.](Python/)
  
## Python Getting Started

```python
import asyncio
import json
from cognex_camera import CognexCamera

async def main():
    # Define a camera
    camera = CognexCamera(ip='192.168.0.5')

    # Connect to the camera
    await camera.Connect()
    await camera.SendReady()

    # Trigger the camera
    await camera.ManualAcquire()

    # Disconnect from the camera
    await camera.Disconnect()

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
    await camera.SendReady()

    # Connect to the camera

    # Option 1: Wait for response.  Application will not continue until the response has arrived or times out.
    info = await camera.Info()
    print(f"Camera Info: {info}")

    # Option 2: Fire and forget.  Application will continue without waiting for the respone to arrive.
    asyncio.create_task(camera.Info())
    print("Immediately continue without confirming the action is complete")

    # Option 3: Fire and sort of forget.  Application will continue immediately up until the await is requested.
    # If it is not complete by that point, the application will not continue until the response has arrived.
    # If it has completed by that point the application will continue on immediately.
    task = asyncio.create_task(camera.Info())
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
    camera.StateChanged.append(state_changed_handler)

    try:
        # Connect
        await camera.Connect()
        await camera.SendReady()

        # Loop
        while True:
            async with state_lock:
                
                if state.state_changed:
                    # print("MAIN saw state change:", state.last_state)
                    state.last_state = False

            # Don't hammer your CPU, a sleep will not miss events, no matter how long it is.
            # All events will be queued and dequeued in order.
            await asyncio.sleep(0.1)
            
    finally:
        print("Disconnecting...")
        await camera.Disconnect()
        print("Disconnected")

if __name__ == "__main__":
    asyncio.run(main())

````
# JavaScript Overview
- `cognex_camera.js` defines two classes:
  - `CogSocket`
  - `CognexCamera`
- `example.html` is an example emplementation of `cognex_camera.js`

cognex_camera can be built/modified from [here.](JavaScript/)

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

                        //Event subscriptions
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
                                await camera.Connect();
                                await camera.ManualAcquire();
                                const files = await camera.ListFiles()
                                console.log(files)
                                await camera.Disconnect();
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
Cognex.InSight.Web.dll can be built/modified from [here](C%23/cognex_camera/) or downloaded directly [here.](C%23/cognex_camera/bin/Debug/net9.0-windows7.0/) 

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

<br>

# Utilities
## cam_scanner (.py, .js, .cs)
### Discovers Cognex Device IP's & ISVS Device Details
```python
# Python
import asyncio
import json
from cognex_camera import CognexCamera
from cam_scanner import CogScanner

async def main():
    scanner = CogScanner(timeout=15, max_workers=100)
    # Ping a range to update your PC ARP cache
    results = scanner.scan("192.168.0.0/24", "00:D0:24")
    
    results_list = []
    
    for ip, mac in results.items():
        # print(f"{ip} : {mac}")
        try:
            camera = CognexCamera(ip=ip, port=80, username='admin', password='')
            await camera.Connect()
            await camera.SendReady()
            resp = await camera.Info()
            data = json.loads(resp)
            results_list.append({"ip": ip,"mac": mac,"model": data.get("model"),"serial": data.get("serial"),"name": data.get("name"), "firmware version": data.get("firmwareVersion"),"error": None})
            await camera.Disconnect()
        except Exception as e:
            # print(f"Error occurred while processing {ip}: {e}")
            results_list.append({"ip": ip,"mac": mac,"model": None, "serial": None,"name": None,"error": str(e)})
    
    for result in results_list:
        print(result)

if __name__ == "__main__":
    asyncio.run(main())     
```
