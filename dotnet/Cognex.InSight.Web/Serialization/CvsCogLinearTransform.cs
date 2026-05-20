// Copyright (c) 2015-2021 Cognex Corporation. All Rights Reserved

using System.ComponentModel;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates a linear transform.
    /// </summary>
    [CvsSerializable(JsonName = "LinearTransform")]
    public class CvsCogLinearTransform
    {
        public CvsCogLinearTransform()
        {

        }

        public CvsCogLinearTransform(double m00, double m01,
                                     double m10, double m11,
                                     double xOffset, double yOffset)
        {
            M00 = m00;
            M01 = m01;
            M10 = m10;
            M11 = m11;
            XOffset = xOffset;
            YOffset = yOffset;
        }

        /// <summary>Gets or sets the term of the scale/rotation matrix in row 0, column 0.</summary>
        [JsonProperty(PropertyName = "m00", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(1)]
        public double M00 { get; internal set; }

        /// <summary>Gets or sets the term of the scale/rotation matrix in row 0, column 1.</summary>
        [JsonProperty(PropertyName = "m01", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        public double M01 { get; internal set; }

        /// <summary>Gets or sets the term of the scale/rotation matrix in row 1, column 0.</summary>
        [JsonProperty(PropertyName = "m10", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        public double M10 { get; internal set; }

        /// <summary>Gets or sets the term of the scale/rotation matrix in row 1, column 1.</summary>
        [JsonProperty(PropertyName = "m11", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(1)]
        public double M11 { get; internal set; }

        /// <summary>Gets or sets the horizontal offset for the transform.</summary>
        [JsonProperty(PropertyName = "tx", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        public double XOffset { get; internal set; }

        /// <summary>Gets or sets the vertical offset for the transform.</summary>
        [JsonProperty(PropertyName = "ty", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        public double YOffset { get; internal set; }
    }
}