// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight point graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Point")]
    public class CvsCogPoint : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogPoint</c> class.</summary>
        public CvsCogPoint()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogPoint</c> class.</summary>
        /// <param name="x">The X location of the graphic in pixels.</param>
        /// <param name="y">The Y location of the graphic in pixels.</param>
        /// <param name="color">32-bit integer ARGB representation of the color of the graphic.</param>
        /// <param name="cellLocation">Cell location of the tool that generated the graphic.</param>
        public CvsCogPoint(double x, double y, int color = DefaultColor, string cellLocation = "")
        {
            X = x;
            Y = y;
            Color = color;
            CellLocation = cellLocation;
        }

        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "Point"; } }

        /// <summary>Gets or sets the X location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        /// <summary>Gets or sets the Y location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }
    }
}