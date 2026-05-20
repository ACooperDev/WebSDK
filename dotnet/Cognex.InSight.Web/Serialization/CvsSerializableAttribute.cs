// Copyright (c) 2021 Cognex Corporation. All Rights Reserved

using System;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Provides information about a serializable class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [CvsChildSerializable]
    public class CvsSerializableAttribute : Attribute
    {
        public CvsSerializableAttribute()
        {

        }

        public CvsSerializableAttribute(string jsonName)
        {
            JsonName = jsonName;
        }

        /// <summary>
        /// Specifies the type name to be used for JSON encoding. If null, the
        /// .NET type's unqualified name will be used.
        /// </summary>
        public string JsonName { get; set; }
    }
}