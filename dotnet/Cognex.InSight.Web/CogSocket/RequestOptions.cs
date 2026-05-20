//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Threading;

namespace Cognex.SimpleCogSocket
{
    /// <summary> Options that can be set when sending a CogSocket request. </summary>
    public struct RequestOptions
    {
        private TimeSpan? _responseTimeout;

        /// <summary> The cancellation token to use with the request. </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        ///  The time to wait before automatically resolving the associated task with a timeout exception.
        ///  Default is no timeout.
        /// </summary>
        /// <remarks>
        ///  Only comes into effect when the remote device does not respond.  If null, the task will never
        ///  timeout, so if a timeout is desired, this member MUST be set.
        ///  
        ///  A value of Timeout.InfiniteTimeSpan will set the value to null. ALl other values must
        ///  represent a timespan greater than zero.
        /// </remarks>
        public TimeSpan? ResponseTimeout
        {
            get { return _responseTimeout; }
            set
            {
                if (value == Timeout.InfiniteTimeSpan || value == null)
                {
                    _responseTimeout = null;
                }
                else if (value.Value.TotalMilliseconds <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value",
                        "value must be null, Timeout.InfiniteTimeSpan, or represent a timeout that is greater than 0");
                }
                else
                {
                    _responseTimeout = value;
                }
            }
        }
    }
}
