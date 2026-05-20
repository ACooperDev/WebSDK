//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Cognex.SimpleCogSocket
{
    /// <summary> An exception that occurs while sending or receiving a CogSocket message. </summary>
    public abstract class CogSocketException : Exception
    {
        /// <summary> Specialized default constructor for use only by derived class. </summary>
        protected CogSocketException()
            : base()
        {
        }

        /// <summary> Constructor. </summary>
        /// <param name="message"> The message indicating the purpose of the exception. </param>
        protected CogSocketException(string message)
            : base(message)
        {
        }

        /// <summary> Constructor. </summary>
        /// <param name="message"> The message indicating the purpose of the exception. </param>
        /// <param name="innerException"> The inner exception. </param>
        protected CogSocketException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary> Occurs when a client does not receive a response from a server in a timely manner. </summary>
    public sealed class CogSocketMessageTimeoutException : CogSocketException
    {
        /// <summary> Constructor. </summary>
        /// <param name="timeWaited"> The time waited for a response. </param>
        public CogSocketMessageTimeoutException(TimeSpan timeWaited)
            : base(String.Format("Message was not received from endpoint after waiting {0}", timeWaited))
        {
        }
    }

    /// <summary> An exception that occurs when an error was encountered while sending a request. </summary>
    public sealed class CogSocketSendException : CogSocketException
    {
        public CogSocketSendException(Exception innerException)
            : base("Error occurred while sending request", innerException)
        {
        }
    }

    /// <summary>
    ///  An exception that indicates that while parsing a response, an exception was caught.
    /// </summary>
    public sealed class CogSocketSerializationException : CogSocketException
    {
        /// <summary> Constructor. </summary>
        /// <param name="innerException"> The inner exception that is to be wrapped. </param>
        /// <param name="body"> The body of the message coming in. </param>
        public CogSocketSerializationException(Exception innerException, JToken body)
            : base("Exception occurred while deserializing response", innerException)
        {
            Body = body;
        }

        /// <summary> The body of the message that caused the exception. </summary>
        public JToken Body { get; private set; }
    }

    /// <summary> An exception that occurs when an error was received from the endpoint. </summary>
    public sealed class CogSocketErrorResponseReceivedException : CogSocketException
    {
        /// <summary> Constructor. </summary>
        /// <param name="errorCode"> The error number received from the endpoint. </param>
        /// <param name="innerException"> The inner exception. </param>
        public CogSocketErrorResponseReceivedException(int errorCode, Exception innerException)
            : base(FormatMessage(errorCode), innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary> Constructor. </summary>
        /// <param name="errorCode"> The error number received from the endpoint. </param>
        public CogSocketErrorResponseReceivedException(int errorCode)
            : base(FormatMessage(errorCode))
        {
            ErrorCode = errorCode;
        }

        /// <summary> Constructor. </summary>
        /// <param name="errorCode"> The error number received from the endpoint. </param>
        /// <param name="additionalData"> The data received as part of the response from the endpoint. </param>
        public CogSocketErrorResponseReceivedException(int errorCode, object additionalData)
            : base(FormatMessage(errorCode, additionalData.ToString()))
        {
            AdditionalData = additionalData;
            ErrorCode = errorCode;
        }

        /// <summary> The error code received from the endpoint. </summary>
        public int ErrorCode { get; private set; }

        /// <summary> Any additional data provided as part of the error message. </summary>
        public object? AdditionalData { get; private set; }

        /// <summary> Format the error message for the given error code. </summary>
        /// <param name="errorId"> The error number received from the endpoint. </param>
        /// <param name="additionalData"> Any additional data provided as part of the error message. </param>
        /// <returns> The formatted message. </returns>
        private static string FormatMessage(int errorId, string additionalData = null)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Error received from endpoint (error code {0:X})", errorId);

            if (additionalData != null)
            {
                builder.AppendLine(":");
                builder.AppendLine(additionalData);
            }

            return builder.ToString();
        }
    }
}
