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
        await camera.Connect()
        await camera.SendReady()
        print("Connected.")

        # Manual trigger
        print("Sending manual trigger...")
        await camera.ManualAcquire()
        print("Manual trigger sent.")

        """     
        # Get info
        print("Getting camera info...")
        info = await camera.Info()
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
        state = await camera.FindState()
        print(f"Camera State: {state}")
        # Boolean for online/offline status
        stateData = json.loads(state)
        print(stateData["online"])
        """
        
        """
        # List files
        print("Listing camera files...")
        files = await camera.ListFiles()
        print(f"Camera Files: {files}")
        """
        
        """
        # Set cell expression and value
        print("Setting cell B16 expression and value...")
        await camera.SetCellExpression('B16', 'EditInt(0,100)')
        await camera.SetCellValue('B16', 42)
        print("Cell B16 expression and value set.")
        """
        
        """
        # Query cell results (example)
        print("Querying cell B16 results...")
        results = await camera.QueryCellResults('B16')
        print(f"Cell B16 Results: {results}")
        """

        """
        # Get cell expressions
        print("Getting cell B16 expression...")
        expr = await camera.GetCellExpression('B16')
        print(f"Cell B16 Expression: {expr}")
        """

        """
        # Enable/disable live mode
        print("Going live...")
        await camera.SetLiveModeAsync(True)
        print("Live mode enabled.")
        print("Going !live...")
        await camera.SetLiveModeAsync(False)
        print("Live mode disabled.")
        """
        
        """
        # Go offline
        print("Going offline...")
        await camera.SetSoftOnlineAsync(False)
        print("Camera is now offline.")
        """
        
        """
        # Go online
        print("Going online...")
        await camera.SetSoftOnlineAsync(True)
        print("Camera is now online.")
        """
        
        """
        # Toggle online/offline
        print("Toggling online/offline...")
        await camera.ToggleOnlineOffline()
        print("Online/offline toggled.")
        """

        """
        # Current job info
        print("Getting current job info...")
        jobInfo = await camera.GetJobInfo()
        print(f"Job Info: {jobInfo}")
        jobInfoData = json.loads(jobInfo)
        print(jobInfoData["name"])
        """
        
        """
        # Save job
        print("Saving current job...")
        await camera.SaveJob("MyJob5.jobx")
        print("Job has been saved.")
        """

        """
        # Load job
        print("Loading job...")
        await camera.LoadJob("MyJob3.jobx")
        print("Job has been loaded.")
        """

        """
        # Send ready
        print("Sending ready...")
        await camera.SendReady()
        print("Ready sent.")
        """

        """
        # Get session IDs        
        print("Getting session IDs...")
        sessionIDs = await camera.GetSessionIDs() 
        print(f"Session IDs: {sessionIDs}")
        """

        """
        # Get job validation state
        print("Getting job validation state...")  
        jobValidationState = await camera.JobValidationState()
        print(f"Job Validation State: {jobValidationState}")
        """
        
        """
        # Get system validation flag        
        print("Getting system validation flag...")  
        systemValidationFlag = await camera.SystemValidationFlag()
        print(f"System Validation Flag: {systemValidationFlag}")    
        """
        
        """
        # Run job validation
        print("Running job validation...")
        await camera.RunJobValidation()
        print("Job validation run.")
        """
        
        """
        # Cancel job validation
        print("Canceling job validation...")
        await camera.CancelJobValidation()
        print("Job validation canceled.")
        """

        """
        # Get/set keep alive interval
        print("Getting keep alive interval...")
        keepAliveInterval = await camera.GetKeepAliveTimeout()
        print(f"Keep Alive Interval: {keepAliveInterval} ms")
        print("Setting keep alive interval to 50 ms...")
        await camera.SetKeepAliveTimeout(50)
        """

        """
        # Load image
        print("Loading image...")
        await camera.LoadImage(r"C:\\test\\myImage.bmp", "myImage")
        print("Image loaded.")
        """
        
        """
        print("Sending GI NMC")
        nmcResponse = camera.NMC("GI", 0.250, 23, camera.ip, camera.username, camera.password)
        print(f"NMC Response: {nmcResponse}")
        """
        
    finally:
        # Disconnect
        print("Disconnecting...")
        await camera.Disconnect()
        print("Disconnected.")

if __name__ == "__main__":
    asyncio.run(main())
