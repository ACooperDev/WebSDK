// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with a blob graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Blob")]
    public class CvsCogBlob : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogBlob</c> class.</summary>
        public CvsCogBlob()
        {
            Points = new double[0];
        }

        /// <summary>Gets or sets the blob's angle.</summary>
        [JsonProperty(PropertyName = "angle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Angle { get; set; }

        /// <summary>Gets or sets the blob's area.</summary>
        [JsonProperty(PropertyName = "area", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Area { get; set; }

        /// <summary>Gets or sets the blob's elongation.</summary>
        [JsonProperty(PropertyName = "elongation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Elongation { get; set; }

        /// <summary>Gets or sets the blob's hole count.</summary>
        [JsonProperty(PropertyName = "holes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Holes { get; set; }

        /// <summary>Gets or sets the blob's perimeter.</summary>
        [JsonProperty(PropertyName = "perimeter", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Perimeter { get; set; }

        /// <summary>Gets or sets the blob's points.</summary>
        [JsonProperty(PropertyName = "points", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double[] Points { get; set; }

        /// <summary>Gets or sets the blob's spread.</summary>
        [JsonProperty(PropertyName = "spread", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Spread { get; set; }
    }
}