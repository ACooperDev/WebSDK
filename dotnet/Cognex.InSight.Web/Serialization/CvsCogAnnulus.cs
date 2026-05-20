// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;
using System.Drawing;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight annulus graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Annulus")]
    public class CvsCogAnnulus : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogRegion</c> class.</summary>
        public CvsCogAnnulus()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogRegion</c> class.</summary>
        /// <param name="x">The X co-ordinate of the center of the graphic in pixels.</param>
        /// <param name="y">The Y co-ordinate of the center of the graphic in pixels.</param>
        /// <param name="innerRadius">The inner radius of the graphic in pixels.</param>
        /// <param name="outerRadius">The outer radius of the graphic in pixels.</param>
        /// <param name="color">32-bit integer ARGB representation of the color of the graphic.</param>
        /// <param name="cellLocation">Cell location of the tool that generated the graphic.</param>
        public CvsCogAnnulus(double x, double y, double innerRadius, double outerRadius, int color = DefaultColor, string cellLocation = "")
        {
            X = x;
            Y = y;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            Color = color;
            CellLocation = cellLocation;
        }

        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "Annulus"; } }

        /// <summary>Gets or sets the X location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        /// <summary>Gets or sets the Y location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }

        /// <summary>Gets or sets the inner radius of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "innerRadius", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double InnerRadius { get; set; }

        /// <summary>Gets or sets the outer radius of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "outerRadius", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double OuterRadius { get; set; }
    }
}