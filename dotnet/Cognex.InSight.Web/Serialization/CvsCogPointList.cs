// Copyright (c) 2018-2021 Cognex Corporation. All Rights Reserved

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates a generic list of points.
    /// </summary>
    [CvsSerializable(JsonName = "PointList")]
    public class CvsCogPointList : CvsCogShape
    {
        /// <summary>Initializes an empty instance of the <c>CvsCogPointList</c> class.</summary>
        public CvsCogPointList()
        {
            Points = new double[0];
        }

        /// <summary>Initializes a new instance of the <c>CvsCogPointList</c> class.</summary>
        /// <param name="points">The set of x,y pairs of doubles defining the point list.</param>
        public CvsCogPointList(IEnumerable<double> points)
        {
            Points = new List<double>(points).ToArray();
        }

        /// <summary>Gets or sets the list of points.</summary>
        [JsonProperty(PropertyName = "points", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double[] Points { get; set; }
    }
}
