// Copyright (c) 2017-2021 Cognex Corporation. All Rights Reserved

using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>Base class for those encapsulating an InSight graphic.</summary>
    [CvsSerializable(JsonName = "ShapesBase")]
    public abstract class CvsCogShape : ICloneable
    {
        /// <summary>
        /// Default color value for spreadsheet graphics.
        /// </summary>
        /// <remarks>
        /// If the camera doesn't send us a color, this is the one that Designer should use.
        /// The value represents a dark blue w/ ARGB 0xFF00008B.
        /// </remarks>
        internal const int DefaultColor = -16777077;

        public CvsCogShape()
        {

        }

        /// <summary>
        /// Copies the shape.
        /// </summary>
        /// <returns>A new instance of the shape.</returns>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        [JsonProperty(PropertyName = "source", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CellLocation { get; set; }

        [JsonProperty(PropertyName = "color", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(DefaultColor)]
        public int Color { get; set; }

        [JsonProperty(PropertyName = "graphicId", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(-1)]
        public int GraphicId { get; set; }

        [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.Ignore), DefaultValue("")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "label", DefaultValueHandling = DefaultValueHandling.Ignore), DefaultValue("")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "font", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue("Arial")]
        public string Font { get; set; }

        [JsonProperty(PropertyName = "fontSize", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(9)]
        public float FontSize { get; set; }

        [JsonProperty(PropertyName = "lineThickness", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(1)]
        public int LineThickness { get; set; }
    }
}