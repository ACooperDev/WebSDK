// Copyright (c) 2017-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates an In-Sight Graphics layer for tool output graphics.
    /// </summary>
    [CvsSerializable(JsonName = "GraphicsLayer")]
    public class CvsCogGraphicsLayer
    {
        /// <summary>Gets or sets the result url.</summary>
        [JsonProperty(PropertyName = "url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "graphics", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JArray Graphics { get; set; }
    }
}