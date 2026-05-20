// Copyright (c) 2023 Cognex Corporation. All Rights Reserved

using System.ComponentModel;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight line graphic.
    /// </summary>
    [CvsSerializable(JsonName = "LineList")]
    public class CvsCogLineList : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogLineList</c> class.</summary>
        public CvsCogLineList()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogLine</c> class.</summary>
        /// <param name="lines">The array of point for each line segment.</param>
        public CvsCogLineList(double[] lines,
                              int color = DefaultColor,
                              string cellLocation = "")
        {
            Lines = lines;
            Color = color;
            CellLocation = cellLocation;
        }

        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "LineList"; } }

        /// <summary>Gets or sets the array of points for the line segments.</summary>
        [JsonProperty(PropertyName = "lines", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double[] Lines { get; set; }
    }
}