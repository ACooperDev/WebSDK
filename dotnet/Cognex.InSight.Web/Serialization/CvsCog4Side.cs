// Copyright (c) 2017-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight 4-side graphic.
    /// </summary>
    [CvsSerializable(JsonName = "4Side")]
    public class CvsCog4Side : CvsCogShape
    {
        public CvsCog4Side()
        {
        }

        /// <summary>
        /// Gets or sets the X location of the first point in the graphic.
        /// </summary>
        [JsonProperty(PropertyName = "x0", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X0 { get; set; }

        /// <summary>
        /// Gets or sets the X location of the second point in the graphic.
        /// </summary>
        [JsonProperty(PropertyName = "x1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X1 { get; set; }

        /// <summary>
        /// Gets or sets the X location of the third point in the graphic.
        /// </summary>
        [JsonProperty(PropertyName = "x2", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X2 { get; set; }

        /// <summary>
        /// Gets or sets the X location of the fourth point in the graphic.
        /// </summary>
        [JsonProperty(PropertyName = "x3", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X3 { get; set; }

        /// <summary>
        /// Gets or sets the Y location of the first point in the graphic.
        /// </summary>
        [JsonProperty(PropertyName = "y0", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y0 { get; set; }

        /// <summary>
        /// Gets or sets the Y location of the second point in the graphic.
        /// </summary>
        [JsonProperty(PropertyName = "y1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y1 { get; set; }

        /// <summary>
        /// Gets or sets the Y location of the third point in the graphic.
        /// </summary>
        [JsonProperty(PropertyName = "y2", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y2 { get; set; }

        /// <summary>
        /// Gets or sets the Y location of the fourth point in the graphic.
        /// </summary>
        [JsonProperty(PropertyName = "y3", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y3 { get; set; }
    }
}