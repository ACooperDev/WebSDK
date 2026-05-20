// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using System.ComponentModel;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight line graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Line")]
    public class CvsCogLine : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogLine</c> class.</summary>
        public CvsCogLine()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogLine</c> class.</summary>
        /// <param name="x0">The X location of the first point of the graphic in pixels.</param>
        /// <param name="y0">The Y location of the first point of the graphic in pixels.</param>
        /// <param name="x1">The X location of the second point of the graphic in pixels.</param>
        /// <param name="y1">The Y location of the second point of the graphic in pixels.</param>
        /// <param name="adornment0">End adornment value for the first point. 0 for none.</param>
        /// <param name="adornment1">End adornment value for the second point. 0 for none.</param>
        /// <param name="color">32-bit integer ARGB representation of the color of the graphic.</param>
        /// <param name="cellLocation">Cell location of the tool that generated the graphic.</param>
        public CvsCogLine(double x0,
                          double y0,
                          double x1,
                          double y1,
                          int adornment0 = 0,
                          int adornment1 = 0,
                          int color = DefaultColor,
                          string cellLocation = "")
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
            Adornment0 = adornment0;
            Adornment1 = adornment1;
            Color = color;
            CellLocation = cellLocation;
        }

        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "Line"; } }

        /// <summary>Gets or sets the type of adornment for the first point in the graphic.</summary>
        [JsonProperty(PropertyName = "adornment0", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(0)]
        public int Adornment0 { get; set; }

        /// <summary>Gets or sets the type of adornment for the second point in the graphic.</summary>
        [JsonProperty(PropertyName = "adornment1", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(0)]
        public int Adornment1 { get; set; }

        /// <summary>Gets or sets the X location of the first point of graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x0", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X0 { get; set; }

        /// <summary>Gets or sets the Y location of the first point of graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y0", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y0 { get; set; }

        /// <summary>Gets or sets the X location of the second point of graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X1 { get; set; }

        /// <summary>Gets or sets the Y location of the second point of graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y1 { get; set; }
    }
}