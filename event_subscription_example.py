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

    try:
        print("Connecting to camera...")
        await camera.connect_async()
        print("Connected.")
        await camera.ready_async()

        # Loop
        while True:
            async with state_lock:
                
                if state.state_changed:
                    #print("MAIN saw state change:", state.last_state)
                    state.state_changed = False

                if state.result_changed:
                    await camera.ready_async()
                    #print("MAIN saw result:", state.last_result)
                    state.result_changed = False
                    
                if state.liveMode_changed:
                    #print("MAIN saw live mode change:", state.last_liveMode_result)
                    state.liveMode_changed = False       
                    
                if state.job_changed:
                    #print("MAIN saw job change:", state.last_job_result)
                    state.job_changed = False         
                    
                if state.editorAttached:
                    #print("MAIN saw editor attached change:", state.last_editorAttached_result)
                    state.editorAttached = False   
                    
                if state.jobLoading_changed:
                    #print("MAIN saw job loading change:", state.last_jobLoading_result)
                    state.jobLoading_changed = False

            await asyncio.sleep(0.5)
            
    finally:
        print("Disconnecting...")
        await camera.disconnect_async()
        print("Disconnected")

if __name__ == "__main__":
    asyncio.run(main())
