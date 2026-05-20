// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with a polyline graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Polyline")]
    public class CvsCogPolyline : CvsCogPointList
    {
        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "Polyline"; } }
    }
}