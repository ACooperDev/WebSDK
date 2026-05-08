# An example of using the CognexCamera class with async/await in Python.
import asyncio
import json
from cognex_camera import CognexCamera

async def main():
    # Create camera
    camera = CognexCamera(ip='192.168.0.5', port=80, username='admin', password='')

    try:
        # Connect
        print("Connecting to camera...")
        await camera.connect_async()
        print("Connected.")

        # Manual trigger
        print("Sending manual trigger...")
        await camera.manual_trigger_async()
        print("Manual trigger sent.")
        
        # Get info
        # print("Getting camera info...")
        # info = await camera.get_info_async()
        # print(f"Camera Info: {info}")
        # infoData = json.loads(info)
        # print(infoData["name"])
        # print(infoData["model"])
        # print(infoData["firmwareVersion"])
        # print(infoData["macID"])
        # print(infoData["serial"])

        # Find state
        # print("Getting camera state...")
        # state = await camera.find_state_async()
        # print(f"Camera State: {state}")
        # Boolean for online/offline status
        # stateData = json.loads(state)
        # print(stateData["online"])

        # List files
        # print("Listing camera files...")
        # files = await camera.list_camera_files_async()
        # print(f"Camera Files: {files}")

        # Set cell expression and value
        # print("Setting cell B16 expression and value...")
        # await camera.set_cell_expression_async('B16', 'EditInt(0,100)')
        # await camera.set_cell_value_async('B16', 42)
        # print("Cell B16 expression and value set.")
        
        # Query cell results (example)
        # print("Querying cell B16 results...")
        # results = await camera.query_check_cell_results_async('B16')
        # print(f"Cell B16 Results: {results}")

        # Get cell expressions
        # print("Getting cell B16 expression...")
        # expr = await camera.get_cell_expressions_async('B16')
        # print(f"Cell B16 Expression: {expr}")

        # Enable/disable live mode
        # print("Going live...")
        # await camera.live_mode_async(True)
        # print("Live mode enabled.")
        # print("Going !live...")
        # await camera.live_mode_async(False)
        # print("Live mode disabled.")
        
        # Go offline
        # print("Going offline...")
        # await camera.go_offline_async()
        # print("Camera is now offline.")
        
        # Go online
        # print("Going online...")
        # wait camera.go_online_async()
        # print("Camera is now online.")
        
        # Toggle online/offline
        # print("Toggling online/offline...")
        # await camera.online_offline_async()
        # print("Online/offline toggled.")

        # Current job info
        # print("Getting current job info...")
        # jobInfo = await camera.get_jobinfo_async()
        #print(f"Job Info: {jobInfo}")
        # jobInfoData = json.loads(jobInfo)
        # print(jobInfoData["name"])

        # Save job
        # print("Saving current job...")
        # await camera.save_job_async("MyJob5.jobx")
        # print("Job has been saved.")

        # Load job
        # print("Loading job...")
        # await camera.load_job_async("MyJob2.jobx")
        # print("Job has been loaded.")
        
    finally:
        # Disconnect
        print("Disconnecting...")
        await camera.disconnect_async()
        print("Disconnected.")

if __name__ == "__main__":
    asyncio.run(main())
