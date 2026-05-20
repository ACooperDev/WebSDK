// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight graphic fixture.
    /// </summary>
    [CvsSerializable(JsonName = "Fixture")]
    public class CvsCogFixture : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogFixture</c> class.</summary>
        public CvsCogFixture()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogFixture</c> class.</summary>
        /// <param name="x">The X location of the graphic in pixels.</param>
        /// <param name="y">The Y location of the graphic in pixels.</param>
        /// <param name="angle">The angle of the graphic in pixels.</param>
        /// <param name="color">32-bit integer ARGB representation of the color of the graphic.</param>
        /// <param name="cellLocation">Cell location of the tool that generated the graphic.</param>
        public CvsCogFixture(double x,
                             double y,
                             double angle,
                             int color = DefaultColor,
                             string cellLocation = "")
        {
            X = x;
            Y = y;
            Angle = angle;
            Color = color;
            CellLocation = cellLocation;
        }

        /// <summary>Gets or sets the X location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        /// <summary>Gets or sets the Y location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }

        /// <summary>Gets or sets the angle of the graphic.</summary>
        [JsonProperty(PropertyName = "angle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Angle { get; set; }
    }
}