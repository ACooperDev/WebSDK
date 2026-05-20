//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cognex.SimpleCogSocket
{
    /// <summary> Provides communication plumbing for CogSocket. </summary>
    internal interface ICogSocketEndpoint : IMessagePayloadPreview
    {
        /// <summary> Sends a message to the server. </summary>
        /// <param name="message"> The message to send. </param>
        void Send(CogSocketMessage message);

        /// <summary> Opens the connection to the server. </summary>
        /// <returns> A Task that completes when the connection has been established. </returns>
        Task Connect();

        /// <summary> Closes the connect to the server. </summary>
        void Close();

        /// <summary> Event that fires when the connection has been established. </summary>
        event EventHandler Opened;

        /// <summary> Event that fires when the connection has been closed. </summary>
        event EventHandler Closed;

        /// <summary>
        /// Event that fires when an error or exception has been received as part of the connection or from.
        /// </summary>
        event EventHandler<CogSocketErrorArgs> Error;

        /// <summary> Event that fires when a new message has been received. </summary>
        event EventHandler<CogSocketMessageEventArgs> Message;

        //TraceListener TraceListener { get; set; }
    }
}
