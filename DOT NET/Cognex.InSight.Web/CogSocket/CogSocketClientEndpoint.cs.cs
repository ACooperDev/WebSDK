//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using System.Security.Cryptography.X509Certificates;

namespace Cognex.SimpleCogSocket
{
    /// <summary> Client implementation of CogSocket. </summary>
    internal class CogSocketClientEndpoint : ICogSocketEndpoint
    {
        private static readonly JsonSerializerSettings __messageSerializerSettings;

        /// <summary>
        ///  Regex to use when we cannot parse the json but still want to resolve the task associated with
        ///  the id.  Looks for an id property at the end of the message, which testing has shown is the
        ///  most common.
        ///  
        ///  Regex explanation:
        ///    - find the "id" property that exists at the end of the document
        ///    - then get the numeric value of the id
        ///    - it must be followed by a the closing bracket at the end of the document.
        /// </summary>
        private static Regex ParseIdAtEndRegex =
            new Regex(@"""id"":(\d+)}$", RegexOptions.Compiled);

        /// <summary>
        ///  Regex to use when we cannot parse the json but still want to resolve the task associated with
        ///  the id.  Fallback to use when <see cref="ParseIdAtEndRegex"/> fails.  This may result in
        ///  false-positives if the JSON is invalid AND the payload has an "id" property that represents
        ///  something other than the message id, but it's already an edge case and we just have to hope
        ///  that it doesn't happen.
        ///  
        ///  Regex explanation:
        ///    - find the "id" property that exists after the first bracket in the document OR a comma
        ///      (which would mean it's following another property).
        ///    - then get the numeric value of the id
        ///    - followed by a comma (so there's another property after this one) or a closing bracket
        ///      at the end of the document.
        ///  fweb
        ///  Making sure that the bracket is at the beginning of the document or the end of the document
        ///  ensures that we're not the first sub-property of a sub-object (which would not be a CogSocket
        ///  Id but rather a property of the response).  Note that the commas of the regex doesn't defend
        ///  against this as it'd be a whole lot more complex.
        /// </summary>
        private static Regex ParseIdManuallyFallbackRegex
            = new Regex(@"(?:^{|,)""id"":(\d+)(?:,|}$)", RegexOptions.Compiled);

        private readonly WebSocket _websocket;
        private readonly JsonSerializerSettings _payloadSerializerSettings;

        private readonly object _connectTaskLock = new object();
        private TaskCompletionSource<object> _connectTask;

        static CogSocketClientEndpoint()
        {
            var binder = new TypeNameBinder();
            binder.RegisterAssembly(typeof(CogSocket).Assembly);
            // Register additional In-Sight-specific types
            InSight.Remoting.Serialization.SerializedTypeRegistration.RegisterBuiltIns();
            __messageSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                //Formatting = Formatting.Indented,
                Binder = binder
            };
        }
        public CogSocketClientEndpoint(string url, JsonSerializerSettings serializerSettings)
        {
            _payloadSerializerSettings = serializerSettings;

            _websocket = new WebSocket(url);
            _websocket.OnOpen += websocket_OnOpen;
            _websocket.OnClose += websocket_OnClose;
            _websocket.OnError += websocket_OnError;
            _websocket.OnMessage += websocket_OnMessage;
        }
        public CogSocketClientEndpoint(string url, JsonSerializerSettings serializerSettings, X509CertificateCollection clientCerts)
            : this(url, serializerSettings)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri) && (uri.Scheme == "wss"))
            {
                _websocket.SslConfiguration.ClientCertificates = clientCerts;
                _websocket.SslConfiguration.EnabledSslProtocols
                    = System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12;

                // To validate the server certificate.
                _websocket.SslConfiguration.ServerCertificateValidationCallback =
                  (sender, certificate, chain, sslPolicyErrors) =>
                  {
                      _websocket.Log.Debug(
                            String.Format(
                              "Certificate:\n- Issuer: {0}\n- Subject: {1}",
                              certificate.Issuer,
                              certificate.Subject));

                      return true; // If the server certificate is valid.
                  };
            }
        }

        public event EventHandler Opened;
        public event EventHandler Closed;
        public event EventHandler<CogSocketErrorArgs> Error;
        public event EventHandler<CogSocketMessageEventArgs> Message;

        private void OnError(CogSocketErrorArgs errorArgs)
        {
            var error = Error;
            if (error != null)
            {
                error.Invoke(this, errorArgs);
            }
        }

        private void websocket_OnOpen(object sender, EventArgs e)
        {
            TryResolveConnectTaskWithSuccess();

            var opened = Opened;
            if (opened != null)
            {
                opened(this, EventArgs.Empty);
            }
        }

        private void websocket_OnClose(object sender, CloseEventArgs e)
        {
            var closed = Closed;

            // In case, the close occurred during connection...
            TryResolveConnectTaskWithException(new Exception("Connect Failed"));

            if (closed != null)
            {
                closed.Invoke(this, EventArgs.Empty);
            }
        }

        private void websocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            TryResolveConnectTaskWithException(new Exception(e.Message, e.Exception));
            OnError(new CogSocketErrorArgs(e.Message, e.Exception));
        }

        private void TryResolveConnectTaskWithSuccess()
        {
            var pendingTaskCompletionSource = ClearAndReturnPendingTask();
            if (pendingTaskCompletionSource == null)
                return;

            // resolve outside of the lock and on a background thread
            Task.Run(() => pendingTaskCompletionSource.TrySetResult(null));
        }

        private void TryResolveConnectTaskWithException(Exception exception)
        {
            var pendingTaskCompletionSource = ClearAndReturnPendingTask();
            if (pendingTaskCompletionSource == null)
                return;

            // resolve outside of the lock and on a background thread
            Task.Run(() => pendingTaskCompletionSource.TrySetException(exception));
        }

        /// <summary>
        ///  Clears the task that we give out to consumers calling Connect and returns it, allowing the
        ///  TaskCompletionSource to be resolved by the caller.
        /// </summary>
        private TaskCompletionSource<object> ClearAndReturnPendingTask()
        {
            TaskCompletionSource<object> pendingTaskCompletionSource;

            lock (_connectTaskLock)
            {
                pendingTaskCompletionSource = _connectTask;
                _connectTask = null;
            }

            return pendingTaskCompletionSource;
        }

        private void websocket_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.IsText)
                {
                    HandleTextBasedMessage(e);
                }
                else if (e.IsBinary)
                {
                    HandleBinaryBasedMessage();
                }
            }
            catch (Exception exception)
            {
                // if exceptions escape here, it shuts down the socket, so don't let anything escape
                OnError(new CogSocketErrorArgs("Exception occurred while processing a websocket message", exception));
            }

        }

        private void HandleTextBasedMessage(MessageEventArgs e)
        {
            try
            {
                OnPreviewMessage(MessagePayloadPreviewEventArgs.Incoming(e.Data));

                // intentionally not using DeserializeObject here, because it throws exceptions for certain data
                // when deserializing it to JToken
                var jobject = JObject.Parse(e.Data);
                var type = jobject["$type"].ToString();
                CogSocketMessage message;
                if (type == "event")
                {
                    message = new CogSocketEventMessage();
                }
                else
                {
                    message = new CogSocketResponseMessage();
                }

                message.Id = jobject["id"] != null ? jobject["id"].ToObject<int>() : 0;
                message.Error = jobject["error"] != null ? jobject["error"].ToObject<int>() : 0;
                message.Path = jobject["path"] != null ? jobject["path"].ToString() : null;
                message.Body = jobject["body"];

                if (Message != null)
                    Message(this, new CogSocketMessageEventArgs(message));
            }
            catch (Exception exception)
            {

                CogSocketMessage error = null;
                try
                {
                    var data = MessageMetaData.DeserializeFrom(e.Data);

                    // if this was a response, that most likely means that we have someone waiting on it. So if an
                    // exception occurs, at least try to bubble it up as an error message so that they don't end up
                    // waiting forever. 
                    if (data.DataType == "resp" && data.Id != null)
                    {
                        error = new CogSocketResponseMessage
                        {
                            Error = -1,
                            Id = data.Id.Value,
                        };
                        error.SetPayload(exception, _payloadSerializerSettings);
                    }
                }
                catch (Exception)
                {
                    error = null;
                }

                if (error == null)
                {
                    // we couldn't even parse it into a small object.  We're desperate now, so try a regex to get
                    // the id so that we can complete with an error. 
                    var match = ParseIdAtEndRegex.Match(e.Data);

                    if (!match.Success)
                    {
                        // we're really getting desperate.  Use the fall-back regex instead. 
                        match = ParseIdManuallyFallbackRegex.Match(e.Data);
                    }

                    if (match.Success)
                    {
                        error = new CogSocketResponseMessage
                        {
                            Error = -1,
                            Id = Int32.Parse(match.Groups[1].Value),
                        };
                        error.SetPayload(exception, _payloadSerializerSettings);
                    }

                }

                if (error == null)
                {
                    Debug.Fail("JSON Deserialization error: " + exception);
                    // let it propagate, there is nothing we can do
                    // ReSharper disable once HeuristicUnreachableCode
                    throw;
                }

                if (Message != null)
                    Message(this, new CogSocketMessageEventArgs(error));
            }
        }

        private void HandleBinaryBasedMessage()
        {
            byte[] sorry = { 0, 0, 0xE0, 0x80 };
            lock (_websocket)
            {
                _websocket.Send(sorry);
            }
        }

        /*
        public Task Connect()
        {
            var tcs = new TaskCompletionSource<object>();

            lock (_connectTaskLock)
            {
                _connectTask = tcs;
            }

            lock (_websocket)
            {
                _websocket.ConnectAsync();
               
            }
            return tcs.Task; // Warning: _connectTask may be null by now if socket has already connected
        }
        */

        //Fix Connect() for .NET9.
        public Task Connect()
        {
            var tcs = new TaskCompletionSource<object>();

            lock (_connectTaskLock)
            {
                _connectTask = tcs;
            }

            lock (_websocket)
            {
                try
                {
                    _websocket.Connect(); // synchronous
                    TryResolveConnectTaskWithSuccess();
                }
                catch (Exception ex)
                {
                    TryResolveConnectTaskWithException(ex);
                }
            }

            return tcs.Task;
        }

        public void Send(CogSocketMessage message)
        {
            string json = JsonConvert.SerializeObject(message, __messageSerializerSettings);
            OnPreviewMessage(MessagePayloadPreviewEventArgs.Outgoing(json));

            lock (_websocket)
            {
                _websocket.Send(json);
            }
        }

        public void Close()
        {
            lock (_websocket)
            {
                if (_websocket.ReadyState != WebSocketState.Closed)
                {
                    _websocket.Close();
                }
            }
        }

        /// <summary> Used for deserialization purposes only. </summary>
        private struct MessageMetaData
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            [JsonProperty("id")]
            public int? Id { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            [JsonProperty("$type")]
            public string DataType { get; set; }

            public static MessageMetaData DeserializeFrom(string json)
            {
                // get id and type of the message
                return JsonConvert.DeserializeObject<MessageMetaData>(
                    json,
                    new JsonSerializerSettings
                    {
                        // MetadataPropertyHandling = MetadataPropertyHandling.Ignore
                    });
            }
        }

        public event EventHandler<MessagePayloadPreviewEventArgs> PreviewMessage;

        protected virtual void OnPreviewMessage(MessagePayloadPreviewEventArgs e)
        {
            var handler = PreviewMessage;
            if (handler != null)
                handler(this, e);
        }
    }
}
