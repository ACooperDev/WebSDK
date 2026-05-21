//*******************************************************************************
// Copyright (c) 2021 Cognex Corporation. All Rights Reserved
//*******************************************************************************

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cognex.SimpleCogSocket
{
    [Obfuscation(Exclude = true)]
    internal abstract class CogSocketMessage
    {
        [JsonProperty(PropertyName = "id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Int32 Id { get; set; }

        [JsonProperty(PropertyName = "error", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Int32 Error { get; set; }

        [JsonProperty(PropertyName = "path", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        internal JToken Body { get; set; }

        public object GetPayload(JsonSerializerSettings serializerSettings)
        {
            if (Body == null)
                return null;

            var serializer = Newtonsoft.Json.JsonSerializer.Create(serializerSettings);
            return serializer.Deserialize(Body.CreateReader(), typeof(object));
        }

        public void SetPayload(object payload, JsonSerializerSettings serializerSettings)
        {
            if (payload == null)
                Body = null;
            else
                Body = JToken.Parse(JsonConvert.SerializeObject(payload, serializerSettings));
        }

    }

    [Obfuscation(Exclude = true)]
    [DataContract(Name = "get")]
    internal class CogSocketGetMessage : CogSocketMessage
    {
    }

    [Obfuscation(Exclude = true)]
    [DataContract(Name = "put")]
    internal class CogSocketPutMessage : CogSocketMessage
    {
    }

    [Obfuscation(Exclude = true)]
    [DataContract(Name = "post")]
    internal class CogSocketPostMessage : CogSocketMessage
    {
    }

    [Obfuscation(Exclude = true)]
    [DataContract(Name = "event")]
    internal class CogSocketEventMessage : CogSocketMessage
    {
    }

    [Obfuscation(Exclude = true)]
    [DataContract(Name = "listen")]
    internal class CogSocketListenMessage : CogSocketMessage
    {
    }

    [Obfuscation(Exclude = true)]
    [DataContract(Name = "unlisten")]
    internal class CogSocketUnlistenMessage : CogSocketMessage
    {
    }

    [Obfuscation(Exclude = true)]
    [DataContract(Name = "resp")]
    internal class CogSocketResponseMessage : CogSocketMessage
    {
    }

    /// <summary>
    /// An event handler for incoming CogSocket events.
    /// </summary>
    /// <param name="sender">The CogSocket from which the event was recevied.</param>
    /// <param name="e">The event arguments.</param>
    public delegate void CogSocketEventHandler(object sender, CogSocketEventArgs e);
}
