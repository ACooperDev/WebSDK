// Copyright (c) 2021 Cognex Corporation. All Rights Reserved

using System;
using System.Reflection;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    ///  Uses <see cref="CvsSerializableAttribute"/> to find types that can be registered with
    ///  <see cref="JsonRemoteSerializer"/>.
    /// </summary>
    [CvsChildSerializable]
    public static class SerializedTypeRegistration
    {
        /// <summary>
        ///  Registers the types located in this assembly that can be used with CogSocket.
        /// </summary>
        public static void RegisterBuiltIns()
        {
            RegisterSerializableTypes(typeof(SerializedTypeRegistration).Assembly);
        }

        /// <summary>
        ///  Registers all the types in an assembly that contain a <see cref="CvsSerializableAttribute"/>
        ///  with <see cref="JsonRemoteSerializer"/>.
        /// </summary>
        public static void RegisterSerializableTypes(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var serAttrs = type.GetCustomAttributes(typeof(CvsSerializableAttribute), false);
                if (serAttrs.Length == 0)
                {
                    continue;
                }
                var ser = (CvsSerializableAttribute)serAttrs[0];

                Cognex.SimpleCogSocket.JsonSerializer.TypeNameBinder.Register(ser.JsonName, type);
            }
        }
    }
}