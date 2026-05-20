// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight cross graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Cross")]
    public class CvsCogCross : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogCross</c> class.</summary>
        public CvsCogCross()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogCross</c> class.</summary>
        /// <param name="x">The X location of the graphic in pixels.</param>
        /// <param name="y">The Y location of the graphic in pixels.</param>
        /// <param name="w">The width of the graphic in pixels.</param>
        /// <param name="h">The height of the graphic in pixels.</param>
        /// <param name="a">The angle of the graphic in degrees.</param>
        /// <param name="color">32-bit integer ARGB representation of the color of the graphic.</param>
        /// <param name="cellLocation">Cell location of the tool that generated the graphic.</param>
        public CvsCogCross(double x, double y, double w, double h, double a, int color = DefaultColor, string cellLocation = "")
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Angle = a;
            Color = color;
            CellLocation = cellLocation;
        }

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