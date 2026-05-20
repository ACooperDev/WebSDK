// Copyright (c) 2017-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight filled box graphic.
    /// </summary>
    [CvsSerializable(JsonName = "FilledBox")]
    public class CvsCogFilledBox : CvsCogShape
    {
        /// <summary>Gets or sets the X location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        /// <summary>Gets or sets the Y location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }

        /// <summary>Gets or sets the width of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "w", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Width { get; set; }

        /// <summary>Gets or sets the height of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "h", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Height { get; set; }

        /// <summary>Gets or sets the angle of the graphic in degrees.</summary>
        [JsonProperty(PropertyName = "angle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Angle { get; set; }
    }
}