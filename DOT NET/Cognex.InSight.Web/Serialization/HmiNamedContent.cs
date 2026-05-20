// Copyright (c) 2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// A class that allows named content to be sent to the device.
    /// </summary>
    [CvsSerializable(JsonName = "HmiNamedContent")]
    public class HmiNamedContent
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }
}
