//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Reflection;

namespace Cognex.SimpleCogSocket
{
    /// <summary>
    /// Arguments provided by incoming CogSocket events.
    /// </summary>
    public class CogSocketEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the event argument values. Will return a zero-length array (not null)
        /// if there are no event argument values.
        /// </summary>
        public readonly object[] Args;

        public CogSocketEventArgs(object[] args = null)
        {
            Args = args ?? EmptyArgs;
        }

        public new static readonly CogSocketEventArgs Empty = new CogSocketEventArgs();

        static readonly object[] EmptyArgs = new object[0];

        internal static CogSocketEventArgs FromEventArgs(EventArgs e)
        {
            Type argsType = e.GetType();
            if (argsType == typeof(EventArgs))
                return Empty;

            CogSocketEventArgs? ea = e as CogSocketEventArgs;
            if (ea == null)
            {
                PropertyInfo[] props = argsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Array.Sort(props, (a, b) => string.Compare(a.Name, b.Name));
                object[] vals = new object[props.Length];
                for (var i = 0; i < props.Length; ++i)
                {
                    vals[i] = props[i].GetValue(e);
                }
                ea = new CogSocketEventArgs(vals);
            }
            return ea;
        }
    }
}
