using Cognex.InSight.Remoting.Serialization;
using Cognex.SimpleCogSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;

namespace Cognex.InSight.Web
{
    /// <summary>
    /// A class that holds a connection to a camera and provides an abstraction for the HMI API.
    /// 
    /// When the Connect method of this class is called, and HmiSession is created and the
    /// user is logged in. Calling the SendReady method will update the Results.
    /// </summary>
    public class CvsInSight
    {
        #region Fields
        // The IP adress and port for the current/last connection
        private string _ipAddress = "127.0.0.1:8087";
        // This is the _ipAddress with "ws://" prepended
        private string _connectionString;

        // The username for the current/last connection
        private string _username = "admin";
        // The password for the current/last connection
        private string _password = "";

        // The common path to identify the root. Either "system" or "cam0/hmi" will be returned.
        private string _hmiRootQueryPath = "/system/paths/hmi";
        // This is set to either "/system/" or "/cam0/hmi/".
        private string _rootPath;

        private HttpClient _httpClient;
        private string _httpRequestRoot = "/";
        private bool _usesXYCoordinates = false;

        #region Web API Paths
        // HmiService Resources:
        private const string _openSessionPath = "openSession";
        private const string _editorAttachedPath = "editorAttached";
        private const string _infoPath = "info";
        private const string _jobPath = "job";
        private const string _jobLoadingPath = "jobLoading";
        private const string _loadJobDataPath = "loadJobData";
        private const string _loadImagePath = "loadImage";
        private const string _loadJobPath = "loadJob";
        private const string _saveJobPath = "saveJob";
        private const string _saveJobDataPath = "saveJobData";
        private const string _jobNamePath = "job/name";
        private const string _settingsPath = "settings";
        private const string _statePath = "state";
        private const string _stateChangedPath = "stateChanged";
        private const string _editorAttachedChangedPath = "editorAttachedChanged";
        private const string _liveModeChangedPath = "liveModeChanged";
        private const string _jobInfoChangedPath = "jobChanged";
        private const string _jobLoadingChangedPath = "jobLoadingChanged";
        private const string _jobLoadFailedPath = "jobLoadFailed";
        private const string _settingsChangedPath = "settingsChanged";
        private const string _sessionDisposedPath = "sessionDisposed";
        private const string _keepAliveTimeout = "keepAliveTimeout";
        private const string _jobValidationDonePath = "jobValidationDone";
        
        // HmiSession Resources:
        private const string _loginPath = "login";
        private const string _readyPath = "ready";
        private const string _keepAlivePath = "keepAlive";
        private const string _manualTriggerPath = "manualTrigger";
        private const string _softOnlinePath = "softOnline";
        private const string _liveModePath = "liveMode";
        private const string _setCellExpressionPath = "setCellExpression";
        private const string _getCellExpressionPath = "getCellExpression";
        private const string _setCellValuePath = "setCellValue";
        private const string _getAllCellNamesPath = "getAllCellNames";
        private const string _getLatestResultPath = "getLatestResult";
        private const string _disposePath = "dispose";
        private const string _beginEditPath = "beginEdit";
        private const string _endEditPath = "endEdit";
        private const string _resultChangedPath = "resultChanged";
        private const string _customViewSettingsPath = "setCustomViewList";
        private const string _oldCustomViewSettingsPath = "setCustomView";
        private const string _hmiSettingsPath = "settings/hmi";
        private const string _hmiSettingsSavePath = "settings/hmi/save";
        private const string _freezeQueuePath = "rq/frozen";
        private const string _queueSlotIndexPath = "rq/slotIndex";
        private const string _getQueuedImageUrlPath = "rq/getImageUrl";
        private const string _sheetFormatPath = "job/getSheetFormat";
        private const string _listFilesPath = "listFiles";
        private const string _sessionIDs = "getSessionIDs";
        private const string _jobValidationState = "jobValidationState";
        private const string _systemValidationFlag = "systemValidationFlag";
        private const string _runJobValidation = "runJobValidation";
        private const string _cancelJobValidation = "cancelJobValidation";

        #endregion

        // Manages a CogSocket connection
        private CogSocket _cogSocket;

        // Holds information about the camera
        private JToken _cameraInfo;
        private CvsCameraInfo _cvsCameraInfo;
        // Holds the job info (Not the cells, but the other information stored with the job)
        private JToken _jobInfo;
        private HmiCustomViewSettings[] _customViewSettingsList;
        // Holds the camera settings
        private JToken _settings;
        // Holds the format for the sheet and all of the cells
        private HmiSheetFormat _sheetFormat = new HmiSheetFormat();

        // A Timer that send the keepAlive message for the HmiSession
        private System.Timers.Timer _keepAliveTimer = new System.Timers.Timer();
        private const int _keepAliveInterval = 10000;

        // The string that identifies the session
        private string _sessionID;
        private string _sessionIDPath; // The Session ID combined with "/" at the end

        // Holds the last results received. Calling "SendReady" will raise a ResultChanged event and update these results.
        private JToken _results = null;

        private CvsCogViewRecord _resultsViewRecord = null;

        // The CogSocket connected state
        private bool _isConnected = false;
        private bool _isConnecting = false;
        // The online/offline state
        private bool _isOnline = false;
        // The flag that designates when live mode is enabled
        private bool _isLiveMode = false;
        // The flag that designates whether softOnline is enabled
        private bool _isSoftOnline = false;
        // The flag that designates whether nativeOnline is enabled
        private bool _isNativeOnline = true;
        // The flag that designates whether discreteOnline is enabled
        private bool _isDiscreteOnline = true;
        // The flag that designates whether FfpOnline is enabled
        private bool _isFfpOnline = true;

        // A flag that designates whether a job is currently loading
        private bool _jobLoading = false;
        // A flag that designates whether an editor (like ISE) is attached and may block certain operations
        private bool _editorAttached = false;

        // The string that represents the access level (i.e. "full", "protected", "locked")
        private string _accessLevel = "";

        #endregion

        static Cognex.SimpleCogSocket.JsonSerializer _sJsonSerializer;

        public static Cognex.SimpleCogSocket.JsonSerializer JsonSerializer { get { return _sJsonSerializer; } }

        static CvsInSight()
        {
            _sJsonSerializer = new Cognex.SimpleCogSocket.JsonSerializer(false);
        }

        /// <summary>
        /// Constructs a new instance.
        /// 
        /// The object contructed is not connected; Call "Connect" to open the connection.
        /// </summary>
        public CvsInSight()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>The <c>ResultsChanged</c> event is raised when the <see cref="Results"/> property has changed.</summary>
        public event EventHandler ResultsChanged;

        /// <summary>
        /// The <c>ConnectedChanged</c> event is raised when the <see cref="Connected"/> property changed.
        /// </summary>
        public event EventHandler ConnectedChanged;

        /// <summary>
        /// The <c>ConnectingChanged</c> event is raised when the <see cref="Connecting"/> property changed.
        /// </summary>
        public event EventHandler ConnectingChanged;

        /// <summary>
        /// The <c>StateChanged</c> event is raised when any of the state-related properties changed.
        /// 
        /// The related properties include:
        /// <see cref="Online"/>, <see cref="SoftOnline"/>, <see cref="NativeOnline"/>, <see cref="DiscreteOnline"/>
        /// </summary>
        public event EventHandler StateChanged;

        /// <summary>The <c>LiveModeChanged</c> event is raised when the <see cref="LiveMode"/> property changed.</summary>
        public event EventHandler LiveModeChanged;

        /// <summary>The <c>JobInfoChanged</c> event is raised when the <see cref="JobInfo"/> property changed.</summary>
        public event EventHandler JobInfoChanged;

        /// <summary>The <c>SettingsChanged</c> event is raised when the <see cref="Settings"/> property changed.</summary>
        public event EventHandler SettingsChanged;

        /// <summary>The <c>JobLoadingChanged</c> event is raised when the <see cref="JobLoading"/> property changed.</summary>
        public event EventHandler JobLoadingChanged;

        /// <summary>The <c>JobLoadFailed</c> event is raised when the a job load operation has failed.</summary>
        public event EventHandler JobLoadFailed;

        /// <summary>The <c>EditorAttachedChanged</c> event is raised when the <see cref="EditorAttached"/> property changed.</summary>
        public event EventHandler EditorAttachedChanged;

        /// <summary>The <c>SessionDisposed</c> event is raised when the a senssion has been disposed.</summary>
        public event EventHandler SessionDisposed;

        /// <summary>
        /// This event may be used for diagnostic purposes to trace all messages.
        /// </summary>
        public event EventHandler<MessagePayloadPreviewEventArgs> PreviewMessage;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler JobValidationDone;


        /// <summary>
        /// Gets the IP adress and port for the current/last connection.
        /// </summary>
        public string RemoteIPAddress
        {
            get { return _ipAddress; }
        }

        public string RemoteIPAddressUrl
        {
            get { return "http://" + _ipAddress; }
        }

        /// <summary>
        /// Gets the username for the current/last connection.
        /// </summary>
        public string Username
        {
            get { return _username; }
        }

        /// <summary>
        /// Gets the password for the current/last connection.
        /// </summary>
        public string Password
        {
            get { return _password; }
        }

        /// <summary>
        /// Gets the SoftOnline flag.
        /// 
        /// Note: The camera will not go online unless SoftOnline, NativeOnline, and DiscreteOnline are all true.
        /// </summary>
        public bool SoftOnline
        {
            get { return _isSoftOnline; }
        }

        /// <summary>
        /// Sets the SoftOnline flag.
        /// 
        /// Note: The camera will not go online unless SoftOnline, NativeOnline, DiscreteOnline, and FfpOnline are all true.
        /// </summary>
        /// <param name="value">Set to true to go online</param>
        public async Task<object> SetSoftOnlineAsync(bool value)
        {
            if (_isSoftOnline != value)
            {
                try
                {
                    var resp = await _cogSocket.PutAsync(_sessionIDPath + _softOnlinePath, value);
                    return resp;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SoftOnline Error: {ex.InnerException}");
                    throw;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets NativeOnline flag that designates whether Native Mode is preventing the camera from going online.
        /// 
        /// Note: The camera will not go online unless SoftOnline, NativeOnline, DiscreteOnline, and FfpOnline are all true.
        /// </summary>
        public bool NativeOnline
        {
            get { return _isNativeOnline; }
        }

        /// <summary>
        /// Gets DiscreteOnline flag that designates whether Discrete I/O is preventing the camera from going online.
        /// 
        /// Note: The camera will not go online unless SoftOnline, NativeOnline, DiscreteOnline, and FfpOnline are all true.
        /// </summary>
        public bool DiscreteOnline
        {
            get { return _isDiscreteOnline; }
        }

        /// <summary>
        /// Gets the FfpOnline flag that designates whether FFP is preventing the camera from going online.
        /// 
        /// Note: The camera will not go online unless SoftOnline, NativeOnline, DiscreteOnline, and FfpOnline are all true.
        /// </summary>
        public bool FfpOnline
        {
            get { return _isFfpOnline; }
        }

        /// <summary>
        /// Gets the connected state that is true when the CogSocket connection is open.
        /// </summary>
        public bool Connected
        {
            get { return _isConnected; }
        }

        /// <summary>
        /// Gets the connected state that is true when the CogSocket connection is in the process of opening.
        /// </summary>
        public bool Connecting
        {
            get { return _isConnecting; }
        }

        /// <summary>
        /// Gets a flag that designates whether the camera is online.
        /// </summary>
        public bool Online
        {
            get { return _isOnline; }
        }

        /// <summary>
        /// Gets or sets the flag that designates whether the camera is in Live Mode.
        /// </summary>
        public bool LiveMode
        {
            get { return _isLiveMode; }
        }

        /// <summary>
        /// Sets the Live Mode flag.
        /// 
        /// Note: The camera will not go int Live Mode unless the sensor is offline.
        /// </summary>
        /// <param name="value">Set to true to go online</param>
        public async Task<object> SetLiveModeAsync(bool value)
        {
            if (_isLiveMode != value)
            {
                try
                {
                    var resp = await _cogSocket.PutAsync(_sessionIDPath + _liveModePath, value);
                    return resp;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SetLiveMode Error: {ex.InnerException}");
                    throw;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a flag that designates whether a job is currenntly loading.
        /// </summary>
        public bool JobLoading
        {
            get { return _jobLoading; }
        }

        /// <summary>
        /// Gets a flag that designates whether an editor (like ISE) is attached and may block certain operations.
        /// </summary>
        public bool EditorAttached
        {
            get { return _editorAttached; }
        }

        /// <summary>
        /// Gets the string that represents the access level. (i.e. "full", "protected", "locked")
        /// </summary>
        public string AccessLevel
        {
            get { return _accessLevel; }
        }

        /// <summary>
        /// Gets the camera info.
        /// </summary>
        public CvsCameraInfo CameraInfo
        {
            get { return _cvsCameraInfo; }
        }

        /// <summary>
        /// Gets the job info (i.e. all properties in the 'job' node).
        /// 
        /// Note: This is not the cells, but the other information that is stored with the job.
        /// </summary>
        public JToken JobInfo
        {
            get { return _jobInfo; }
        }

        /// <summary>
        /// Gets the custom view settings that are stored in the job.
        /// </summary>
        public HmiCustomViewSettings[] CustomViewSettings
        {
            get { return _customViewSettingsList; }
        }

        /// <summary>
        /// Sets the custom view settings for the job.
        /// </summary>
        /// <param name="settings">The new settings.</param>
        public async Task<object> SetCustomViewSettingsAsync(HmiCustomViewSettings[] settings)
        {
            try
            {
                object[] args = { settings };
                var resp = await _cogSocket.PostAsync(_sessionIDPath + _customViewSettingsPath, args).ConfigureAwait(false) as string;
                return resp;
            }
            catch (Exception ex)
            {
                try
                {
                    object[] args = { settings[0] };
                    var resp = await _cogSocket.PostAsync(_sessionIDPath + _oldCustomViewSettingsPath, args).ConfigureAwait(false) as string;
                    return resp;
                }
                catch
                {
                    Debug.WriteLine($"CustomViewSettings Error: {ex.InnerException}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the settings that are stored on the camera (i.e. all properties in the 'settings' node).
        /// 
        /// Note: This is does not include items that are stored in the job.
        /// </summary>
        public JToken Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Sets the HMI settings for the camera.
        /// </summary>
        /// <param name="settings">The new settings.</param>
        public async Task<object> SetHmiSettingsAsync(HmiSettings settings)
        {
            try
            {
                var resp = await _cogSocket.PutAsync(_rootPath + _hmiSettingsPath, settings);

                // Persist them on the camera
                // Note: Cameras prior to 22.1.0 do not support this API. Settings are always saved.
                try
                {
                    await _cogSocket.PostAsync(_rootPath + _hmiSettingsSavePath, new object[0]).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    Debug.WriteLine($"HmiSettings Error: Save settings is not supported on this camera");
                }

                // Update _settings
                await RequestSettings();

                return resp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HmiSettings Error: {ex.InnerException}");
                throw;
            }
        }

        /// <summary>
        /// Gets the sheet format.
        /// </summary>
        public HmiSheetFormat SheetFormat
        {
            get { return _sheetFormat; }
        }

        /// <summary>
        /// Gets the last results received.
        /// 
        /// The contents of the object will be an HmiResult.
        /// Calling "SendReady" will raise a ResultChanged event and update these results.
        /// </summary>
        public JToken Results
        {
            get { return _results; }
        }

        /// <summary>
        /// Gets the view record for the current result.
        /// </summary>
        /// <returns>The CvsCogViewRecord or null</returns>
        public CvsCogViewRecord GetViewRecord()
        {
            return _resultsViewRecord;
        }

        /// <summary>
        /// Gets the CvsCogViewRecord for the current result
        /// </summary>
        /// <returns>The CvsCogViewRecord or null if not available</returns>
        private CvsCogViewRecord GetViewRecordFromResult()
        {
            if (_results == null)
                return null;

            JToken token = _results.SelectToken("acqImageView");
            if (token == null)
                return null;

            CvsCogViewRecord viewRecord = token.ToObject(typeof(CvsCogViewRecord)) as CvsCogViewRecord;
            return viewRecord;
        }

        /// <summary>
        /// Gets the ViewPort for the view in the results.
        /// </summary>
        public CvsCogViewPort ViewPort
        {
            get
            {
                return (_resultsViewRecord != null) ? _resultsViewRecord.Viewport : null;
            }
        }

        /// <summary>
        /// Return true if the camera supports an XY coordinate system (i.e. not row/col graphics)
        /// </summary>
        public bool UsesXYCoordinates
        {
            get { return _usesXYCoordinates; }
        }

        /// <summary>
        /// The vertical offset for rendering in the pixel space of the camera for a partially acquired image.
        /// </summary>
        public int ImageOffsetY
        {
            get
            {
                if ((_resultsViewRecord == null) || (_resultsViewRecord.Layers[0] == null))
                    return 0;

                CvsCogImageLayer imageLayer = (_resultsViewRecord.Layers[0].ToObject(typeof(CvsCogImageLayer))) as CvsCogImageLayer;
                if (imageLayer.Image == null)
                    return 0;

                // Adjust for the old coordinate system, if necessary
                return _usesXYCoordinates ? imageLayer.Image.OffsetY : imageLayer.Image.OffsetX;
            }
        }

        /// <summary>
        /// Gets the first graphics layer of the current result
        /// </summary>
        /// <returns>The layer or null</returns>
        public CvsCogGraphicsLayer GetGraphicsLayer()
        {
            CvsCogViewRecord viewRecord = _resultsViewRecord;
            if (viewRecord == null)
                return null;

            int numLayers = viewRecord.Layers.Length;
            for (int index = 0; index < numLayers; index++)
            {
                JToken layer = viewRecord.Layers[index];
                string layerType = layer["$type"].ToString();
                if (layerType == "GraphicsLayer")
                {
                    CvsCogGraphicsLayer grLayer = layer.ToObject(typeof(CvsCogGraphicsLayer)) as CvsCogGraphicsLayer;
                    return grLayer;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the job name for the job currently loaded in the device
        /// </summary>
        /// <returns>string - the job name</returns>
        public async Task<string> GetJobName()
        {
            return await _cogSocket.GetAsync(_rootPath + _jobNamePath).ConfigureAwait(false) as string;
        }

        /// <summary>
        /// Gets the graphics for the current results.
        /// </summary>
        /// <returns></returns>
        public async Task<CvsCogShape[]> GetGraphicsAsync()
        {
            JArray graphicsObj = null;
            CvsCogGraphicsLayer grLayer = GetGraphicsLayer();

            string graphicsUrl = "";
            if ((grLayer != null) && (grLayer.Url.Length > 0))
            {
                graphicsUrl = RemoteIPAddressUrl + grLayer.Url;
            }

            if (grLayer?.Graphics != null)
            {
                graphicsObj = grLayer.Graphics;
            }
            else // Remote graphics...
            {
                if (graphicsUrl.Length == 0)
                {
                    return new CvsCogShape[0];
                }

                try
                {
                    string graphics = await _httpClient.GetStringAsync(graphicsUrl).ConfigureAwait(false) as string;

                    graphicsObj = _sJsonSerializer.DeserializeObject(graphics) as JArray;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("WARNING: Graphics not retrieved: " + ex.Message);
                }
            }

            if (graphicsObj == null)
            {
                return new CvsCogShape[0];
            }

            int numShapes = graphicsObj.Count;
            CvsCogShape[] shapes = new CvsCogShape[graphicsObj.Count];
            for (int index = 0; index < numShapes; index++)
            {
                CvsCogShape shape = _sJsonSerializer.DeserializeObject(graphicsObj[index].ToString()) as CvsCogShape;
                shapes[index] = shape;
            }

            return shapes;
        }

        /// <summary>
        /// Gets the url for the main image of the current result
        /// </summary>
        /// <param name="scaledWidth"></param>
        /// <param name="scaledHeight"></param>
        /// <returns>The url or empty string if not available</returns>
        public string GetMainImageUrl(int scaledWidth = -1, int scaledHeight = -1)
        {
            if (_results == null)
                return "";

            JToken token = _results.SelectToken("acqImageView.layers[0].url");
            if (token == null)
                return "";

            string imageUrl = token.ToString();
            string sizeQueryString = "";
            if ((scaledWidth > 0) && (scaledHeight > 0))
            {
                sizeQueryString = $"?sz={scaledWidth},{scaledHeight}";
            }
            return RemoteIPAddressUrl + imageUrl + sizeQueryString;
        }

        /// <summary>
        /// Connects to the device, opening a CogSocket connection.
        /// 
        /// The method will also create an HmiSession and login to it.
        /// </summary>
        /// <param name="address">The IP address and port</param>
        /// <param name="username">The username to establish the connection</param>
        /// <param name="password">The password associated with the username</param>
        /// <param name="sessionInfo">
        /// Designates the cells that should be included in the results and other information about
        /// how the HmiSession should generate results.
        /// This may be null to designate that all cells should be returned.
        /// </param>
        /// <returns></returns>
        public async Task Connect(string address, string username, string password, HmiSessionInfo sessionInfo)
        {
            if (_isConnected)
            {
                await Disconnect();
            }

            _isConnecting = true;
            if (ConnectingChanged != null)
            {
                ConnectingChanged(this, EventArgs.Empty);
            }

            _ipAddress = address; // This should include the port
            _username = username;
            _password = password;
            _connectionString = "ws://" + _ipAddress + "/ws";

            try
            {
                try
                {
                    _cogSocket = new CogSocket(_connectionString, _sJsonSerializer.Settings);
                    _cogSocket.Closed += _cogSocket_Closed;
                    await _cogSocket.Connect().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Connection Error: {ex.Message}");
                    throw;
                }

                // Subscribes to the PreviewMessage, so that it can be forwarded on when necessary.
                var existingMessages = new Dictionary<long, CogSocketMessage>();

                _cogSocket.PreviewMessage += (sender, arg) =>
                {
                    if (this.PreviewMessage != null)
                    {
                        PreviewMessage(sender, arg);
                    }
                };

                // Get the root path
                string root = await _cogSocket.GetAsync(_hmiRootQueryPath).ConfigureAwait(false) as string;
                _rootPath = "/" + root + "/";

                // Get the camera info
                JToken infoObj = await _cogSocket.GetAsync(_rootPath + _infoPath).ConfigureAwait(false) as JToken;
                _cameraInfo = infoObj;
                _cvsCameraInfo = new CvsCameraInfo(_cameraInfo);
                Debug.WriteLine("Connected to: ");
                Debug.WriteLine("Name: " + _cvsCameraInfo.HostName);
                Debug.WriteLine("IP Address: " + _cvsCameraInfo.IPAddress);
                Debug.WriteLine("Firmware Verison: " + _cvsCameraInfo.FirmwareVersion);
                Debug.WriteLine("API Verison: " + _cvsCameraInfo.ApiVersion);

                JToken token = infoObj.SelectToken("httpRequestRoot");
                if (token != null)
                {
                    _httpRequestRoot = "/" + token.ToString() + "/";
                }
                else
                {
                    _httpRequestRoot = "/";
                }

                string[] capabilities = _cvsCameraInfo.Capabilities;
                _usesXYCoordinates = Array.Find<string>(capabilities, element => element == "xyCoordinates") != null;

                // Get the job info (Not the cells, but the other information stored with the job)
                await RequestJobInfo();

                // Assume that a job is not being loaded on connect
                _jobLoading = false;

                // Get the settings
                await RequestSettings();

                // Get the editorAttached flag
                await RequestEditorAttached();

                // Get the state
                JToken stateObj = await _cogSocket.GetAsync(_rootPath + _statePath).ConfigureAwait(false) as JToken;
                _isOnline = Convert.ToBoolean(stateObj["online"]);
                _isSoftOnline = Convert.ToBoolean(stateObj["softOnline"]);
                _isNativeOnline = Convert.ToBoolean(stateObj["nativeOnline"]);
                _isDiscreteOnline = Convert.ToBoolean(stateObj["discreteOnline"]);
                _isFfpOnline = Convert.ToBoolean(stateObj["ffpOnline"]);
                _isLiveMode = Convert.ToBoolean(stateObj["liveMode"]);

                // Open a session
                object[] args1 = { sessionInfo };
                try
                {
                    _sessionID = await _cogSocket.PostAsync(_rootPath + _openSessionPath, args1).ConfigureAwait(false) as string;
                    if ((_sessionID == null) || (_sessionID.Length == 0))
                    {
                        _cogSocket.Dispose();
                        throw new Exception("No more sessions");
                    }
                }
                catch (Exception)
                {
                    _cogSocket.Dispose();
                    throw new Exception("No more sessions");
                }
                _sessionIDPath = _sessionID + "/";

                await _cogSocket.AddListenerAsync(_rootPath + _stateChangedPath, new CogSocketEventHandler(HandleStateChanged)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_rootPath + _liveModeChangedPath, new CogSocketEventHandler(HandleLiveModeChanged)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_rootPath + _jobInfoChangedPath, new CogSocketEventHandler(HandleJobInfoChanged)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_rootPath + _settingsChangedPath, new CogSocketEventHandler(HandleSettingsChanged)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_rootPath + _jobLoadingChangedPath, new CogSocketEventHandler(HandleJobLoadingChanged)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_rootPath + _jobLoadFailedPath, new CogSocketEventHandler(HandleJobLoadFailed)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_rootPath + _editorAttachedChangedPath, new CogSocketEventHandler(HandleEditorAttachedChanged)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_rootPath + _sessionDisposedPath, new CogSocketEventHandler(HandleSessionDisposed)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_sessionIDPath + _resultChangedPath, new CogSocketEventHandler(HandleResultsChanged)).ConfigureAwait(false);
                await _cogSocket.AddListenerAsync(_rootPath + _jobValidationDonePath, new CogSocketEventHandler(HandleJobValidationDone)).ConfigureAwait(false);

                // Login
                object[] args = { username, password, false, false };
                string accessLevel = await _cogSocket.PostAsync(_sessionIDPath + _loginPath, args).ConfigureAwait(false) as string;
                if (accessLevel.Length == 0)
                {
                    _cogSocket.Dispose();
                    throw new Exception("Unable to log in");
                }
                else
                {
                    // Send Ready to get a result
                    await _cogSocket.PostAsync(_sessionIDPath + _readyPath, new object[0]).ConfigureAwait(false);
                }

                _accessLevel = accessLevel; // Save the level for the getter

                try
                {
                    _sheetFormat = (await RequestSheetFormat("A0:Z599")).ToObject<HmiSheetFormat>();
                }
                catch (Exception)
                {
                    Debug.WriteLine("Warning: Unable to read sheet format. Setting to null.");
                    _sheetFormat = new HmiSheetFormat();
                }

                _keepAliveTimer.Interval = _keepAliveInterval;
                _keepAliveTimer.Elapsed += _keepAliveTimer_Elapsed;
                _keepAliveTimer.Start();

                _isConnecting = false;
                _isConnected = true;
            }
            catch (Exception)
            {
                _isConnecting = false;
                throw;
            }
            finally
            {
                if (ConnectingChanged != null)
                {
                    ConnectingChanged(this, EventArgs.Empty);
                }

                if (ConnectedChanged != null)
                {
                    ConnectedChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Handles unexpected socket disconnects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _cogSocket_Closed(object sender, EventArgs e)
        {
            try
            {
                _cogSocket.Dispose();
                _cogSocket = null;
                // Do the other clean up after an unexpected disconnect and raise the StateChanged event
                await Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"_cogSocket_Closed: {ex.Message}");
            }
        }

        /// <summary>
        /// Closes the CogSocket connection to the camera.
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
            if (_keepAliveTimer.Enabled)
            {
                _keepAliveTimer.Stop();
            }

            if (_cogSocket != null)
            {
                _cogSocket.Closed -= _cogSocket_Closed;
                try
                {
                    await _cogSocket.PostAsync(_sessionIDPath + _disposePath, new object[0]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Session Dispose Error: {ex.Message}");
                }
                _cogSocket.Dispose();
                _cogSocket = null;
            }

            if (_isConnected)
            {
                ResetAllFields();
                if (ConnectedChanged != null)
                {
                    ConnectedChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Reset all fields on disconnect.
        /// </summary>
        private void ResetAllFields()
        {
            _results = null;
            _resultsViewRecord = null;
            _sheetFormat = new HmiSheetFormat();
            _isConnected = false;
            _isOnline = false;
            _isLiveMode = false;
            _isSoftOnline = false;
            _isNativeOnline = true;
            _isDiscreteOnline = true;
            _isFfpOnline = true;
            _jobLoading = false;
            _editorAttached = false;
            _accessLevel = "";
            _jobInfo = null;
            _customViewSettingsList = null;
        }

        private bool _skipKeepAlive = false;

        /// <summary>
        /// Sends the 'ready' message to allow the results to be updated.
        /// 
        /// Note: This performs the same operation as "AcceptUpdate" in the In-Sight SDK.
        /// </summary>
        public async Task SendReady()
        {
            _skipKeepAlive = true;
            // Accept the next result
            await _cogSocket.PostAsync(_sessionIDPath + _readyPath, new object[0]).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a request to manually trigger a new acquisition.
        /// </summary>
        public async Task ManualAcquire()
        {
            await _cogSocket.PostAsync(_sessionIDPath + _manualTriggerPath, new object[0]).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the new cell value for a cell.
        /// 
        /// Note: This replaces these ISDK methods:
        /// ClickButton, SetCheckBox, SetFloat, SetInteger, SetListBoxIndex, SetString, SetEditGraphic
        /// </summary>
        /// <param name="cellName">The name of the cell or the location</param>
        /// <param name="value">The value of the cell</param>
        public async Task SetCellValue(string cellName, object value)
        {
            object[] args = { cellName, value };
            await _cogSocket.PostAsync(_sessionIDPath + _setCellValuePath, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the new expression for a cell.
        /// 
        /// Note: This replaces these ISDK method: SetExpression
        /// </summary>
        /// <param name="cellName">The name of the cell or the location</param>
        /// <param name="expr">The expresison for  the cell</param>
        public async Task SetCellExpression(string cellName, object value)
        {
            object[] args = { cellName, value };
            await _cogSocket.PostAsync(_sessionIDPath + _setCellExpressionPath, args).ConfigureAwait(false);
            // Be sure that we get updated results
            object result = await _cogSocket.PostAsync(_sessionIDPath + _getLatestResultPath, args).ConfigureAwait(false);

            UpdateResults((JToken)result);
        }

        /// <summary>
        /// Gets the expression for a cell.
        /// </summary>
        /// <param name="cellName">The name of the cell or the location</param>
        public async Task<string> GetCellExpression(string cellName)
        {
            object[] args = { cellName };
            return await _cogSocket.PostAsync(_sessionIDPath + _getCellExpressionPath, args).ConfigureAwait(false) as string;
        }

        /// <summary>
        /// Begins an edit operation for a cell with an edit graphic.
        /// 
        /// This method should be called before setting the cell value for the graphic.
        /// When complete, the EndEdit method should be called.
        /// </summary>
        /// <param name="cellLocation">The location of the cell that generate the edit graphic</param>
        /// <returns>A JToken with the ViewRecord for editing the graphic</returns>
        public async Task<JToken> BeginEdit(string cellLocation)
        {
            object[] args = { cellLocation };
            return await _cogSocket.PostAsync(_sessionIDPath + _beginEditPath, args).ConfigureAwait(false) as JToken;
        }

        /// <summary>
        /// Ends an edit operation.
        /// 
        /// This method should be called after a graphic is done being edited and
        /// it is expected that beginEdit was called prior to this.
        /// </summary>
        public async Task EndEdit()
        {
            object[] args = { };
            await _cogSocket.PostAsync(_sessionIDPath + _endEditPath, args).ConfigureAwait(false);
        }


        /// <summary>
        /// Gets the latest result from the camera
        /// </summary>
        public async Task GetLatestResult()
        {
            object[] args = { };
            object result = await _cogSocket.PostAsync(_sessionIDPath + _getLatestResultPath, args).ConfigureAwait(false);

            UpdateResults((JToken)result);
        }

        /// <summary>
        /// Loads a job from the designated file on the camera.
        /// </summary>
        public async Task LoadJob(string filename)
        {
            object[] args = { filename };
            await _cogSocket.PostAsync(_sessionIDPath + _loadJobPath, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the job from the designated file on the PC, passing its data contents to the camera.
        /// </summary>
        /// <param name="filename">The fully qualified path to the job file to load</param>
        /// <returns></returns>
        public async Task LoadJobData(string filename)
        {
            byte[] bytes = File.ReadAllBytes(filename);

            HmiNamedContent namedContent = new HmiNamedContent();
            namedContent.Name = Path.GetFileName(filename);
            namedContent.Content = System.Convert.ToBase64String(bytes);

            string argsJson = _sJsonSerializer.SerializeObject(namedContent);
            byte[] encodedBytes = System.Text.Encoding.ASCII.GetBytes(argsJson);

            string url = RemoteIPAddressUrl + _httpRequestRoot + _sessionIDPath + _loadJobDataPath;

            TimeSpan timeout = TimeSpan.FromSeconds(30);

            using (MemoryStream byteStream = new MemoryStream(encodedBytes))
            {
                await MakeHttpPostRequest(url, timeout, "application/json", byteStream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Saves a job to the designated file on the camera.
        /// </summary>
        public async Task SaveJob(string filename)
        {
            object[] args = { filename };
            await _cogSocket.PostAsync(_sessionIDPath + _saveJobPath, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves a job to the designated place outside of the camera (PC).
        /// </summary>
        public async Task<object> SaveJobData(string filename)
        {
            object[] args = { filename };
            return await _cogSocket.PostAsync(_sessionIDPath + _saveJobDataPath, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Requests all of the cell names from the current job.
        /// </summary>
        /// <returns>A JToken that has the mapping from cell locations to cell names</returns>
        public async Task<JToken> RequestAllCellNames()
        {
            object[] args = { };
            return await _cogSocket.PostAsync(_sessionIDPath + _getAllCellNamesPath, args).ConfigureAwait(false) as JToken;
        }

        /// <summary>
        /// Freeze or unfreeze the system results queue.
        /// </summary>
        /// <param name="freeze">The flag that designates whether the queue should be frozen or not</param>
        public async Task FreezeQueue(bool freeze)
        {
            await _cogSocket.PutAsync(_sessionIDPath + _freezeQueuePath, freeze).ConfigureAwait(false);
        }

        /// <summary>
        /// Select a slot in the system results queue.
        /// </summary>
        /// <param name="slotIndex">The zero-based index of the slot to select.</param>
        public async Task SelectQueueSlot(int slotIndex)
        {
            await _cogSocket.PutAsync(_sessionIDPath + _queueSlotIndexPath, slotIndex).ConfigureAwait(false);
        }

        /// <summary>
        /// Requests the image URL for a slot in the result queue.
        /// </summary>
        /// <param name="slotIndex">The zero-based index of the slot to select.</param>
        /// <returns>A string that holds the URL.</returns>
        public async Task<string> RequestQueuedImageUrl(int slotIndex)
        {
            object[] args = new object[1] { slotIndex };
            return await _cogSocket.PostAsync(_sessionIDPath + _getQueuedImageUrlPath, args).ConfigureAwait(false) as string;
        }

        private async Task MakeHttpPostRequest(string url, TimeSpan timeout, string contentType, Stream dataStream)
        {
            try
            {
                if (dataStream != null)
                {
                    StreamReader sr = new StreamReader(dataStream);
                    string json = await sr.ReadToEndAsync();
                    StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage msg = await _httpClient.PostAsync(url, httpContent);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Loads the designated image file as the current image.
        /// </summary>
        /// <param name="filename">The fully qualified path to the BMP file to load</param>
        /// <returns></returns>
        /// <remarks>
        /// This method blocks until the image is loaded.
        /// </remarks>
        public async void LoadImage(string filename)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            string url = RemoteIPAddressUrl + _httpRequestRoot + _sessionIDPath + _loadImagePath;

            TimeSpan timeout = TimeSpan.FromSeconds(30);

            using (var content = new ByteArrayContent(bytes))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/bmp");
                HttpResponseMessage msg = await _httpClient.PostAsync(url, content);
                msg.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Requests cell results for the range of cells.
        /// </summary>
        /// <param name="cellRange">The range of cells (e.g. "A0:D10")</param>
        /// <returns>An array of cell results for the current results held by the session</returns>
        /// <remarks>
        /// The following code can be used to display a range of cells to the Console:
        /// 
        /// Console.WriteLine("QueryCellResults:");
        /// object[] cells = await QueryCellResults("A0:Z399");
        /// foreach (object cell in cells)
        /// {
        ///   string strCell = _sJsonSerializer.SerializeObject(cell);
        ///   Console.WriteLine(strCell);
        /// }
        /// </remarks>
        public async Task<object[]> QueryCellResults(string cellRange)
        {
            object[] args = { new object[] { cellRange } };
            object obj = await _cogSocket.PostAsync(_sessionIDPath + "queryCellResults", args).ConfigureAwait(false);
            object[] cells = obj as object[];
            return cells;
        }



        /// <summary>
        /// Requests the sheet format for the range of cells.
        /// </summary>
        /// <param name="cellRange">The range of cells (e.g. "A0:D10")</param>
        /// <returns>A JToken that has object that contains the sheet formatting for the range of cells</returns>
        public async Task<JToken> RequestSheetFormat(string cellRange)
        {
            object[] args = { new object[] { cellRange } };
            return await _cogSocket.PostAsync(_rootPath + _sheetFormatPath, args).ConfigureAwait(false) as JToken;
        }

        /// <summary>
        /// Periodically send the 'keepAlive' message to keep the HmiSession active.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _keepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_skipKeepAlive) // This will be set if a 'ready' was recently sent. Avoids extra traffic.
                {
                    _skipKeepAlive = false;
                    return;
                }

                if (_isConnected)
                {
                    await _cogSocket.PostAsync(_sessionIDPath + _keepAlivePath, new object[0]).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KeepAlive Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the ResultsChanged event from the CogSocket and forwards it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleResultsChanged(object sender, CogSocketEventArgs args)
        {
            try
            {
                UpdateResults((JToken)args.Args[0]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleResultsChanged: {ex.Message}");
            }
        }

        private void UpdateResults(JToken results)
        {
            try
            {
                _results = results;
                _resultsViewRecord = GetViewRecordFromResult();

                if (ResultsChanged != null)
                {
                    ResultsChanged(this, EventArgs.Empty); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateResults: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the StateChanged event from the CogSocket and forwards it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleStateChanged(object sender, CogSocketEventArgs args)
        {
            try
            {
                object[] state = (object[])args.Args;
                if (state.Length >= 4)
                {
                    _isOnline = Convert.ToBoolean(state[0]);
                    _isSoftOnline = Convert.ToBoolean(state[1]);
                    _isNativeOnline = Convert.ToBoolean(state[2]);
                    _isDiscreteOnline = Convert.ToBoolean(state[3]);
                    if (state.Length >= 5)
                        _isFfpOnline = Convert.ToBoolean(state[4]);
                }

                if (StateChanged != null)
                {
                    StateChanged(this, EventArgs.Empty); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleStateChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the LiveModeChanged event from the CogSocket and forwards it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleLiveModeChanged(object sender, CogSocketEventArgs args)
        {
            try
            {
                bool? isLiveMode = args.Args[0] as bool?;
                _isLiveMode = isLiveMode.HasValue ? isLiveMode.Value : false;

                if (LiveModeChanged != null)
                {
                    LiveModeChanged(this, EventArgs.Empty); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleLiveModeChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Requests the latest 'job' node.
        /// </summary>
        private async Task RequestJobInfo()
        {
            JToken jobInfoObj = await _cogSocket.GetAsync(_rootPath + _jobPath).ConfigureAwait(false) as JToken;
            _jobInfo = jobInfoObj;
            _customViewSettingsList = _jobInfo["customViewSettingsList"]?.ToObject<HmiCustomViewSettings[]>();

            // Before 23.3.0 firmware, the customViewSettingsList was not supported, so try to use customViewSettings instead
            if (_customViewSettingsList == null)
            {
                var customView = _jobInfo["customViewSettings"].ToObject<HmiCustomViewSettings>();
                _customViewSettingsList = new HmiCustomViewSettings[] { customView };
            }


        }

        /// <summary>
        /// Handles the JobInfoChanged event from the CogSocket and forwards it.
        /// </summary>
        private async void HandleJobInfoChanged(object sender, CogSocketEventArgs args)
        {
            try
            {
                // Update _jobInfo
                await RequestJobInfo().ConfigureAwait(false);

                if (JobInfoChanged != null)
                {
                    JobInfoChanged(this, EventArgs.Empty); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleJobInfoChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Requests the latest 'settings' node.
        /// </summary>
        private async Task RequestSettings()
        {
            JToken settingsObj = await _cogSocket.GetAsync(_rootPath + _settingsPath).ConfigureAwait(false) as JToken;
            _settings = settingsObj;
        }

        /// <summary>
        /// Handles the SettingsChanged event from the CogSocket and forwards it.
        /// </summary>
        private async void HandleSettingsChanged(object sender, CogSocketEventArgs args)
        {
            try
            {
                // Update _settings
                await RequestSettings().ConfigureAwait(false);

                if (SettingsChanged != null)
                {
                    SettingsChanged(this, EventArgs.Empty); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleSettingsChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the JobLoadingChanged event from the CogSocket and forwards it.
        /// </summary>
        private void HandleJobLoadingChanged(object sender, CogSocketEventArgs args)
        {
            try
            {
                _jobLoading = !_jobLoading;
                if (_jobLoading)
                {
                    _results = null;
                    _resultsViewRecord = null;
                }

                if (JobLoadingChanged != null)
                {
                    JobLoadingChanged(this, EventArgs.Empty); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleJobLoadingChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the JobLoadFailed event from the CogSocket and forwards it.
        /// </summary>
        private void HandleJobLoadFailed(object sender, CogSocketEventArgs args)
        {
            try
            {
                // Just forward this event...
                if (JobLoadFailed != null)
                {
                    JobLoadFailed(this, args); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleJobLoadFailed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the SessionDisposed event from the CogSocket and forwards it.
        /// </summary>
        private void HandleSessionDisposed(object sender, CogSocketEventArgs args)
        {
            try
            {
                // Just forward this event...
                if (SessionDisposed != null)
                {
                    SessionDisposed(this, args); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleSessionDisposed: {ex.Message}");
            }
        }

        /// <summary>
        /// Requests the latest 'editorAttached' flag.
        /// </summary>
        private async Task RequestEditorAttached()
        {
            bool? editorAttached = await _cogSocket.GetAsync(_rootPath + _editorAttachedPath).ConfigureAwait(false) as bool?;
            _editorAttached = editorAttached.HasValue ? editorAttached.Value : false;
        }

        /// <summary>
        /// Handles the EditorAttachedChanged event from the CogSocket and forwards it.
        /// </summary>
        private async void HandleEditorAttachedChanged(object sender, CogSocketEventArgs args)
        {
            try
            {
                await RequestEditorAttached();

                if (EditorAttachedChanged != null)
                {
                    EditorAttachedChanged(this, EventArgs.Empty); // Propagate the event
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleEditorAttachedChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void HandleJobValidationDone(object sender, CogSocketEventArgs args)
        {
            try
            {
                JobValidationDone(this, EventArgs.Empty); // Propagate the event
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleJobValidationDone: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JToken> FindState()
        {
            // Get the state
            JToken stateObj = await _cogSocket.GetAsync(_rootPath + _statePath).ConfigureAwait(false) as JToken;
            return stateObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JToken> Info()
        {
            JToken infoObj = await _cogSocket.GetAsync(_rootPath + _infoPath).ConfigureAwait(false) as JToken;
            return infoObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JObject[]> ListFiles()
        {
            object result = await _cogSocket.PostAsync(_sessionIDPath + _listFilesPath, new object[0]).ConfigureAwait(false);

            return ((object[])result).Cast<JObject>().ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JToken> GetJobInfo()
        {
            var jobInfoObj = await _cogSocket.GetAsync(_rootPath + _jobPath).ConfigureAwait(false) as JToken;
            //Console.WriteLine(jobInfoObj.GetType());
           return jobInfoObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Object> GetSessionIDs()
        {
            var result = await _cogSocket.PostAsync(_sessionIDPath + _sessionIDs, new object[0]).ConfigureAwait(false);
            
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> JobValidationState()
        {
            return await _cogSocket.GetAsync(_sessionIDPath + _jobValidationState).ConfigureAwait(false) as string;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> SystemValidationFlag()
        {
            var result = await _cogSocket.GetAsync(_sessionIDPath + _systemValidationFlag).ConfigureAwait(false);
           
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task RunJobValidation()
        {
            await _cogSocket.PostAsync(_sessionIDPath + _runJobValidation, Array.Empty<object>()).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task CancelJobValidation()
        {
            await _cogSocket.PostAsync(_sessionIDPath + _cancelJobValidation, Array.Empty<object>()).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetKeepAliveTimeout()
        {
            var result = await _cogSocket.GetAsync(_rootPath + _keepAliveTimeout).ConfigureAwait(false);
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task SetKeepAliveTimeout(int timeout)
        {
            await _cogSocket.PutAsync(_rootPath + _keepAliveTimeout, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task ToggleOnlineOffline()
        {
            Boolean resp = Convert.ToBoolean(await _cogSocket.GetAsync(_sessionIDPath + _softOnlinePath));
            var temp = await _cogSocket.PutAsync(_sessionIDPath + _softOnlinePath, !resp);
        }

        public string SendNMC(string nmc, double timeout, int port, string ipAddress, string username, string password)
        {
            string result;
            string server = ipAddress.Split(':')[0];

            try
            {
                using (TcpClient client = new TcpClient(server, port))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true })
                {
                    // Read server's request for username
                    string request = reader.ReadLine();

                    // Send username and password
                    string login = username + "\r\n" + password + "\r\n";
                    writer.Write(login);

                    // Read server's response
                    request = null;
                    request = reader.ReadLine();

                    // Send NMC
                    writer.Write(nmc + "\r\n");
                    request = null;
                    // Read NMC response
                    try
                    {
                        // Set timeout
                        stream.ReadTimeout = (int)timeout;

                        string temp;
                        while ((temp = reader.ReadLine()) != null)
                        {
                            request = request + " " + temp + "\r\n";
                            Console.WriteLine("Server: " + request);
                        }
                    }
                    catch (IOException ex)
                    {
                        result = "fail";
                        // Console.WriteLine("IOException occurred: " + ex.Message);
                    }

                    result = request;
                }
            }
            catch (Exception e)
            {
                result = "fail";
                // Console.WriteLine("Exception: " + e.Message);
            }
            return result;
        }


    }
}
