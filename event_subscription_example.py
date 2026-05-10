# An example of using the CognexCamera class with async/await in Python.
import asyncio
import json
from cognex_camera import CognexCamera
class CameraState:
    def __init__(self):
        self.state_changed = False
        self.last_state = None

        self.result_changed = False
        self.last_result = None
        
        self.liveMode_changed = False
        self.last_liveMode_result = None
        
        self.job_changed = False
        self.last_job_result = None
        
        self.editorAttached = False
        self.last_editorAttached_result = None
        
        self.jobLoading_changed = False
        self.last_jobLoading_result = None
        
        self.settings_changed = False
        self.last_settings_result = None
        
        self.jobLoadFailed = False
        self.last_jobLoadFailed_result = None
        
        self.jobValidationDone = False
        self.last_jobValidationDone_result = None
        
        self.sessionDisposed = False
        self.last_sessionDisposed_result = None

state = CameraState()

# Prevents race conditions
state_lock = asyncio.Lock()

# Event handlers
async def state_changed_handler(*args):
    async with state_lock:
        #print(f"STATE EVENT: {args}")
        state.state_changed = True
        state.last_state = args

async def result_changed_handler(*args):
    async with state_lock:
        #print(f"RESULT EVENT: {args}")
        state.result_changed = True
        state.last_result = args
        
async def liveMode_changed_handler(*args):
    async with state_lock:
        #print(f"LIVEMODE EVENT: {args}")
        state.liveMode_changed = True
        state.last_liveMode_result = args
        
async def job_changed_handler(*args):
    async with state_lock:
        #print(f"JOB EVENT: {args}")
        state.job_changed = True
        state.last_job_result = args
        
async def editorAttached_handler(*args):
    async with state_lock:
        #print(f"EDITOR ATTACHED EVENT: {args}")
        state.editorAttached = True
        state.last_editorAttached_result = args
        
async def jobLoading_changed_handler(*args):
    async with state_lock:
        #print(f"JOB LOADING EVENT: {args}")
        state.jobLoading_changed = True
        state.last_jobLoading_result = args
        
async def settings_changed_handler(*args):
    async with state_lock:
        # print(f"SETTINGS CHANGED EVENT: {args}")
        state.settings_changed = True
        state.last_settings_result = args 
        
async def jobLoadFailed_handler(*args):
    async with state_lock:
        # print(f"JOB LOAD FAILED EVENT: {args}")
        state.jobLoadFailed = True
        state.last_jobLoadFailed_result = args
        
async def jobValidationDone_handler(*args):
    async with state_lock:
        # print(f"JOB VALIDATION DONE EVENT: {args}")
        state.jobValidationDone = True
        state.last_jobValidationDone_result = args
        
async def sessionDisposed_handler(*args):
    async with state_lock:
        # print(f"SESSION DISPOSED EVENT: {args}")
        state.sessionDisposed = True
        state.last_sessionDisposed_result = args
            
async def main():
    # Create camera
    camera = CognexCamera(ip='192.168.0.74')

    # Subscribe to events
    camera.on_state_changed.append(state_changed_handler)
    camera.on_result_changed.append(result_changed_handler)
    camera.on_liveMode_changed.append(liveMode_changed_handler)
    camera.on_job_changed.append(job_changed_handler)
    camera.on_editorAttached.append(editorAttached_handler)
    camera.on_jobLoading_changed.append(jobLoading_changed_handler)
    camera.on_settings_changed.append(settings_changed_handler)
    camera.on_jobLoadFailed.append(jobLoadFailed_handler)
    camera.on_jobValidationDone.append(jobValidationDone_handler)
    camera.on_sessionDisposed.append(sessionDisposed_handler)

    try:
        # Connect
        print("Connecting to camera...")
        await camera.connect_async()
        print("Connected.")
        await camera.ready_async()

        # Loop
        while True:
            async with state_lock:
                
                if state.settings_changed:
                    # print("MAIN saw settings change:", state.last_settings_result)
                    state.settings_changed = False
                    
                if state.jobValidationDone:
                    # print("MAIN saw job validation done:", state.last_jobValidationDone_result)
                    state.jobValidationDone = False
                    
                if state.sessionDisposed:
                    # print("MAIN saw session disposed:", state.last_sessionDisposed_result)
                    state.sessionDisposed = False
                
                if state.state_changed:
                    # print("MAIN saw state change:", state.last_state)
                    state.state_changed = False

                if state.liveMode_changed:
                    #print("MAIN saw live mode change:", state.last_liveMode_result)
                    state.liveMode_changed = False       
                    
                if state.job_changed:
                    # print("MAIN saw job change:", state.last_job_result)
                    state.job_changed = False      
                    
                if state.jobLoadFailed:
                    # print("MAIN saw job load failed:", state.last_jobLoadFailed_result)
                    state.jobLoadFailed = False   
                    
                if state.editorAttached:
                    # print("MAIN saw editor attached change:", state.last_editorAttached_result)
                    state.editorAttached = False   
                    
                if state.jobLoading_changed:
                    # print("MAIN saw job loading change:", state.last_jobLoading_result)
                    state.jobLoading_changed = False
                    
                if state.result_changed:
                    await camera.ready_async()
                    # print("MAIN saw result:", state.last_result)
                    state.result_changed = False
                    
                    # Get a cell value
                    try:
                        value = next(
                            (
                                cell["data"]
                                for cell in state.last_result[0]["cells"]
                                if cell["location"] == "B16"
                            ),
                            None
                        )

                        # print("Value at B16:", value)

                    except (TypeError, KeyError, IndexError):
                        print("Value at B16: None")  
                        
            # Don't hammer your CPU, a sleep will not miss events, no matter how long it is.
            # All events will be queued and dequeued in order.
            await asyncio.sleep(0.1)
            
    finally:
        print("Disconnecting...")
        await camera.disconnect_async()
        print("Disconnected")

if __name__ == "__main__":
    asyncio.run(main())
