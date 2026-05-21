//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;

namespace Cognex.SimpleCogSocket
{
    /// <summary> Argument information when a new CogSocket message is received. </summary>
    internal class CogSocketMessageEventArgs : EventArgs
    {
        /// <summary> The message that was received. </summary>
        public readonly CogSocketMessage Message;

        /// <summary> Constructor. </summary>
        /// <param name="message"> The message that was received. </param>
        public CogSocketMessageEventArgs(CogSocketMessage message)
        {
            Message = message;
        }
    }
}
