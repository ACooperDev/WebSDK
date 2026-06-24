// An example of implemting a Cognex camera interface using the WebAPI in JavaScript.

class CogSocket {
    constructor(websocketUri) {
        this.uri = websocketUri;
        this.websocket = null;
        this.requestCounter = 0;
        
        // Maps request ID to the Promise resolve function
        this.pendingRequests = new Map(); 
        this.listeners = new Map();
        this.space = null;
        // Can be set externally (e.g., console.log)
        this.log = null; 
        
        // Event callbacks
        this.onOpen = null;
        this.onError = null;
        this.onClose = null;
    }

    Connect() {
        return new Promise((resolve, reject) => {
            try {
                this.websocket = new WebSocket(this.uri);

                this.websocket.onopen = () => {
                    if (this.onOpen) this.onOpen();
                    resolve();
                };

                this.websocket.onmessage = (event) => {
                    this._handleMessage(event.data);
                };

                this.websocket.onerror = (error) => {
                    if (this.onError) this.onError(error);
                    reject(new Error(`WebSocket Error: ${error.message || 'Unknown error'}`));
                };

                this.websocket.onclose = (event) => {
                    if (this.onClose) this.onClose(event);
                };
            } catch (e) {
                if (this.onError) this.onError(e);
                throw e;
            }
        });
    }

    _handleMessage(message) {
        if (this.log) {
            //this.log(`Received: ${message}`);
        }
        
        let msg;
        try {
            msg = JSON.parse(message);
        } catch (e) {
            // JSON Decode Error
            return;
        }
        
        const msgType = msg['$type'];
        const path = msg['path'];

        if (msgType === 'resp') {
            const reqId = msg.id;
            const pending = this.pendingRequests.get(reqId);
            
            if (pending) {
                const resolveFn = pending.resolve;
                const rejectFn = pending.reject;
                
                this.pendingRequests.delete(reqId);

                if (msg.error) {
                    const errorMsg = msg.body || String(msg.error);
                    const error = new Error(errorMsg);
                    rejectFn(error);
                } else {
                    // Attempt to cast body, assuming it should resolve to data.
                    try {
                        const body = msg.body ? JSON.parse(msg.body) : null;
                        resolveFn(body);
                    } catch(e) {
                        resolveFn(msg.body); // Resolve with raw body if JSON parsing fails
                    }
                }
            }
        } else if (msgType === 'event') {
            const body = msg.body;
            
            if (path && this.listeners.has(path)) {
                const listeners = this.listeners.get(path);
                
                // Determine arguments based on body structure
                let args = [];
                if (Array.isArray(body)) {
                    args = body;
                } else if (body !== undefined && body !== null) {
                    args = [body];
                } else {
                    args = [];
                }

                // Execute all registered listeners for this path
                for (const listener of listeners) {
                    try {
                        // In JS, we await coroutines/async functions
                        if (typeof listener.then === 'function') {
                            // Await the asynchronous listener callback
                            listener(...args);
                        } else {
                            // Execute synchronous listener
                            listener(...args);
                        }
                    } catch (e) {
                        console.error(`Error executing listener for ${path}:`, e);
                    }
                }
            }
        }
    }

    _nextRequestId() {
        this.requestCounter = (this.requestCounter + 1) % 0x7FFFFFFF;
        return this.requestCounter;
    }

    async _sendRequest(type_, path, body = null) {
        const reqId = this._nextRequestId();
        
        const promise = new Promise((resolve, reject) => {
            this.pendingRequests.set(reqId, { resolve: resolve, reject: reject });
        });

        const msg = {
            '$type': type_,
            id: reqId,
            path: path
        };
        
        if (body !== null) {
            msg.body = body;
        }
        
        const jsonMsg = JSON.stringify(msg);

        if (this.log) {
            this.log(`Send: ${jsonMsg}`);
        }

        if (!this.websocket || this.websocket.readyState !== WebSocket.OPEN) {
            const err = new Error('WebSocket is not open, cannot send request.');
            this.pendingRequests.delete(reqId);
            throw err;
        }

        this.websocket.send(jsonMsg);
        return promise;
    }

    get(path) {
        return this._sendRequest('get', path);
    }

    put(path, data) {
        return this._sendRequest('put', path, data);
    }

    post(path, data) {
        return this._sendRequest('post', path, data);
    }

    async addListener(path, listener) {
        if (!this.listeners.has(path)) {
            this.listeners.set(path, []);
            await this._sendRequest('listen', path);
        }
        this.listeners.get(path).push(listener);
    }

    async removeListener(path, listener = null) {
        if (!this.listeners.has(path)) return;
        
        const currentListeners = this.listeners.get(path);

        if (listener) {
            const index = currentListeners.indexOf(listener);
            if (index > -1) {
                currentListeners.splice(index, 1);
            }
        } else {
            // Removing all listeners
            this.listeners.delete(path);
        }

        if (!this.listeners.has(path)) {
            await this._sendRequest('unlisten', path);
        }
    }
 
    async close() {
        if (this.websocket) {
            this.websocket.close();
            this.websocket = null;
        }
    }
}

 //CognexCamera Class
class CognexCamera {

    constructor(ip, port = 80, username = 'admin', password = '') {
        this.ip = ip;
        this.port = port;
        this.username = username;
        this.password = password;
        this.cogsock = null;
        this.sessionId = null;
        this.keepAliveTimer = null;
        this.root = 'cam0/hmi';
        this.cells = 'A0:Z100';
        
        // Event subscriber lists
        this.StateChanged = [];
        this.ResultsChanged = [];
        this.LiveModeChanged = [];
        this.JobInfoChanged = [];
        this.EditorAttachedChanged = [];
        this.JobLoadingChanged = [];
        this.SettingsChanged = [];
        this.JobLoadFailed = [];
        this.JobValidationDone = [];
        this.SessionDisposed = [];

        // Initialize logging (optional)
        this.logger = (msg) => console.log(`[CogSocket] ${msg}`);
    }

    async Connect() {
        const uri = `ws://${this.ip}:${this.port}/ws`;
        this.cogsock = new CogSocket(uri);
        this.cogsock.log = this.logger;

        // Connect Socket
        await this.cogsock.Connect();
        
        // Open Session
        const sessionInfo = { cellNames: [this.cells] };
        const openSessionResponse = await this.cogsock.post(`${this.root}/openSession`, sessionInfo);
        this.sessionId = openSessionResponse && typeof openSessionResponse === 'object' && openSessionResponse.number
            ? openSessionResponse.number
            : openSessionResponse;

        if (!this.sessionId) {
            throw new Error('Unable to establish session.');
        }

        console.log(`Session ID: ${this.sessionId}`);

        // Login
        const ok = await this.cogsock.post(`${this.sessionId}/login`, [this.username, this.password, false]);
        if (ok && ok.error) {
            throw new Error("Login failed");
        }
        console.log("Login successful");

        // Add Listeners
        await this.cogsock.addListener(`${this.root}/stateChanged`, this._onStateChanged.bind(this));
        await this.cogsock.addListener(`${this.sessionId}/resultChanged`, this._onResultChanged.bind(this));
        await this.cogsock.addListener(`${this.root}/liveModeChanged`, this._onLiveModeChanged.bind(this));
        await this.cogsock.addListener(`${this.root}/jobChanged`, this._onJobChanged.bind(this));
        await this.cogsock.addListener(`${this.root}/editorAttachedChanged`, this._onEditorAttached.bind(this));
        await this.cogsock.addListener(`${this.root}/jobLoadingChanged`, this._onJobLoading_changed.bind(this));
        await this.cogsock.addListener(`${this.root}/settingsChanged`, this._onSettingsChanged.bind(this));
        await this.cogsock.addListener(`${this.root}/jobLoadFailed`, this._onJobLoadFailed_changed.bind(this));
        await this.cogsock.addListener(`${this.root}/jobValidationDone`, this._onJobValidationDone_changed.bind(this));
        await this.cogsock.addListener(`${this.root}/sessionDisposed`, this._onSessionDisposed_changed.bind(this));

        // Ready and Start Keep Alive
        await this.cogsock.post(`${this.sessionId}/ready`, null);
        this._startKeepAlive();
    }

    _startKeepAlive() {
        if (this.keepAliveTimer) {
            clearInterval(this.keepAliveTimer);
        }

        this.keepAliveTimer = setInterval(async () => {
            if (!this.sessionId || !this.cogsock) {
                return;
            }

            try {
                await this.cogsock.post(`${this.sessionId}/keepAlive`, null);
                await this.cogsock.post(`${this.sessionId}/ready`, null);
            } catch (e) {
                console.warn("Keep alive failed, possibly disconnected.", e);
                clearInterval(this.keepAliveTimer);
                this.keepAliveTimer = null;
            }
        }, 15000);
    }

    async _keepAlive() {
        while (true) {
            await new Promise(resolve => setTimeout(resolve, 15000)); // 15 seconds
            if (this.sessionId) {
                try {
                    await this.cogsock.post(`${this.sessionId}/keepAlive`, null);
                    await this.cogsock.post(`${this.sessionId}/ready`, null);
                } catch (e) {
                    console.warn("Keep alive failed, possibly disconnected.");
                    break; 
                }
            }
        }
    }

    // Events
    async _onStateChanged(...args) {
        for (const cb of this.StateChanged) {
            await cb(args);
        }
    }

    async _onResultChanged(...args) {
        this.SendReady()
        for (const cb of this.ResultsChanged ) {
            await cb(...args);
        }
    }
    
    async _onLiveModeChanged(...args) {
        for (const cb of this.LiveModeChanged) {
            await cb(...args);
        }
    }

    async _onJobChanged(...args) {
        for (const cb of this.JobInfoChanged) {
            await cb(...args);
        }
    }

    async _onEditorAttached(...args) {
        for (const cb of this.EditorAttachedChanged) {
            await cb(...args);
        }
    }

    async _onJobLoading_changed(...args) {
        for (const cb of this.JobLoadingChanged) {
            await cb(...args);
        }
    }
    
    async _onSettingsChanged(...args) {
        for (const cb of this.SettingsChanged) {
            await cb(...args);
        }
    }
    
    async _onJobLoadFailed_changed(...args) {
        for (const cb of this.JobLoadFailed) {
            await cb(...args);
        }
    }
  
    async _onJobValidationDone_changed(...args) {
        for (const cb of this.JobValidationDone) {
            await cb(...args);
        }
    }
    
    async _onSessionDisposed_changed(...args) {
        for (const cb of this.SessionDisposed) {
            await cb(...args);
        }
    }

    async Disconnect() {
        // Stop keep-alive timer
        if (this.keepAliveTimer) {
            clearInterval(this.keepAliveTimer);
            this.keepAliveTimer = null;
        }

        // Dispose session on Cognex server
        if (this.cogsock && this.sessionId) {
            try {
                console.log(`Disposing session: ${this.sessionId}`);
                await this.cogsock.post(`${this.sessionId}/dispose`, null);
            } catch (e) {
                console.error("Dispose failed:", e);
            }
        }

        // Close socket
        if (this.cogsock) {
            await this.cogsock.close();
        }

        this.sessionId = null;
        console.log("Disconnected successfully.");
    }

    // --- Camera API Methods ---

    async ManualAcquire() {
        await this.cogsock.post(`${this.sessionId}/manualTrigger`, null);
    }

    async SetLiveModeAsync(enabled) {
        await this.cogsock.put(`${this.sessionId}/liveMode`, enabled);
    }

    async ToggleOnlineOffline() {
        let resp = await this.cogsock.get(`${this.sessionId}/softOnline`);
        let currentOnline = typeof resp === 'boolean' ? resp : false;
        let newOnline = !currentOnline;
        await this.cogsock.put(`${this.sessionId}/softOnline`, newOnline);
    }

    async QueryCellResults(cell) {
        const resp = await this.cogsock.post(`${this.sessionId}/queryCellResults`, [[cell]]);
        return JSON.stringify(resp);
    }

    async GetCellExpression(cell) {
        const resp = await this.cogsock.post(`${this.sessionId}/getCellExpressions`, [cell, true]);
        return JSON.stringify(resp);
    }

    async SetCellExpression(cell, expr) {
        await this.cogsock.post(`${this.sessionId}/setCellExpression`, [cell, expr]);
    }

    async SetCellValue(cell, value) {
        await this.cogsock.post(`${this.sessionId}/setCellValue`, [cell, value]);
    }

    async ListFiles() {
        const resp = await this.cogsock.post(`${this.sessionId}/listFiles`, []);
        return JSON.stringify(resp);
    }

    async Info() {
        const resp = await this.cogsock.get(`${this.root}/info`);
        return JSON.stringify(resp);
    }

    async FindState() {
        const resp = await this.cogsock.get(`${this.root}/state`);
        return JSON.stringify(resp);
    }

    async SetSoftOnlineAsync(state) {
        await this.cogsock.put(`${this.sessionId}/softOnline`, state);
    }

    async GetJobInfo() {
        const resp = await this.cogsock.get(`${this.root}/job`);
        return JSON.stringify(resp);
    }

    async SaveJob(jobName) {
        await this.cogsock.post(`${this.sessionId}/saveJob`, jobName);
    }

    async LoadJob(jobName) {
        await this.cogsock.post(`${this.sessionId}/loadJob`, jobName);
    }

    async SendReady() {
        await this.cogsock.post(`${this.sessionId}/ready`, null);
    }

    async GetSessionIDs() {
        const resp = await this.cogsock.post(`${this.sessionId}/getSessionIDs`, null);
        return JSON.stringify(resp);
    }

    async JobValidationState() {
        const resp = await this.cogsock.get(`${this.sessionId}/jobValidationState`);
        return JSON.stringify(resp);
    }

    async SystemValidationFlag() {
        const resp = await this.cogsock.get(`${this.sessionId}/systemValidationFlag`);
        return JSON.stringify(resp);
    }
 
    async RunJobValidation() {
        await this.cogsock.post(`${this.sessionId}/runJobValidation`, null);
    }

    async CancelJobValidation() {
        await this.cogsock.post(`${this.sessionId}/cancelJobValidation`, null);
    }

    async GetKeepAliveTimeout() {
        const resp = await this.cogsock.get(`${this.root}/keepAliveTimeout`);
        return JSON.stringify(resp);
    }

    async SetKeepAliveTimeout(intervalMs) {
        const resp = await this.cogsock.put(`${this.root}/keepAliveTimeout`, intervalMs);
        return JSON.stringify(resp);
    }

    async LoadImage(fileObject) {

        const url =
            `http://${this.ip}:${this.port}/${this.sessionId}/loadImage`;

        // Convert file → base64
        const arrayBuffer = await fileObject.arrayBuffer();

        const base64 = btoa(
            String.fromCharCode(...new Uint8Array(arrayBuffer))
        );

        // Serialize string
        const jsonBody = JSON.stringify(base64);

        // Send
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: jsonBody
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        return response.json();
    }
    
    async GetStartupJob() {
        const resp =await this.cogsock.get(`${this.sessionId}/startupJob`);
        return JSON.stringify(resp);
    }

    async SetStartupJob(jobName) {
        await this.cogsock.put(`${this.sessionId}/startupJob`, jobName);
    }
    
    async GetAllCellNames() {
        const resp = await this.cogsock.post(`${this.sessionId}/getAllCellNames`);
        return JSON.stringify(resp);
    }
    
    async SetCellName(cell, name) {
        await this.cogsock.post(`${this.sessionId}/setCellName`, [cell, name]);
    }

    async CreateNewJob() {
        await this.cogsock.post(`${this.sessionId}/createNewJob`, "");
    }

    async GetCellCondition(cell){
        const resp = await this.cogsock.post(`${this.sessionId}/getCellCondition`, cell);
        return JSON.stringify(resp);
    }

    async StartupOnline(state){
        await this.cogsock.put(`${this.sessionId}/startupOnline`, state);
    }

    async StartupOnlineStatus(){
        const resp = await this.cogsock.get(`${this.sessionId}/startupOnline`);
        return JSON.stringify(resp);
    }
    
}
