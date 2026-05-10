# Cognex WebSDK In Python (Unofficial)

This is an independent project not affiliated with, endorsed by, or supported by Cognex Corporation.

TODO: jobValidationState, runJobValidation, systemValidationFlag, cancelJobValidation, event subscription

## Overview

- `cognex_camera.py` defines two classes:
  - `CogSocket`
  - `CognexCamera`
- `async_example.py` is an example implementation of `cognex_camera.py` method calls
- `event_subscription_example.py` is an example implementation of `cognex_camera.py` event subscription
- `In-Sight HMI API.pdf` is the ISVS REST API documentation

## Requirements

Install required packages:
- pip install websockets
  
## Getting Started

```bash
import asyncio
import json
from cognex_camera import CognexCamera

async def main():
    # Define a camera
    camera = CognexCamera(ip='192.168.0.5')

    # Connect to the camera
    await camera.connect_async()
    await camera.ready_async()

    # Trigger the camera
    await camera.manual_trigger_async()

    # Disconnect from the camera
    await camera.disconnect_async()

if __name__ == "__main__":
    asyncio.run(main())
```
## asyncio Options

```bash
import asyncio
import json
from cognex_camera import CognexCamera

async def main():
    # Define a camera
    camera = CognexCamera(ip='192.168.0.5')
    await camera.ready_async()

    # Connect to the camera

    # Trigger the camera

    # Option 1: Wait for response.  Application will not continue until the response has arrived or times out.
    info = await camera.get_info_async()
    print(f"Camera Info: {info}")

    # Option 2: Fire and forget.  Application will continue without waiting for the respone to arrive.
    asyncio.create_task(camera.get_info_async())
    print("Immediately continue without confirming the action is complete")

    # Option 3: Fire and sort of forget.  Application will continue immediately up until the await is requested.
    # If it is not complete by that point, the application will not continue until the response has arrived.
    # If it has completed by that point the application will continue on immediately.
    task = asyncio.create_task(camera.get_info_async())
    # Run some logic
    value = await task
    print(f"Camera Info: {value}")
    
    # Disconnect from the camera

if __name__ == "__main__":
    asyncio.run(main())
```
## Recommendation
```bash
# Add Try/Except to all method calls.
# Example
try:
    await camera.someMethod()
except Exception as e:
    print(f"Error occurred while running xMethod: {e}")
```

## Camera Properties
- Required to create a new camera
  - ip
  - port
  - username
  - password
- Get or set after a camera is created
  - cogsock
  - session_id
  - keep-alive_task
  - root
  - cells
  
## Camera Methods
- connect_async()
- disconnect_async()
- manual_trigger_async()
- live_mode_async(bool)
- online_offline_async()
- query_check_cell_results_async('yourCell')
- get_cell_expressions_async('yourCell')
- set_cell_expression_async('yourCell', 'yourFunction')
- set_cell_value_async('yourCell', yourValue)
- list_camera_files_async()
- get_info_async()
- find_state_async()
- go_online_async()
- go_offline_async()
- get_jobinfo_async()
- save_job_async(string)
- load_job_async(string)
- ready_async()
- session_IDs_async()

## Camera Events
- on_state_changed
- on_result_changed
- on_liveMode_changed
- on_job_changed
- on_editorAttached
- on_jobLoading_changed
- on_settings_changed

## Example Event Subscription
```bash
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
        await camera.connect_async()
        await camera.ready_async()

        # Loop
        while True:
            async with state_lock:
                
                if state.settings_changed:
                    # print("MAIN saw settings change:", state.last_settings_result)
                    state.settings_changed = False

            # Don't hammer your CPU, a sleep will not miss events, no matter how long it is.  All events will be queued and dequeued in order.
            await asyncio.sleep(0.1)
            
    finally:
        print("Disconnecting...")
        await camera.disconnect_async()
        print("Disconnected")

if __name__ == "__main__":
    asyncio.run(main())

````
