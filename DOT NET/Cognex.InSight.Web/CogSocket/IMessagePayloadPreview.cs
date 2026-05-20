//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cognex.SimpleCogSocket
{
    /// <summary> Allows previewing message data before it sent across the wire. </summary>
    public interface IMessagePayloadPreview
    {
        /// <summary>
        /// Invoked when a message is received before it has been deserialized or before a message is sent
        /// and has already been serialized.
        /// </summary>
        event EventHandler<MessagePayloadPreviewEventArgs> PreviewMessage;
    }

    /// <summary> Event arguments for an implementation of IMessagePayloadPreview. </summary>
    public class MessagePayloadPreviewEventArgs : EventArgs
    {
        /// <summary> The data associated with the message </summary>
        public object Payload { get; private set; }

        /// <summary> True if the message is being sent. </summary>
        public bool IsOutgoing { get; private set; }

        /// <summary> True if the message is being received. </summary>
        public bool IsIncoming
        {
            get { return !IsOutgoing; }
        }

        private MessagePayloadPreviewEventArgs(bool isOutgoing, object payload)
        {
            IsOutgoing = isOutgoing;
            Payload = payload;
        }

        /// <summary> Creates a new MessagePayloadPreviewEventArgs that represents an incoming message. </summary>
        /// <param name="payload"> The data associated with the message. </param>
        /// <returns> The MessagePayloadPreviewEventArgs. </returns>
        public static MessagePayloadPreviewEventArgs Incoming(object payload)
        {
            return new MessagePayloadPreviewEventArgs(false, payload);
        }

        /// <summary> Creates a new MessagePayloadPreviewEventArgs that represents an outgoing message. </summary>
        /// <param name="payload"> The data associated with the message. </param>
        /// <returns> The MessagePayloadPreviewEventArgs. </returns>
        public static MessagePayloadPreviewEventArgs Outgoing(object payload)
        {
            return new MessagePayloadPreviewEventArgs(true, payload);
        }
    }
}
