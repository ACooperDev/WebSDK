//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

// Define this symbols to dump JSON to the debug console by default
// #define DEBUG_SERIALIZE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Serialization;

namespace Cognex.SimpleCogSocket
{
    public class JsonSerializer
    {
        private static TypeNameBinder __typeNameBinder = new TypeNameBinder();

        private JsonSerializerSettings _jsonSettings;

        public static TypeNameBinder TypeNameBinder
        {
            get { return __typeNameBinder; }
        }

        public JsonSerializer(bool verbose)
        {
            _jsonSettings = new JsonSerializerSettings();
            _jsonSettings.TypeNameHandling = TypeNameHandling.Objects;

            _jsonSettings.Binder = __typeNameBinder;
            _jsonSettings.Converters.Add(JsonObjectConverter.Instance);
            _jsonSettings.ContractResolver = ContractResolver.Instance;

            if (verbose)
            {
                _jsonSettings.Formatting = Formatting.Indented;
            }

#if DEBUG_SERIALIZE
      TraceListener = new ConsoleTraceListener();
#endif
        }

        public JsonSerializerSettings Settings
        {
            get
            {
                return _jsonSettings;
            }
        }

        public TraceListener TraceListener { get; set; }

        public object DeserializeObject(string json)
        {
            var obj = JsonConvert.DeserializeObject(json, _jsonSettings);
            if (TraceListener != null)
            {
                TraceListener.WriteLine("JSON " + json + " -> " + obj.GetType().Name + "TIME: " + Environment.TickCount);
            }
            return obj;
        }

        public string SerializeObject(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, _jsonSettings);
            if (TraceListener != null)
            {
                TraceListener.WriteLine(obj.GetType().Name + " -> JSON " + json + "TIME: " + Environment.TickCount);
            }
            return json;
        }

        /// <summary>
        /// Handles conversion to 'object' properties, converting arrays into an object[] array.
        /// Without this converter, arrays will get deserialized as a Newtonsoft.Json.Linq.JArray. 
        /// </summary>
        private class JsonObjectConverter : JsonConverter
        {
            /// <summary>
            /// Singleton instance, no need to create more than one.
            /// </summary>
            public static readonly JsonObjectConverter Instance = new JsonObjectConverter();

            /// <summary>
            /// Yes we can read JSON.
            /// </summary>
            public override bool CanRead
            {
                get { return true; }
            }

            /// <summary>
            /// But no we can't write it (use default serialization for that, no need for anything special).
            /// </summary>
            public override bool CanWrite
            {
                get { return false; }
            }


            /// <summary>
            /// This converter is only needed when writing 'object' properties.
            /// </summary>
            /// <param name="objectType">Type of the property being written.</param>
            /// <returns>True if this converter should be used.</returns>
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(object);
            }

            /// <summary>
            /// This is called when reading a property's value.
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="objectType"></param>
            /// <param name="existingValue"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            public override object ReadJson(JsonReader reader, Type objectType,
                                            object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    // This coerces all array values into 'object[]'
                    return serializer.Deserialize<object[]>(reader);
                }
                else
                {
                    // Use the default serialization for everything else
                    return serializer.Deserialize(reader);
                }
            }

            /// <summary>
            /// Should never be called because CanWrite = false.
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="value"></param>
            /// <param name="serializer"></param>
            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public class DictionaryConverter : JsonConverter
        {
            public static readonly DictionaryConverter Instance = new DictionaryConverter();

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(Dictionary<string, object>));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                var dict = value as Dictionary<string, object>;
                if (dict != null)
                {
                    writer.WriteStartObject();

                    foreach (var key in dict.Keys)
                    {
                        writer.WritePropertyName(key);
                        serializer.Serialize(writer, dict[key]);
                    }

                    writer.WriteEndObject();
                }
            }
        }

        public class ContractResolver : DefaultContractResolver
        {
            public static readonly ContractResolver Instance = new ContractResolver();

            protected override JsonContract CreateContract(Type objectType)
            {
                var contract = base.CreateContract(objectType);

                if (objectType == typeof(Dictionary<string, object>))
                {
                    contract.Converter = DictionaryConverter.Instance;
                }

                return contract;
            }
        }

    }
}
