// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight Offset graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Offset")]
    public class CvsCogOffset : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogOffset</c> class.</summary>
        public CvsCogOffset()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogOffset</c> class.</summary>
        /// <param name="x">The X location of the graphic in pixels.</param>
        /// <param name="y">The Y location of the graphic in pixels.</param>
        /// <param name="xo">The X origin of the graphic in pixels.</param>
        /// <param name="yo">The Y origin of the graphic in pixels.</param>
        /// <param name="a">The angle of the graphic in degrees.</param>
        public CvsCogOffset(double x, double y, double xo, double yo, double a)
        {
            X = x;
            Y = y;
            XOrigin = xo;
            YOrigin = yo;
            Angle = a;
        }

        /// <summary>Gets or sets the X location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        /// <summary>Gets or sets the Y location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }

        /// <summary>Gets or sets the X origin of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "xOrigin", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double XOrigin { get; set; }

        /// <summary>Gets or sets the Y origin of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "yOrigin", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double YOrigin { get; set; }

        /// <summary>Gets or sets the angle of the graphic in degrees.</summary>
        [JsonProperty(PropertyName = "angle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Angle { get; set; }
    }
}