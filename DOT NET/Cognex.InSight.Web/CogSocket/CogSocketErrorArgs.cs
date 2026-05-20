//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.SimpleCogSocket
{
    /// <summary>
    ///  Holds the arguments that are used when informing callers of errors that occur at the
    ///  websocket layer of CogSocket.
    /// </summary>
    public class CogSocketErrorArgs : EventArgs
    {
        /// <summary> Constructor. </summary>
        /// <param name="message"> An error message associated with the error. </param>
        /// <param name="exception"> The exception associated with the error. </param>
        public CogSocketErrorArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        /// <summary> An error message associated with the error. </summary>
        public string Message { get; set; }

        /// <summary> The exception associated with the error. </summary>
        public Exception Exception { get; set; }
    }
}
