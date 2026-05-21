//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Cognex.SimpleCogSocket
{
    /// <summary>
    /// Provides a static registry of types that can be serialized.
    /// </summary>
    public class TypeNameBinder : SerializationBinder
    {
        /// <summary>
        /// Registers all types in an assembly that have a <see />.
        /// </summary>
        /// <param name="asm">An assembly containing types to be registered.</param>
        public void RegisterAssembly(Assembly asm)
        {
            Register(asm.GetTypes());
        }

        /// <summary>
        /// Registers one oe more types for serialization.
        /// </summary>
        /// <param name="types">One or more types to be registered.</param>
        public void Register(params Type[] types)
        {
            foreach (Type type in types)
            {
                string name = null;

                var dcAttrs = type.GetCustomAttributes(typeof(DataContractAttribute), false);
                if (dcAttrs.Length > 0)
                {
                    var dc = (DataContractAttribute)dcAttrs[0];
                    name = dc.Name;
                }

                if (name == null)
                {
                    continue;
                }

                Register(name, type);
            }
        }

        public void Register(string name, Type type)
        {
            _typeFromName[name] = type;
            _nameFromType[type] = name;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            if (_nameFromType.ContainsKey(serializedType))
                typeName = _nameFromType[serializedType];
            else
                typeName = serializedType.AssemblyQualifiedName;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (_typeFromName.ContainsKey(typeName))
            {
                return _typeFromName[typeName];
            }
            else
            {
                // if no type is found, bind to JToken so that registering types is not required.
                return Type.GetType(typeName, false) ?? typeof(JToken);
            }
        }

        private Dictionary<Type, string> _nameFromType = new Dictionary<Type, string>();
        private Dictionary<string, Type> _typeFromName = new Dictionary<string, Type>();
    }

}
