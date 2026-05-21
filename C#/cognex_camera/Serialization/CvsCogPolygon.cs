// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with a polygon graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Polygon")]
    public class CvsCogPolygon : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogPolygon</c> class.</summary>
        public CvsCogPolygon()
        {
            Points = new double[0];
        }

        /// <summary>Initializes a new instance of the <c>CvsCogPolygon</c> class.</summary>
        /// <param name="points">The set of x,y pairs of doubles defining the polygon.</param>
        public CvsCogPolygon(IEnumerable<double> points)
        {
            Points = new List<double>(points).ToArray();
        }

        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "Polygon"; } }

        /// <summary>Gets or sets a flag indicating whether the polygon should be drawn filled.</summary>
        [JsonProperty(PropertyName = "showFill", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(false)]
        public bool ShowFill { get; set; }

        /// <summary>Gets or sets the polygon's points.</summary>
        [JsonProperty(PropertyName = "points", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double[] Points { get; set; }

        /// <summary>Gets or sets the polygon's length.</summary>
        [JsonProperty(PropertyName = "length", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Length { get; set; }
    }
}