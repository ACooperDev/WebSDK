// Copyright (c) 2017-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight text graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Text")]
    public class CvsCogText : CvsCogShape
    {
        public CvsCogText()
        {
            BackgroundColor = 0;
        }

        [JsonProperty(PropertyName = "bgColor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }
    }
}