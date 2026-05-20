// Copyright (c) 2015-2021 Cognex Corporation. All Rights Reserved

using System;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Designates a class that does not need to be serialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
    [CvsChildSerializable]
    public class CvsChildSerializableAttribute : Attribute
    {
    }
}