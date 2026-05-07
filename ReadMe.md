# Cognex WebSDK In Python (Unofficial)

This API example in Python is not supported by Cognex.

## Overview

- `cognex_camera.py` defines two classes:
  - `CogSocket`
  - `CognexCamera`
- `async_example.py` is an example implementation of `cognex_camera.py`

## Requirements

Install required packages:
- pip install websockets

```bash
import asyncio
import json
from cognex_camera import CognexCamera

async def main():
    # Define a camera
    camera = CognexCamera(ip='192.168.0.5', port=80, username='admin', password='')

    # Connect to the camera
    await camera.connect_async()

    # Trigger the camera
    await camera.manual_trigger_async()

    # Disconnect from the camera
    await camera.disconnect_async()

if __name__ == "__main__":
    asyncio.run(main())
```

## Supported methods
- connect_async()
- manual_trigger_async()
- get_info_async()
- find_state_async()
- list_camera_files_async()
- set_cell_expression_async('yourCell', 'yourFunction')
- set_cell_value_async('yourCell', yourValue)
- query_check_cell_results_async('yourCell')
- get_cell_expressions_async('yourCell')
- go_offline_async()
- go_online_async()
- online_offline_async()
- disconnect_async()
