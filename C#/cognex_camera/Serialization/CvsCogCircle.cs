// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight circle graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Circle")]
    public class CvsCogCircle : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogCircle</c> class.</summary>
        public CvsCogCircle()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogCircle</c> class.</summary>
        /// <param name="x">The X location of the graphic in pixels.</param>
        /// <param name="y">The Y location of the graphic in pixels.</param>
        /// <param name="radius">The radius of the graphic in pixels.</param>
        /// <param name="color">32-bit integer ARGB representation of the color of the graphic.</param>
        /// <param name="cellLocation">Cell location of the tool that generated the graphic.</param>
        public CvsCogCircle(double x, double y, double radius, int color = DefaultColor, string cellLocation = "")
        {
            X = x;
            Y = y;
            Radius = radius;
            Color = color;
            CellLocation = cellLocation;
        }

        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "Circle"; } }

        /// <summary>Gets or sets the X location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        /// <summary>Gets or sets the Y location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }

        /// <summary>Gets or sets the radius of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "radius", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Radius { get; set; }
    }
}