using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace CognexCameraSdk
{
    public class HmiNamedContent
    {
        [JsonPropertyName("$type")]
        public string Type { get; set; } = "HmiNamedContent";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }

    public class CogSocket : IAsyncDisposable
    {
        private readonly Uri uri;
        private ClientWebSocket? websocket;
        private int requestCounter;
        private readonly object sync = new();
        private readonly Dictionary<int, TaskCompletionSource<JsonElement?>> pendingRequests = new();
        private readonly Dictionary<string, List<Func<JsonElement?, Task>>> listeners = new();
        private CancellationTokenSource? receiveLoopCts;
        private Task? receiveLoopTask;

        public Action<string>? Log { get; set; }
        public event EventHandler? OnOpen;
        public event EventHandler<Exception>? OnError;
        public event EventHandler? OnClose;

        public CogSocket(string websocketUri)
        {
            uri = new Uri(websocketUri);
        }

        public async Task Connect()
        {
            websocket = new ClientWebSocket();
            receiveLoopCts = new CancellationTokenSource();

            try
            {
                await websocket.ConnectAsync(uri, CancellationToken.None).ConfigureAwait(false);
                OnOpen?.Invoke(this, EventArgs.Empty);
                receiveLoopTask = Task.Run(() => ReceiveLoopAsync(receiveLoopCts.Token));
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                throw;
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var buffer = new byte[8192];
                while (websocket is not null && websocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult? result = null;

                    do
                    {
                        result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).ConfigureAwait(false);
                            break;
                        }

                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    var message = Encoding.UTF8.GetString(ms.ToArray());
                    HandleMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when closing.
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
            finally
            {
                OnClose?.Invoke(this, EventArgs.Empty);
            }
        }

        private void HandleMessage(string message)
        {
            if (Log is not null)
            {
                Log($"Received: {message}");
            }

            JsonDocument? document = null;
            try
            {
                document = JsonDocument.Parse(message);
            }
            catch
            {
                return;
            }

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            var root = document.RootElement;
            if (!root.TryGetProperty("$type", out var typeProperty))
            {
                return;
            }

            var msgType = typeProperty.GetString();
            root.TryGetProperty("path", out var pathProperty);
            var path = pathProperty.ValueKind == JsonValueKind.String ? pathProperty.GetString() ?? string.Empty : string.Empty;

            if (string.Equals(msgType, "resp", StringComparison.OrdinalIgnoreCase))
            {
                if (!root.TryGetProperty("id", out var idProperty) || idProperty.ValueKind != JsonValueKind.Number)
                {
                    return;
                }

                var reqId = idProperty.GetInt32();
                TaskCompletionSource<JsonElement?>? tcs = null;
                lock (sync)
                {
                    if (pendingRequests.TryGetValue(reqId, out var source))
                    {
                        tcs = source;
                        pendingRequests.Remove(reqId);
                    }
                }

                if (tcs is null)
                {
                    return;
                }

                if (root.TryGetProperty("error", out var errorProperty) && errorProperty.ValueKind != JsonValueKind.Null)
                {
                    var errorText = root.TryGetProperty("body", out var bodyProperty)
                        ? bodyProperty.GetRawText()
                        : errorProperty.ToString();
                    tcs.SetException(new InvalidOperationException(errorText));
                    return;
                }

                if (root.TryGetProperty("body", out var bodyProperty2))
                {
                    tcs.SetResult(ParseBody(bodyProperty2));
                }
                else
                {
                    tcs.SetResult(null);
                }

                return;
            }

            if (string.Equals(msgType, "event", StringComparison.OrdinalIgnoreCase))
            {
                var body = root.TryGetProperty("body", out var bodyValue) ? ParseBody(bodyValue) : null;
                if (!string.IsNullOrEmpty(path))
                {
                    List<Func<JsonElement?, Task>>? callbacks = null;
                    lock (sync)
                    {
                        if (listeners.TryGetValue(path, out var list))
                        {
                            callbacks = new List<Func<JsonElement?, Task>>(list);
                        }
                    }

                    if (callbacks is not null)
                    {
                        foreach (var callback in callbacks)
                        {
                            try
                            {
                                callback(body);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine($"Error executing listener for {path}: {ex}");
                            }
                        }
                    }
                }
            }
        }

        private static JsonElement? ParseBody(JsonElement bodyValue)
        {
            if (bodyValue.ValueKind == JsonValueKind.String)
            {
                var rawText = bodyValue.GetString();
                if (string.IsNullOrEmpty(rawText))
                {
                    return bodyValue;
                }

                try
                {
                    using var parsed = JsonDocument.Parse(rawText);
                    return parsed.RootElement.Clone();
                }
                catch
                {
                    return bodyValue;
                }
            }

            return bodyValue;
        }

        private int NextRequestId()
        {
            var next = Interlocked.Increment(ref requestCounter);
            if (next == 0)
            {
                next = Interlocked.Increment(ref requestCounter);
            }

            return next;
        }

        private async Task<JsonElement?> SendRequest(string type, string path, object? body = null)
        {
            if (websocket is null || websocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("WebSocket is not open, cannot send request.");
            }

            var reqId = NextRequestId();
            var tcs = new TaskCompletionSource<JsonElement?>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (sync)
            {
                pendingRequests[reqId] = tcs;
            }

            var message = new Dictionary<string, object?>
            {
                ["$type"] = type,
                ["id"] = reqId,
                ["path"] = path
            };

            if (body is not null)
            {
                message["body"] = body;
            }

            var jsonMsg = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            if (Log is not null)
            {
                Log($"Send: {jsonMsg}");
            }

            var bytes = Encoding.UTF8.GetBytes(jsonMsg);
            await websocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
            return await tcs.Task.ConfigureAwait(false);
        }

        public Task<JsonElement?> Get(string path) => SendRequest("get", path);
        public Task<JsonElement?> Put(string path, object? data) => SendRequest("put", path, data);
        public Task<JsonElement?> Post(string path, object? data) => SendRequest("post", path, data);

        public async Task AddListener(string path, Func<JsonElement?, Task> listener)
        {
            var shouldSubscribe = false;
            lock (sync)
            {
                if (!listeners.ContainsKey(path))
                {
                    listeners[path] = new List<Func<JsonElement?, Task>>();
                    shouldSubscribe = true;
                }

                listeners[path].Add(listener);
            }

            if (shouldSubscribe)
            {
                await SendRequest("listen", path).ConfigureAwait(false);
            }
        }

        public async Task RemoveListener(string path, Func<JsonElement?, Task>? listener = null)
        {
            var shouldUnsubscribe = false;
            lock (sync)
            {
                if (!listeners.ContainsKey(path))
                {
                    return;
                }

                if (listener is not null)
                {
                    listeners[path].Remove(listener);
                    if (listeners[path].Count == 0)
                    {
                        listeners.Remove(path);
                        shouldUnsubscribe = true;
                    }
                }
                else
                {
                    listeners.Remove(path);
                    shouldUnsubscribe = true;
                }
            }

            if (shouldUnsubscribe)
            {
                await SendRequest("unlisten", path).ConfigureAwait(false);
            }
        }

        public async Task Close()
        {
            if (websocket is null)
            {
                return;
            }

            receiveLoopCts?.Cancel();

            try
            {
                if (websocket.State == WebSocketState.Open || websocket.State == WebSocketState.CloseReceived)
                {
                    await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch
            {
                // Ignore close exceptions
            }

            if (receiveLoopTask is not null)
            {
                await receiveLoopTask.ConfigureAwait(false);
            }

            websocket.Dispose();
            websocket = null;
        }

        public async ValueTask DisposeAsync()
        {
            await Close().ConfigureAwait(false);
            receiveLoopCts?.Dispose();
        }
    }

    public class CognexCamera
    {
        private readonly string ip;
        private readonly int port;
        private readonly string username;
        private readonly string password;
        private CogSocket? cogsock;
        private string? sessionId;
        private Timer? keepAliveTimer;
        private readonly string root = "cam0/hmi";
        private readonly string cells = "A0:Z100";

        public Action<string>? Logger { get; set; }

        public event Func<JsonElement?, Task>? StateChanged;
        public event Func<JsonElement?, Task>? ResultsChanged;
        public event Func<JsonElement?, Task>? LiveModeChanged;
        public event Func<JsonElement?, Task>? JobInfoChanged;
        public event Func<JsonElement?, Task>? EditorAttachedChanged;
        public event Func<JsonElement?, Task>? JobLoadingChanged;
        public event Func<JsonElement?, Task>? SettingsChanged;
        public event Func<JsonElement?, Task>? JobLoadFailed;
        public event Func<JsonElement?, Task>? JobValidationDone;
        public event Func<JsonElement?, Task>? SessionDisposed;

        public CognexCamera(string ip, int port = 80, string username = "admin", string password = "")
        {
            this.ip = ip;
            this.port = port;
            this.username = username;
            this.password = password;
        }

        public async Task Connect()
        {
            var uri = $"ws://{ip}:{port}/ws";
            cogsock = new CogSocket(uri)
            {
                Log = msg => Logger?.Invoke(msg)
            };

            cogsock.OnError += (sender, exception) => Logger?.Invoke($"WebSocket error: {exception.Message}");
            cogsock.OnClose += (sender, args) => Logger?.Invoke("WebSocket closed.");

            await cogsock.Connect().ConfigureAwait(false);

            var sessionInfo = new Dictionary<string, object?>
            {
                ["cellNames"] = new[] { cells }
            };

            var openSessionResponse = await cogsock.Post($"{root}/openSession", sessionInfo).ConfigureAwait(false);
            sessionId = ExtractSessionId(openSessionResponse);
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new InvalidOperationException("Unable to establish session.");
            }

            Logger?.Invoke($"Session ID: {sessionId}");

            var loginResponse = await cogsock.Post($"{sessionId}/login", new object[] { username, password, false }).ConfigureAwait(false);
            if (loginResponse.HasValue && loginResponse.Value.ValueKind == JsonValueKind.Object && loginResponse.Value.TryGetProperty("error", out _))
            {
                throw new InvalidOperationException("Login failed");
            }

            Logger?.Invoke("Login successful");

            await cogsock.AddListener($"{root}/stateChanged", _onStateChanged).ConfigureAwait(false);
            await cogsock.AddListener($"{sessionId}/resultChanged", _onResultChanged).ConfigureAwait(false);
            await cogsock.AddListener($"{root}/liveModeChanged", _onLiveModeChanged).ConfigureAwait(false);
            await cogsock.AddListener($"{root}/jobChanged", _onJobChanged).ConfigureAwait(false);
            await cogsock.AddListener($"{root}/editorAttachedChanged", _onEditorAttached).ConfigureAwait(false);
            await cogsock.AddListener($"{root}/jobLoadingChanged", _onJobLoadingChanged).ConfigureAwait(false);
            await cogsock.AddListener($"{root}/settingsChanged", _onSettingsChanged).ConfigureAwait(false);
            await cogsock.AddListener($"{root}/jobLoadFailed", _onJobLoadFailedChanged).ConfigureAwait(false);
            await cogsock.AddListener($"{root}/jobValidationDone", _onJobValidationDoneChanged).ConfigureAwait(false);
            await cogsock.AddListener($"{root}/sessionDisposed", _onSessionDisposedChanged).ConfigureAwait(false);

            await cogsock.Post($"{sessionId}/ready", null).ConfigureAwait(false);
            StartKeepAlive();
        }

        private static string? ExtractSessionId(JsonElement? response)
        {
            if (!response.HasValue)
            {
                return null;
            }

            var element = response.Value;
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("number", out var numberProperty))
            {
                return numberProperty.ValueKind switch
                {
                    JsonValueKind.Number => numberProperty.GetInt32().ToString(),
                    JsonValueKind.String => numberProperty.GetString(),
                    _ => numberProperty.GetRawText()
                };
            }

            return element.ValueKind switch
            {
                JsonValueKind.Number => element.GetInt32().ToString(),
                JsonValueKind.String => element.GetString(),
                _ => element.GetRawText()
            };
        }

        private void StartKeepAlive()
        {
            keepAliveTimer?.Dispose();
            keepAliveTimer = new Timer(async _ => await KeepAliveTick().ConfigureAwait(false), null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
        }

        private async Task KeepAliveTick()
        {
            if (cogsock is null || string.IsNullOrEmpty(sessionId))
            {
                return;
            }

            try
            {
                await cogsock.Post($"{sessionId}/keepAlive", null).ConfigureAwait(false);
                await cogsock.Post($"{sessionId}/ready", null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger?.Invoke($"Keep alive failed: {ex.Message}");
                keepAliveTimer?.Dispose();
                keepAliveTimer = null;
            }
        }

        private async Task _onStateChanged(JsonElement? body)
        {
            if (StateChanged is not null)
            {
                await StateChanged(body).ConfigureAwait(false);
            }
        }

        private async Task _onResultChanged(JsonElement? body)
        {
            await SendReady().ConfigureAwait(false);
            if (ResultsChanged is not null)
            {
                await ResultsChanged(body).ConfigureAwait(false);
            }
        }

        private async Task _onLiveModeChanged(JsonElement? body)
        {
            if (LiveModeChanged is not null)
            {
                await LiveModeChanged(body).ConfigureAwait(false);
            }
        }

        private async Task _onJobChanged(JsonElement? body)
        {
            if (JobInfoChanged is not null)
            {
                await JobInfoChanged(body).ConfigureAwait(false);
            }
        }

        private async Task _onEditorAttached(JsonElement? body)
        {
            if (EditorAttachedChanged is not null)
            {
                await EditorAttachedChanged(body).ConfigureAwait(false);
            }
        }

        private async Task _onJobLoadingChanged(JsonElement? body)
        {
            if (JobLoadingChanged is not null)
            {
                await JobLoadingChanged(body).ConfigureAwait(false);
            }
        }

        private async Task _onSettingsChanged(JsonElement? body)
        {
            if (SettingsChanged is not null)
            {
                await SettingsChanged(body).ConfigureAwait(false);
            }
        }

        private async Task _onJobLoadFailedChanged(JsonElement? body)
        {
            if (JobLoadFailed is not null)
            {
                await JobLoadFailed(body).ConfigureAwait(false);
            }
        }

        private async Task _onJobValidationDoneChanged(JsonElement? body)
        {
            if (JobValidationDone is not null)
            {
                await JobValidationDone(body).ConfigureAwait(false);
            }
        }

        private async Task _onSessionDisposedChanged(JsonElement? body)
        {
            if (SessionDisposed is not null)
            {
                await SessionDisposed(body).ConfigureAwait(false);
            }
        }

        public async Task Disconnect()
        {
            keepAliveTimer?.Dispose();
            keepAliveTimer = null;

            if (cogsock is not null && !string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    Logger?.Invoke($"Disposing session: {sessionId}");
                    await cogsock.Post($"{sessionId}/dispose", null).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger?.Invoke($"Dispose failed: {ex.Message}");
                }
            }

            if (cogsock is not null)
            {
                await cogsock.Close().ConfigureAwait(false);
                cogsock = null;
            }

            sessionId = null;
            Logger?.Invoke("Disconnected successfully.");
        }

        private static string SerializeResponse(JsonElement? response)
        {
            return response?.GetRawText() ?? "null";
        }

        public Task ManualAcquire() => cogsock?.Post($"{sessionId}/manualTrigger", null) ?? throw new InvalidOperationException("Not connected.");
        public Task SetLiveMode(bool enabled) => cogsock?.Put($"{sessionId}/liveMode", enabled) ?? throw new InvalidOperationException("Not connected.");

        public async Task<string> GetLiveMode()
        {
            var resp = await (cogsock?.Get($"{sessionId}/liveMode") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task ToggleOnlineOffline()
        {
            var resp = await (cogsock?.Get($"{sessionId}/softOnline") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            var currentOnline = resp.HasValue && resp.Value.ValueKind == JsonValueKind.True;
            var newOnline = !currentOnline;
            await cogsock.Put($"{sessionId}/softOnline", newOnline).ConfigureAwait(false);
        }

        public async Task<string> QueryCellResults(string cell)
        {
            var resp = await (cogsock?.Post($"{sessionId}/queryCellResults", new object[] { new[] { cell } }) ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task<string> GetCellExpression(string cell)
        {
            var resp = await (cogsock?.Post($"{sessionId}/getCellExpressions", new object[] { cell, true }) ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public Task SetCellExpression(string cell, string expr) => cogsock?.Post($"{sessionId}/setCellExpression", new object[] { cell, expr }) ?? throw new InvalidOperationException("Not connected.");
        public Task SetCellValue(string cell, object value) => cogsock?.Post($"{sessionId}/setCellValue", new object[] { cell, value }) ?? throw new InvalidOperationException("Not connected.");
        public async Task<string> ListFiles()
        {
            var resp = await (cogsock?.Post($"{sessionId}/listFiles", Array.Empty<object>()) ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task<string> Info()
        {
            var resp = await (cogsock?.Get($"{root}/info") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task<string> FindState()
        {
            var resp = await (cogsock?.Get($"{root}/state") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public Task SetSoftOnline(bool state) => cogsock?.Put($"{sessionId}/softOnline", state) ?? throw new InvalidOperationException("Not connected.");

        public async Task<string> GetJobInfo()
        {
            var resp = await (cogsock?.Get($"{root}/job") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public Task SaveJob(string jobName) => cogsock?.Post($"{sessionId}/saveJob", jobName) ?? throw new InvalidOperationException("Not connected.");
        public Task LoadJob(string jobName) => cogsock?.Post($"{sessionId}/loadJob", jobName) ?? throw new InvalidOperationException("Not connected.");
        public Task SendReady() => cogsock?.Post($"{sessionId}/ready", null) ?? throw new InvalidOperationException("Not connected.");

        public async Task<string> GetSessionIDs()
        {
            var resp = await (cogsock?.Post($"{sessionId}/getSessionIDs", null) ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task<string> JobValidationState()
        {
            var resp = await (cogsock?.Get($"{sessionId}/jobValidationState") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task<string> SystemValidationFlag()
        {
            var resp = await (cogsock?.Get($"{sessionId}/systemValidationFlag") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public Task RunJobValidation() => cogsock?.Post($"{sessionId}/runJobValidation", null) ?? throw new InvalidOperationException("Not connected.");


        public Task CancelJobValidation() => cogsock?.Post($"{sessionId}/cancelJobValidation", null) ?? throw new InvalidOperationException("Not connected.");
        
        public async Task<string> GetKeepAliveTimeout()
        {
            var resp = await (cogsock?.Get($"{root}/keepAliveTimeout") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task<string> SetKeepAliveTimeout(int timeoutSeconds)
        {
            var resp = await (cogsock?.Put($"{root}/keepAliveTimeout", timeoutSeconds) ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task<string> LoadImage(string filename, string? imageName = null)
        {
            byte[] bytesData = await File.ReadAllBytesAsync(filename).ConfigureAwait(false);

            string url = $"http://{ip}:{port}/{sessionId}/loadImage";

            using var content = new ByteArrayContent(bytesData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/bmp");

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            using var response = await client.PostAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> NMC(string nmc, int timeout, int port, string ip, string username, string password)
        {
            using var client = new TcpClient();

            // Initial connection timeout
            using var connectCts = new CancellationTokenSource(
                TimeSpan.FromSeconds(timeout));

            await client.ConnectAsync(ip, port, connectCts.Token);

            using NetworkStream stream = client.GetStream();

            // Initial read
            byte[] buffer = new byte[4096];

            int bytesRead = await stream.ReadAsync(
                buffer,
                0,
                buffer.Length,
                connectCts.Token);

            // Login
            string login = $"{username}\r\n{password}\r\n";
            byte[] loginBytes = Encoding.ASCII.GetBytes(login);

            await stream.WriteAsync(loginBytes, 0, loginBytes.Length);

            // Keep login delay
            await Task.Delay(250);

            // Read login response
            bytesRead = await stream.ReadAsync(
                buffer,
                0,
                buffer.Length,
                connectCts.Token);

            // Send command
            string command = $"{nmc}\r\n";
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);

            await stream.WriteAsync(commandBytes, 0, commandBytes.Length);

            // Wait for NMC response
            var response = new StringBuilder();
            bool receivedData = false;

            using var timeoutCts = new CancellationTokenSource(
                TimeSpan.FromSeconds(timeout));

            while (true)
            {
                try
                {
                    // Once data has started arriving, use a 100 ms quiet period.
                    using var readCts = receivedData
                        ? new CancellationTokenSource(TimeSpan.FromMilliseconds(100))
                        : CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token);

                    bytesRead = await stream.ReadAsync(
                        buffer,
                        0,
                        buffer.Length,
                        readCts.Token);

                    if (bytesRead == 0)
                        break;

                    response.Append(
                        Encoding.ASCII.GetString(buffer, 0, bytesRead));

                    if (!receivedData)
                        receivedData = true;
                }
                catch (OperationCanceledException)
                {
                    // No data yet: keep waiting until the overall timeout.
                    if (!receivedData)
                    {
                        if (timeoutCts.IsCancellationRequested)
                            break;

                        continue;
                    }

                    // We already received data, so 100 ms of silence means done.
                    break;
                }
            }

            return response.ToString();
        }


        public async Task<string> GetStartupJob()
        {
            var resp = await (cogsock?.Get($"{sessionId}/startupJob") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public Task SetStartupJob(string jobName) => cogsock?.Put($"{sessionId}/startupJob", jobName) ?? throw new InvalidOperationException("Not connected.");

        public async Task<string> GetAllCellNames()
        {
            var resp = await (cogsock?.Post($"{sessionId}/getAllCellNames", null) ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public Task SetCellName(string cell, string name) => cogsock?.Post($"{sessionId}/setCellName", new object[] { cell, name }) ?? throw new InvalidOperationException("Not connected.");

        public Task CreateNewJob() => cogsock?.Post($"{sessionId}/createNewJob", "") ?? throw new InvalidOperationException("Not connected.");

        public Task StartupOnline(bool online) => cogsock?.Put($"{sessionId}/startupOnline", online) ?? throw new InvalidOperationException("Not connected.");

        public async Task<string> StartupOnlineStatus()
        {
            var resp = await (cogsock?.Get($"{sessionId}/startupOnline") ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);
        }

        public async Task<string> GetCellCondition(string cell)
        {
            var resp = await (cogsock?.Post($"{sessionId}/getCellCondition", cell) ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);            
        }

        public async Task<string> SetCellCondition(string cell, string condition)
        {
            var resp = await (cogsock?.Post($"{sessionId}/setCellCondition", new object[] { cell, condition }) ?? throw new InvalidOperationException("Not connected.")).ConfigureAwait(false);
            return SerializeResponse(resp);            
        }

        public async Task LoadJobData(string filePath)
        {
            byte[] fileData = await File.ReadAllBytesAsync(filePath);

            var hmiNamedContent = new HmiNamedContent
            {
                Name = Path.GetFileName(filePath),
                Content = Convert.ToBase64String(fileData)
            };

            await cogsock!.Post($"{sessionId}/loadJobData", hmiNamedContent);
        }
    }
}
