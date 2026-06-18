# An example of using the CognexCamera class with async/await in Python.
import asyncio
import json
from cognex_camera import CognexCamera
from cam_scanner import CogScanner

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
        await camera.SetLiveMode(True)
        print("Live mode enabled.")
        print("Going !live...")
        await camera.SetLiveMode(False)
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

        """
        # Get startup job
        print("Getting startup job...")
        resp = await camera.GetStartupJob()
        print(f"Startup job: {resp}")
        """
        
        """
        # Set startup job
        print("Setting startup job...")
        await camera.SetStartupJob("aaa.jobx")
        print("Startup job set.")
        """

        '''
        # Get all cell names
        print("Getting all cell names...")
        resp = await camera.GetAllCellNames()
        print(resp)
        '''
        
        '''
        # Set cell name
        print("Setting cell name...")
        await camera.SetCellName("B16","IWishIWasB15")
        print("Cell name set.")
        '''
        
        '''
        # Create new job
        print("Creating new job...")
        await camera.CreateNewJob()
        print("New job created.")
        '''
        
        """
        # Set camera to startup online
        print("Setting camera to startup online...")
        await camera.StartupOnline(True)
        print("Camera set to startup online.")
        """
    
        """
        # Check startup online status
        print("Checking startup online status...")
        resp = await camera.StartupOnlineStatus()
        print(resp)
        """

        """
        # Perform some operations with the camera
        print("Getting cell condition for B16...")
        resp = await camera.GetCellCondition("B16")
        print(resp)
        """
        
        """
        # Set the condition for a cell
        print("Setting cell condition")
        await camera.SetCellCondition("A2", "$B$13")
        print("Cell condition set")
        """

        """
        # Discover Cognex Device IP/MAC and ISVS Device Details
        # Create scanner and scan the network.
        scanner = CogScanner(timeout=15, max_workers=100)
        results = scanner.scan("192.168.0.0/24", "00:D0:24")
    
        # Object to store final results.
        results_list = []
    
        # Process each result.  Attempt to connect to each camera and retrieve its information.
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
    
        # Print a specific cameras results        
        # print(results_list[0])
        # Print the IP of the first camera
        # print(results_list[0]["ip"])
        # Or loop through all results
        for result in results_list:
            print(result)
        """

        """
        # Get live mode
        print("getting live mode")
        resp = await camera.GetLiveMode()
        print(resp)
        """
    
    finally:
        # Disconnect
        print("Disconnecting...")
        await camera.Disconnect()
        print("Disconnected.")

if __name__ == "__main__":
    asyncio.run(main())
