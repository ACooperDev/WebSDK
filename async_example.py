# An example of using the CognexCamera class with async/await in Python.
import asyncio
import json
from cognex_camera import CognexCamera

async def main():
    # Create camera
    camera = CognexCamera(ip='192.168.0.74', port=80, username='admin', password='')

    try:
        # Connect
        print("Connecting to camera...")
        await camera.connect()
        await camera.ready()
        print("Connected.")

        # Manual trigger
        print("Sending manual trigger...")
        await camera.manual_trigger()
        print("Manual trigger sent.")

        """     
        # Get info
        print("Getting camera info...")
        info = await camera.get_info()
        print(f"Camera Info: {info}")
        infoData = json.loads(info)
        print(infoData["name"])
        print(infoData["model"])
        print(infoData["firmwareVersion"])
        print(infoData["macID"])
        print(infoData["serial"])
        """
        
        """
        # Find state
        print("Getting camera state...")
        state = await camera.find_state()
        print(f"Camera State: {state}")
        # Boolean for online/offline status
        stateData = json.loads(state)
        print(stateData["online"])
        """
        
        """
        # List files
        print("Listing camera files...")
        files = await camera.list_camera_files()
        print(f"Camera Files: {files}")
        """
        
        """
        # Set cell expression and value
        print("Setting cell B16 expression and value...")
        await camera.set_cell_expression('B16', 'EditInt(0,100)')
        await camera.set_cell_value('B16', 42)
        print("Cell B16 expression and value set.")
        """
        
        """
        # Query cell results (example)
        print("Querying cell B16 results...")
        results = await camera.query_check_cell_results('B16')
        print(f"Cell B16 Results: {results}")
        """

        """
        # Get cell expressions
        print("Getting cell B16 expression...")
        expr = await camera.get_cell_expressions('B16')
        print(f"Cell B16 Expression: {expr}")
        """

        """
        # Enable/disable live mode
        print("Going live...")
        await camera.live_mode(True)
        print("Live mode enabled.")
        print("Going !live...")
        await camera.live_mode(False)
        print("Live mode disabled.")
        """
        
        """
        # Go offline
        print("Going offline...")
        await camera.go_offline()
        print("Camera is now offline.")
        """
        
        """
        # Go online
        print("Going online...")
        await camera.go_online()
        print("Camera is now online.")
        """
        
        """
        # Toggle online/offline
        print("Toggling online/offline...")
        await camera.online_offline()
        print("Online/offline toggled.")
        """

        """
        # Current job info
        print("Getting current job info...")
        jobInfo = await camera.get_jobinfo()
        print(f"Job Info: {jobInfo}")
        jobInfoData = json.loads(jobInfo)
        print(jobInfoData["name"])
        """
        
        """
        # Save job
        print("Saving current job...")
        await camera.save_job("MyJob5.jobx")
        print("Job has been saved.")
        """

        """
        # Load job
        print("Loading job...")
        await camera.load_job("MyJob2.jobx")
        print("Job has been loaded.")
        """

        """
        # Send ready
        print("Sending ready...")
        await camera.ready()
        print("Ready sent.")
        """

        """
        # Get session IDs        
        print("Getting session IDs...")
        sessionIDs = await camera.session_IDs() 
        print(f"Session IDs: {sessionIDs}")
        """

        """
        # Get job validation state
        print("Getting job validation state...")  
        jobValidationState = await camera.job_validation_state()
        print(f"Job Validation State: {jobValidationState}")
        """
        
        """
        # Get system validation flag        
        print("Getting system validation flag...")  
        systemValidationFlag = await camera.system_validation_flag()
        print(f"System Validation Flag: {systemValidationFlag}")    
        """
        
        """
        # Run job validation
        print("Running job validation...")
        await camera.run_job_validation()
        print("Job validation run.")
        """
        
        """
        # Cancel job validation
        print("Canceling job validation...")
        await camera.cancel_job_validation()
        print("Job validation canceled.")
        """

        """
        # Get/set keep alive interval
        print("Getting keep alive interval...")
        keepAliveInterval = await camera.get_keep_alive_interval()
        print(f"Keep Alive Interval: {keepAliveInterval} ms")
        print("Setting keep alive interval to 50 ms...")
        await camera.set_keep_alive_interval(50)
        """

        """
        # Load image
        print("Loading image...")
        await camera.load_image(r"C:\test\myImage.bmp", "myImage")
        print("Image loaded.")
        """
    
        
    finally:
        # Disconnect
        print("Disconnecting...")
        await camera.disconnect()
        print("Disconnected.")

if __name__ == "__main__":
    asyncio.run(main())
