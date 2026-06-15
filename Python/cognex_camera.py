# An example of implementing a Cognex camera interface using the WebAPI in Python.
import asyncio
import json
import websockets
import logging
import httpx
import socket
import time

class CogSocket:
    def __init__(self, websocket_uri, root=None):
        self.uri = websocket_uri
        self.root = root
        self.websocket = None
        self.request_id = 0
        self.pending_requests = {}
        self.listeners = {}
        self.space = None
        self.log = None
        self.onopen = None
        self.onerror = None
        self.onclose = None

    # Connect to a URI.
    async def connect(self):
        try:
            self.websocket = await websockets.connect(self.uri)
            if self.onopen:
                self.onopen()
            asyncio.create_task(self._listen())
        except Exception as e:
            if self.onerror:
                self.onerror()
            raise

    # Listen for messages.
    async def _listen(self):
        try:
            async for message in self.websocket:
                await self._handle_message(message)
        except websockets.exceptions.ConnectionClosed:
            if self.onclose:
                self.onclose()

    # Handle message responses.
    async def _handle_message(self, message):
        if self.log:
            self.log(f"Received: {message}")
        try:
            msg = json.loads(message)
            msg_type = msg.get('$type')
            path = msg.get('path')
            if msg_type == 'resp':
                req_id = msg.get('id')
                pending = self.pending_requests.get(req_id)
                if pending:
                    future = pending.get('future')
                    if future:
                        if 'error' in msg:
                            error = Exception(msg.get('body', str(msg['error'])))
                            future.set_exception(error)
                        else:
                            future.set_result(msg.get('body'))
                    del self.pending_requests[req_id]
            elif msg_type == 'event':
                listeners = self.listeners.get(path, [])
                body = msg.get('body')
                if body is not None:
                    args = body if isinstance(body, list) else [body]
                else:
                    args = []
                for listener in listeners:
                    if asyncio.iscoroutinefunction(listener):
                        await listener(*args)
                    else:
                        listener(*args)
        except json.JSONDecodeError:
            pass

    # Create a unique ID.
    def _next_request_id(self):
        self.request_id = (self.request_id + 1) % 0x7FFFFFFF
        return self.request_id

    # Send a request.
    async def _send_request(self, type_, path, body=None):
        req_id = self._next_request_id()
        future = asyncio.Future()
        self.pending_requests[req_id] = {'future': future}
        msg = {
            '$type': type_,
            'id': req_id,
            'path': path
        }
        if body is not None:
            msg['body'] = body
        json_msg = json.dumps(msg, indent=self.space)
        if self.log:
            self.log(f"Send: {json_msg}")
        await self.websocket.send(json_msg)
        return await future

    # Send a GET request.
    async def get(self, path):
        return await self._send_request('get', path)

    # Send a PUT request.
    async def put(self, path, data):
        return await self._send_request('put', path, data)

    # Send a POST request.
    async def post(self, path, data):
        return await self._send_request('post', path, data)

    # Add message listeners.
    async def add_listener(self, path, listener):
        if path not in self.listeners:
            self.listeners[path] = []
            await self._send_request('listen', path)
        self.listeners[path].append(listener)

    # Remove message listeners.
    async def remove_listener(self, path, listener=None):
        if path in self.listeners:
            if listener:
                try:
                    self.listeners[path].remove(listener)
                except ValueError:
                    pass
                if not self.listeners[path]:
                    del self.listeners[path]
                    await self._send_request('unlisten', path)
            else:
                del self.listeners[path]
                await self._send_request('unlisten', path)

    # Close the connection.
    async def close(self):
        if self.websocket:
            await self.websocket.close()
            self.websocket = None

class CognexCamera:
    def __init__(self, ip, port=80, username='admin', password=''):
        self.ip = ip
        self.port = port
        self.username = username
        self.password = password
        self.cogsock = None
        self.session_id = None
        self.keep_alive_task = None
        self.root = 'cam0/hmi'
        self.cells = 'A0:Z100'
        
        # Event subscriber lists
        self.StateChanged = []
        self.ResultsChanged = []
        self.LiveModeChanged = []
        self.JobInfoChanged = []
        self.EditorAttachedChanged = []
        self.JobLoadingChanged = []
        self.SettingsChanged = []
        self.JobLoadFailed = []
        self.JobValidationDone = []
        self.SessionDisposed = []

    async def Connect(self):
        uri = f"ws://{self.ip}:{self.port}/ws"
        self.cogsock = CogSocket(uri)
        #self.cogsock.log = lambda msg: print(f"[CogSocket] {msg}")
        await self.cogsock.connect()
        # open session
        session_info = {'cellNames': [self.cells]}
        self.session_id = await self.cogsock.post(f"{self.root}/openSession", session_info)
        print(f"Session: {self.session_id}")
        # Login
        ok = await self.cogsock.post(f"{self.session_id}/login", [self.username, self.password, False])
        if isinstance(ok, dict) and ok.get('error'):
            raise Exception("Login failed")
        print("Login successful")
        # Add listeners
        await self.cogsock.add_listener(f"{self.root}/stateChanged", self._on_state_changed)
        await self.cogsock.add_listener(f"{self.session_id}/resultChanged", self._on_result_changed)
        await self.cogsock.add_listener(f"{self.root}/liveModeChanged", self._on_liveMode_changed)
        await self.cogsock.add_listener(f"{self.root}/jobChanged", self._on_job_changed)
        await self.cogsock.add_listener(f"{self.root}/editorAttachedChanged", self._on_editorAttached)
        await self.cogsock.add_listener(f"{self.root}/jobLoadingChanged", self._on_jobLoading_changed)
        await self.cogsock.add_listener(f"{self.root}/settingsChanged", self._on_settings_changed)
        await self.cogsock.add_listener(f"{self.root}/jobLoadFailed", self._on_jobLoadFailed_changed)
        await self.cogsock.add_listener(f"{self.root}/jobValidationDone", self._on_jobValidationDone_changed)
        await self.cogsock.add_listener(f"{self.root}/sessionDisposed", self._on_sessionDisposed_changed)
        # ready
        await self.cogsock.post(f"{self.session_id}/ready", "")
        # Start keep alive
        self.keep_alive_task = asyncio.create_task(self._keep_alive())

    async def _keep_alive(self):
        while True:
            await asyncio.sleep(15)
            if self.session_id:
                await self.cogsock.post(f"{self.session_id}/keepAlive", "")
                await self.cogsock.post(f"{self.session_id}/ready", "")

    async def _on_state_changed(self, *args):
        #print("State changed")

        # ONLY callbacks
        for cb in self.StateChanged:
            await cb(*args)

    async def _on_result_changed(self, *args):
        #print("Result changed")

        # ONLY callbacks
        for cb in self.ResultsChanged:
            await cb(*args)
            
    async def _on_liveMode_changed(self, *args):
        #print("Live mode changed")

        # ONLY callbacks
        for cb in self.LiveModeChanged:
            await cb(*args)

    async def _on_job_changed(self, *args):
        #print("Job changed")

        # ONLY callbacks
        for cb in self.JobInfoChanged:
            await cb(*args)

    async def _on_editorAttached(self, *args):
        #print("Editor attached")

        # ONLY callbacks
        for cb in self.EditorAttachedChanged:
            await cb(*args)

    async def _on_jobLoading_changed(self, *args):
        #print("Job loading changed")

        # ONLY callbacks
        for cb in self.JobLoadingChanged:
            await cb(*args)
            
    async def _on_settings_changed(self, *args):
        # print("Settings changed")

        # ONLY callbacks
        for cb in self.SettingsChanged:
            await cb(*args)           
            
    async def _on_jobLoadFailed_changed(self, *args):
        # print("Job load failed")

        # ONLY callbacks
        for cb in self.JobLoadFailed:
            await cb(*args)
  
    async def _on_jobValidationDone_changed(self, *args):
        # print("Job validation done")

        # ONLY callbacks
        for cb in self.JobValidationDone:
            await cb(*args)          
            
    async def _on_sessionDisposed_changed(self, *args):
        # print("Session disposed")

        # ONLY callbacks
        for cb in self.SessionDisposed:
            await cb(*args)

    async def Disconnect(self):
        try:
            # Stop keep-alive first
            if self.keep_alive_task:
                self.keep_alive_task.cancel()
                try:
                    await self.keep_alive_task
                except asyncio.CancelledError:
                    pass

            # Dispose session on Cognex server
            if self.cogsock and self.session_id:
                try:
                    print(f"Disposing session: {self.session_id}")
                    await self.cogsock.post(f"{self.session_id}/dispose", None)
                except Exception as e:
                    print(f"Dispose failed: {e}")

            # Close socket
            if self.cogsock:
                await self.cogsock.close()

        finally:
            self.keep_alive_task = None
            self.session_id = None

    # Camera functions
    async def ManualAcquire(self):
        await self.cogsock.post(f"{self.session_id}/manualTrigger", "")

    async def SetLiveModeAsync(self, enabled: bool):
        await self.cogsock.put(f"{self.session_id}/liveMode", enabled)

    async def ToggleOnlineOffline(self):
        resp = await self.cogsock.get(f"{self.session_id}/softOnline")
        current_online = resp if isinstance(resp, bool) else False
        new_online = not current_online
        await self.cogsock.put(f"{self.session_id}/softOnline", new_online)

    async def QueryCellResults(self, cell):
        resp = await self.cogsock.post(f"{self.session_id}/queryCellResults", [[cell]])
        return json.dumps(resp)

    async def GetCellExpression(self, cell):
        resp = await self.cogsock.post(f"{self.session_id}/getCellExpressions", [cell, True])
        return json.dumps(resp)

    async def SetCellExpression(self, cell, expr):
        await self.cogsock.post(f"{self.session_id}/setCellExpression", [cell, expr])

    async def SetCellValue(self, cell, value):
        await self.cogsock.post(f"{self.session_id}/setCellValue", [cell, value])

    async def ListFiles(self):
        resp = await self.cogsock.post(f"{self.session_id}/listFiles", [])
        return json.dumps(resp)

    async def Info(self):
        resp = await self.cogsock.get(f"{self.root}/info")
        return json.dumps(resp)

    async def FindState(self):
        resp = await self.cogsock.get(f"{self.root}/state")
        return json.dumps(resp)

    async def SetSoftOnlineAsync(self, enabled: bool):
        await self.cogsock.put(f"{self.session_id}/softOnline", enabled)

    async def GetJobInfo(self):
        resp = await self.cogsock.get(f"{self.root}/job")
        return json.dumps(resp)
    
    async def SaveJob(self, job_name):
        await self.cogsock.post(f"{self.session_id}/saveJob", job_name)

    async def LoadJob(self, job_name):
        await self.cogsock.post(f"{self.session_id}/loadJob", job_name)

    async def SendReady(self):
        await self.cogsock.post(f"{self.session_id}/ready", "")

    async def GetSessionIDs(self):
        resp = await self.cogsock.post(f"{self.session_id}/getSessionIDs", "")
        return json.dumps(resp)

    async def JobValidationState(self):
        resp = await self.cogsock.get(f"{self.session_id}/jobValidationState")
        return json.dumps(resp)
    
    async def SystemValidationFlag(self):
        resp = await self.cogsock.get(f"{self.session_id}/systemValidationFlag")
        return json.dumps(resp)
    
    async def RunJobValidation(self):
        await self.cogsock.post(f"{self.session_id}/runJobValidation", "")
        
    async def CancelJobValidation(self):
        await self.cogsock.post(f"{self.session_id}/cancelJobValidation", "")

    async def GetKeepAliveTimeout(self):
        resp = await self.cogsock.get(f"{self.root}/keepAliveTimeout")
        return json.dumps(resp)
    
    async def SetKeepAliveTimeout(self, interval_ms):
        resp = await self.cogsock.put(f"{self.root}/keepAliveTimeout", interval_ms)
        return json.dumps(resp)

    async def LoadImage(self, filename, image_name=None):
        # Read file as bytes
        with open(filename, "rb") as f:
            bytes_data = f.read()

        # Build URL
        url = f"http://{self.ip}:{self.port}/{self.session_id}/loadImage"

        timeout = httpx.Timeout(30.0)

        async with httpx.AsyncClient(timeout=timeout) as client:

            # ByteArrayContent + Content-Type header
            resp = await client.post(url, content=bytes_data, headers={"Content-Type": "image/bmp"})

            # EnsureSuccessStatusCode()
            resp.raise_for_status()

    def NMC(self, nmc, timeout, port, ip, username, password):
        sock = socket.create_connection((ip, port))
        sock.settimeout(timeout)

        # Read initial data
        data = sock.recv(4096)
        # print(repr(data.decode('ascii', errors='ignore')))

        # Login
        sock.sendall((username + "\r\n" + password + "\r\n").encode("ascii"))

        time.sleep(timeout)

        # Read response
        data = sock.recv(4096)
        # print(repr(data.decode('ascii', errors='ignore')))

        # Send command
        sock.sendall((nmc + "\r\n").encode("ascii"))

        time.sleep(timeout)

        # Read response
        response = ""
        while True:
            try:
                data = sock.recv(4096)
                if not data:
                    break
                response += data.decode('ascii', errors='ignore')
            except socket.timeout:
                break
            
        sock.close()   
        
        return response

    # Get the startup job
    async def GetStartupJob(self):
        resp = await self.cogsock.get(f"{self.session_id}/startupJob")
        return json.dumps(resp)
    
    # Set the startup job
    async def SetStartupJob(self, job_name):
        await self.cogsock.put(f"{self.session_id}/startupJob", job_name)

    # Get all cell names
    async def GetAllCellNames(self):
        resp = await self.cogsock.post(f"{self.session_id}/getAllCellNames", "")
        return json.dumps(resp)
        
    # Set cell name
    async def SetCellName(self, cell, name):
        await self.cogsock.post(f"{self.session_id}/setCellName", [cell, name])
        
   # Create new job
    async def CreateNewJob(self):
        await self.cogsock.post(f"{self.session_id}/createNewJob", "")   

    # Startup camera online
    async def StartupOnline(self):
       await self.cogsock.put(f"{self.session_id}/startupOnline","data")   
       
    # Get startup online status
    async def StartupOnlineStatus(self):
        resp = await self.cogsock.get(f"{self.session_id}/startupOnline")
        return json.dumps(resp)
