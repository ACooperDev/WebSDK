//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************ghts Reserved

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace Cognex.SimpleCogSocket
{
    /// <summary>
    /// Provides object remoting between systems using the CogSocket protocol.
    /// </summary>
    /// <remarks>
    /// All members of this class are thread-safe and can be called from multiple threads concurrently.
    /// </remarks>
    public class CogSocket : IDisposable, IMessagePayloadPreview
    {
        #region Private fields

        private readonly ICogSocketEndpoint _endpoint;
        private readonly Dictionary<string, List<CogSocketEventHandler>> _listeners;

        private Int32 _requestId;
        private readonly ConcurrentDictionary<int, SingleRequestPendingResult> _pendingRequests;
        private JsonSerializerSettings _payloadSerializerSettings;
        private TimeSpan _responseTimeout;

        #endregion

        /// <summary> Static constructor. </summary>
        static CogSocket()
        {
            TaskTimeout = TimeSpan.FromSeconds(45);
        }

        /// <summary> Deprecated, use <see cref="ResponseTimeout"/> instead. </summary>
        public static TimeSpan TaskTimeout { get; set; }

        #region Public (client) Constructor

        /// <summary>
        /// Constructs a new CogSocket and establishes a connection to a server.
        /// </summary>
        /// <param name="url">The URL of the CogSocket server.</param>
        /// <param name="serializerSettings"></param>
        public CogSocket(string url, JsonSerializerSettings serializerSettings)
            : this(new CogSocketClientEndpoint(url, serializerSettings), serializerSettings)
        {
        }

        /// <summary>
        /// Constructs a new CogSocket and establishes a WSS (WebSocket over TLS) connection to a server.
        /// </summary>
        /// <param name="url">The URL of the CogSocket server.</param>
        /// <param name="serializerSettings"></param>
        /// <param name="clientCerts">The CA certificate used for establishing the WSS conneciton.</param>
        public CogSocket(string url, JsonSerializerSettings serializerSettings, X509CertificateCollection clientCerts)
            : this(new CogSocketClientEndpoint(url, serializerSettings, clientCerts), serializerSettings)
        {
        }

        #endregion

        #region Internal constructor

        internal CogSocket(ICogSocketEndpoint endpoint,
                                 JsonSerializerSettings serializerSettings)
        {
            _payloadSerializerSettings = serializerSettings;

            _endpoint = endpoint;
            _endpoint.Message += endpoint_OnMessage;
            _endpoint.Closed += endpoint_Closed;

            _requestId = 0;
            _pendingRequests = new ConcurrentDictionary<int, SingleRequestPendingResult>();
            _listeners = new Dictionary<string, List<CogSocketEventHandler>>();
        }

        #endregion

        /// <summary>
        ///  The time, from request to response, that a request is allowed to take before it is cancelled
        ///  with an exception.
        /// </summary>
        /// <remarks>
        ///  If set to zero, will default to <see cref="TaskTimeout"/>.
        ///  
        ///  If per-request timeout is desired, an overload of the Async methods allows passing in a
        ///  <see cref="RequestOptions"/> instance which can specify a timeout for each specific request.
        /// </remarks>
        public TimeSpan ResponseTimeout
        {
            get { return _responseTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentException("ResponseTimeout must be > 0");

                _responseTimeout = value;
            }
        }

        /// <summary> Retrieves the timeout to use for new tasks. </summary>
        private TimeSpan GetDefaultTimeout()
        {
            if (_responseTimeout != TimeSpan.Zero)
            {
                return _responseTimeout;
            }
            else
            {
                return TaskTimeout;
            }
        }

        #region Public methods

        /// <summary>
        /// Connects a client socket to a server.
        /// </summary>
        /// <returns>A Task object with which to await the response.</returns>
        public Task Connect()
        {
            return _endpoint.Connect();
        }

        /// <summary>
        ///  Close the socket and cancel all pending tasks. Has no effect if it has already been closed.
        /// </summary>
        public void Dispose()
        {
            HandleCleanup();
        }

        /// <summary> Cleanup due to closing or disposal. </summary>
        private void HandleCleanup(bool isCalledByEndpoint = false)
        {
            // the endpoint closed itself, so don't attempt to close it again, or we'll become re-entrant
            // which eventually deadlocks. 
            if (!isCalledByEndpoint)
            {
                _endpoint.Close();
            }

            // cancel all pending requests
            foreach (var request in _pendingRequests)
            {
                request.Value.SetCancelled();
                SingleRequestPendingResult ignored;
                _pendingRequests.TryRemove(request.Key, out ignored);
            }

            // remove all listeners
            lock (_listeners)
            {
                _listeners.Clear();
            }
        }

        /// <summary> Sends a 'get' request, usually to retrieve the value of a property. </summary>
        /// <param name="path"> The path to a property to retrieve. </param>
        /// <param name="cancellationToken"> A cancellation token that, if cancelled, cancels the returned
        ///  task. </param>
        /// <returns> A task that resolves when the response to the 'get' has been received. </returns>
        public Task<object> GetAsync(string path,
                                     CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetAsync(path,
                            new RequestOptions
                            {
                                CancellationToken = cancellationToken,
                                ResponseTimeout = GetDefaultTimeout()
                            });
        }

        /// <summary> Sends a 'get' request, usually to retrieve the value of a property. </summary>
        /// <param name="path"> The path to a property to retrieve. </param>
        /// <param name="options"> The options that configure how the request and response are handled. </param>
        /// <returns> A task that resolves when the response to the 'get' has been received. </returns>
        public Task<object> GetAsync(string path,
                                     RequestOptions options)
        {
            return SendRequest(new CogSocketGetMessage { Path = path }, options);
        }

        /// <summary> Sends a 'put' request, usually to set a property value. </summary>
        /// <param name="path"> The path to a property to set. </param>
        /// <param name="value"> The property value to set. </param>
        /// <param name="cancellationToken"> A cancellation token that, if cancelled, cancels the returned
        ///  task. </param>
        /// <returns> A task that resolves when the response to the 'put' has been received. </returns>
        public Task<object> PutAsync(string path,
                                     object value,
                                     CancellationToken cancellationToken = default(CancellationToken))
        {
            return PutAsync(path, value, new RequestOptions
            {
                CancellationToken = cancellationToken,
                ResponseTimeout = GetDefaultTimeout()
            });
        }

        /// <summary> Sends a 'put' request, usually to set a property value. </summary>
        /// <param name="path"> The path to a property to set. </param>
        /// <param name="value"> The property value to set. </param>
        /// <param name="options"> The options that configure how the request and response are handled. </param>
        /// <returns> A task that resolves when the response to the 'put' has been received. </returns>
        public Task<object> PutAsync(string path,
                                     object value,
                                     RequestOptions options)
        {
            var message = new CogSocketPutMessage { Path = path };
            message.SetPayload(value, _payloadSerializerSettings);
            return SendRequest(message, options);
        }

        /// <summary> Sends a 'post' request, usually to execute a method. </summary>
        /// <param name="path"> The path to a method to execute. </param>
        /// <param name="args"> Optional method arguments.  Pass null to pass no arguments. </param>
        /// <param name="cancellationToken"> A cancellation token that, if cancelled, cancels the returned
        ///  task. </param>
        /// <returns> A task that resolves when the response to the 'post' has been received. </returns>
        public Task<object> PostAsync(string path,
                                      object[] args,
                                      CancellationToken cancellationToken = default(CancellationToken))
        {
            return PostAsync(path, args, new RequestOptions() { CancellationToken = cancellationToken });
        }

        /// <summary> Sends a 'post' request, usually to execute a method. </summary>
        /// <param name="path"> The path to a method to execute. </param>
        /// <param name="args"> Optional method arguments.  Pass null to pass no arguments. </param>
        /// <param name="options"> The options that configure how the request and response are handled. </param>
        /// <returns> A task that resolves when the response to the 'post' has been received. </returns>
        public Task<object> PostAsync(string path,
                                      object[] args,
                                      RequestOptions options)
        {
            if (args != null && args.Length == 0)
                args = null;

            var message = new CogSocketPostMessage { Path = path };
            message.SetPayload(args, _payloadSerializerSettings);
            return SendRequest(message, options);
        }

        /// <summary> Adds an event listener. </summary>
        /// <remarks>
        ///  The event handler may be invoked on a worker thread so callers must be prepared for that.
        /// </remarks>
        /// <param name="eventName"> The slash-separated path to the event. </param>
        /// <param name="listener"> The event handler to be called when the event is received. </param>
        /// <param name="cancellationToken"> A cancellation token that, if cancelled, cancels the returned
        ///  task. </param>
        /// <returns> A task that resolves when the event listener has been added. </returns>
        public Task<object> AddListenerAsync(string eventName,
                                             CogSocketEventHandler listener,
                                             CancellationToken cancellationToken = default(CancellationToken))
        {
            return AddListenerAsync(eventName, listener, new RequestOptions
            {
                CancellationToken = cancellationToken,
                ResponseTimeout = GetDefaultTimeout()
            });
        }

        /// <summary> Adds an event listener. </summary>
        /// <remarks>
        ///  The event handler may be invoked on a worker thread so callers must be prepared for that.
        /// </remarks>
        /// <param name="eventName"> The slash-separated path to the event. </param>
        /// <param name="listener"> The event handler to be called when the event is received. </param>
        /// <param name="options"> The options that configure how the request and response are handled. </param>
        /// <returns> A task that resolves when the event listener has been added. </returns>
        public Task<object> AddListenerAsync(string eventName,
                                             CogSocketEventHandler listener,
                                             RequestOptions options)
        {
            var firstListener = false;
            lock (_listeners)
            {
                List<CogSocketEventHandler> list;
                if (_listeners.TryGetValue(eventName, out list))
                {
                    list.Add(listener);
                }
                else
                {
                    firstListener = true;
                    list = new List<CogSocketEventHandler>();
                    list.Add(listener);
                    _listeners[eventName] = list;
                }
            }

            if (firstListener)
            {
                return SendRequest(new CogSocketListenMessage { Path = eventName }, options);
            }
            else
            {
                var done = new TaskCompletionSource<object>();
                done.SetResult(null);
                return done.Task;
            }
        }


        /// <summary> Removes an event listener. </summary>
        /// <param name="eventName"> The slash-separated path to the event. </param>
        /// <param name="listener"> The event handler that was passed to <see cref="AddListenerAsync"/>
        ///  or null to remove all listeners for the specified event. </param>
        /// <param name="cancellationToken"> A cancellation token that, if cancelled, cancels the returned
        ///  task. </param>
        /// <returns> A task that resolves when the event listener has been removed. </returns>
        public Task<object> RemoveListenerAsync(string eventName,
                                                CogSocketEventHandler listener = null,
                                                CancellationToken cancellationToken = default(CancellationToken))
        {
            return RemoveListenerAsync(eventName, listener, new RequestOptions
            {
                CancellationToken = cancellationToken,
                ResponseTimeout = GetDefaultTimeout()
            });
        }

        /// <summary> Removes an event listener. </summary>
        /// <param name="eventName"> The slash-separated path to the event. </param>
        /// <param name="listener"> The event handler that was passed to <see cref="AddListenerAsync"/>
        ///  or null to remove all listeners for the specified event. </param>
        /// <param name="options"> The options that configure how the request and response are handled. </param>
        /// <returns> A task that resolves when the event listener has been removed. </returns>
        public Task<object> RemoveListenerAsync(string eventName,
                                                CogSocketEventHandler listener,
                                                RequestOptions options)
        {
            var lastListener = false;
            lock (_listeners)
            {
                List<CogSocketEventHandler> listeners;
                if (_listeners.TryGetValue(eventName, out listeners))
                {
                    if (listener != null)
                    {
                        // Remove a specific listener (else remove all of them)
                        listeners.Remove(listener);
                    }

                    if (listener == null || listeners.Count == 0)
                    {
                        lastListener = true;
                        _listeners.Remove(eventName);
                    }
                }
            }

            if (lastListener)
            {
                return SendRequest(new CogSocketUnlistenMessage { Path = eventName }, options);
            }
            else
            {
                var done = new TaskCompletionSource<object>();
                done.SetResult(null);
                return done.Task;
            }
        }

        #endregion

        #region Public events

        /// <summary>
        /// Raised when the socket has been closed.
        /// </summary>
        public event EventHandler Closed
        {
            add { _endpoint.Closed += value; }
            remove { _endpoint.Closed -= value; }
        }

        /// <summary> Raised when the socket has an error. </summary>
        public event EventHandler<CogSocketErrorArgs> Error
        {
            add { _endpoint.Error += value; }
            remove { _endpoint.Error -= value; }
        }

        #endregion

        #region Handling Incoming Requests & Responses

        /// <summary>
        /// Handles incoming message events from the endpoint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void endpoint_OnMessage(object sender, CogSocketMessageEventArgs e)
        {
            try
            {
                var message = e.Message;

                if (message is CogSocketResponseMessage)
                {
                    HandleResponseMessage((CogSocketResponseMessage)message);
                }
                else if (message is CogSocketEventMessage)
                {
                    HandleEventMessage((CogSocketEventMessage)message);
                }
                else
                {
                    throw new Exception("Invalid message type: " + message.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while handling CogSocket message: " + ex.Message);
                if (e.Message.Id != 0)
                {
                    var message = new CogSocketResponseMessage { Id = e.Message.Id, Error = -1 };
                    message.SetPayload(ex.Message, _payloadSerializerSettings);
                    _endpoint.Send(message);
                }
            }
        }

        private void HandleEventMessage(CogSocketEventMessage message)
        {
            lock (_listeners)
            {
                List<CogSocketEventHandler> listeners;
                if (_listeners.TryGetValue(message.Path, out listeners))
                {
                    var payload = message.GetPayload(_payloadSerializerSettings);
                    var args = payload as object[];
                    if (args == null && payload != null)
                        args = new[] { payload };

                    foreach (var handler in listeners)
                    {
                        handler.Invoke(this, new CogSocketEventArgs(args));
                    }
                }
            }

            if (message.Id != 0)
            {
                _endpoint.Send(new CogSocketResponseMessage { Id = message.Id });
            }
        }

        private void HandleResponseMessage(CogSocketMessage message)
        {
            SingleRequestPendingResult requestResult;
            if (_pendingRequests.TryRemove(message.Id, out requestResult))
            {
                try
                {
                    var payload = message.GetPayload(_payloadSerializerSettings);

                    if (message.Error == 0)
                    {
                        requestResult.SetResult(payload);
                    }
                    else
                    {
                        // some sort of error occurred, so wrap the error in our CogSocketException
                        Exception exception;

                        if (payload is Exception)
                        {
                            exception = new CogSocketErrorResponseReceivedException(message.Error, (Exception)payload);
                        }
                        else if (payload != null)
                        {
                            exception = new CogSocketErrorResponseReceivedException(message.Error, payload);
                        }
                        else
                        {
                            exception = new CogSocketErrorResponseReceivedException(message.Error);
                        }

                        requestResult.SetException(exception);
                    }
                }
                catch (Exception e)
                {
                    // if we've reached the point where we have a response, we ALWAYS want to set something
                    // so that the task does not sit there forever.
                    requestResult.SetException(new CogSocketSerializationException(e, message.Body));

                    // then propagate it up further
                    throw;
                }
            }
        }

        #endregion

        #region Sending Outgoing Requests

        /// <summary> Sends an outgoing request. </summary>
        /// <param name="message"> The outgoing message to send. </param>
        /// <param name="options"> The options that configure how the request and response are handled. </param>
        /// <returns> A Task object with which to await the response. </returns>
        private Task<object> SendRequest(CogSocketMessage message, RequestOptions options)
        {
            var id = 0;
            try
            {
                while (true)
                {
                    id = Interlocked.Increment(ref _requestId);
                    if (id > 0) // Almost always the case
                        break;

                    lock (this)
                    {
                        // Rollover to one after next increment
                        if (_requestId < 0)
                            _requestId = 0;
                    }
                }

                var newEntry = new SingleRequestPendingResult(options);

                var wasAdded = _pendingRequests.TryAdd(id, newEntry);
                Debug.Assert(wasAdded, "Entry with id " + id + " could not be added to the dictionary");

                message.Id = id;

                _endpoint.Send(message);
                return newEntry.RequestTask;
            }
            catch (Exception ex)
            {
                SingleRequestPendingResult ignored;
                _pendingRequests.TryRemove(id, out ignored);

                TaskCompletionSource<object> trouble = new TaskCompletionSource<object>();
                trouble.SetException(new CogSocketSendException(ex));
                return trouble.Task;
            }
        }

        public event EventHandler<MessagePayloadPreviewEventArgs> PreviewMessage
        {
            add { _endpoint.PreviewMessage += value; }
            remove { _endpoint.PreviewMessage -= value; }
        }

        #endregion

        private void endpoint_Closed(object sender, EventArgs eventArgs)
        {
            HandleCleanup(isCalledByEndpoint: true);
        }

        #region Private classes

        /// <summary>
        /// Holds a references to the TaskCompletionSource and the time that the task was created.  Allows
        /// checking for tasks that should be timed out.
        /// </summary>
        private class SingleRequestPendingResult
        {
            /// <summary> Callback to use when a CancellationToken is passed into the request. </summary>
            private static readonly Action<object> CancellationCallback = delegate (object obj)
            {
                var self = (SingleRequestPendingResult)obj;
                self.SetCancelled();
            };

            /// <summary> Callback to use when a timer elapses. </summary>
            private static readonly TimerCallback TimerCallback = delegate (object state)
            {
                var self = (SingleRequestPendingResult)state;
                self.HandleTimeout();
            };

            /// <summary>
            ///  If period is zero (0) or negative one (-1) milliseconds and dueTime is positive, callback is
            ///  invoked once; the periodic behavior of the timer is disabled, but can be re-enabled using the
            ///  Change method.
            /// </summary>
            private static readonly TimeSpan IntervalDisabledTime = TimeSpan.FromMilliseconds(0);

            /// <summary> The cancellation registration to use when the result completes. </summary>
            private CancellationTokenRegistration _registration;

            /// <summary>
            /// The completion source that will complete the task that callers will be waiting on.
            /// </summary>
            private readonly TaskCompletionSource<object> _taskSource;

            /// <summary> The time at which the source was created. </summary>
            private readonly DateTime _creationTime;

            /// <summary> The timer that will automatically cancel the task once it elapses. </summary>
            private readonly Timer _timer;

            /// <summary> Default constructor. </summary>
            /// <param name="options"> The options to configure how the system responds to the responses. </param>
            public SingleRequestPendingResult(RequestOptions options)
            {
                _taskSource = new TaskCompletionSource<object>();

                _creationTime = DateTime.UtcNow;

                // only register a token if we have a valid token
                if (options.CancellationToken.CanBeCanceled)
                {
                    _registration = options.CancellationToken.Register(CancellationCallback, this);
                }

                // only register a timer if we have a valid time
                if (options.ResponseTimeout.HasValue)
                {
                    _timer = new Timer(TimerCallback, this, options.ResponseTimeout.Value, IntervalDisabledTime);
                }
            }

            /// <summary> The task that resolves when the request is completed. </summary>
            public Task<object> RequestTask
            {
                get { return _taskSource.Task; }
            }

            /// <summary> Invoked when the task times out.  </summary>
            private void HandleTimeout()
            {
                var timeWaited = DateTime.UtcNow - _creationTime;
                SetException(new CogSocketMessageTimeoutException(timeWaited));
            }

            /// <summary>
            ///  Resolves the associated task to the cancelled state, if it has not already been resolved.
            /// </summary>
            public void SetCancelled()
            {
                // TT38999 Resolve the task on a background thread to avoid invoking callbacks on the receive thread.
                Task.Run(() =>
                {
                    _taskSource.TrySetCanceled();
                    Dispose();
                });
            }

            /// <summary>
            ///  Resolves the associated task with the associated value, if it has not already been resolved.
            /// </summary>
            /// <param name="value"> The value with which to resolve the task. </param>
            public void SetResult(object value)
            {
                // Resolve the task on a background thread to avoid invoking callbacks on the receive thread.
                Task.Run(() =>
                {
                    _taskSource.TrySetResult(value);
                    Dispose();
                });
            }

            /// <summary>
            ///  Resolves the associated task to the exception state, if it has not already been resolved.
            /// </summary>
            /// <param name="exception"> The exception witch which to resolve the task. </param>
            public void SetException(Exception exception)
            {
                // Resolve the task on a background thread to avoid invoking callbacks on the receive thread.
                Task.Run(() =>
                {
                    _taskSource.TrySetException(exception);
                    Dispose();
                });
            }

            private void Dispose()
            {
                _registration.Dispose();

                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }


        }

        #endregion
    }
}
