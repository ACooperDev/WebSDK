// Copyright (c) 2017-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight sub-region in a 
    /// composite region.
    /// </summary>
    [CvsSerializable(JsonName = "SubRegion")]
    public class CvsCogSubRegion : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogSubRegion</c> class.</summary>
        public CvsCogSubRegion()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogSubRegion</c> class.</summary>
        public CvsCogSubRegion(bool add, CvsCogShape shape)
        {
            Add = add;
            Shape = shape;
        }

        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "SubRegion"; } }

        /// <summary>Gets or sets whether the sub-region adds or subtracts from the final space.</summary>
        [JsonProperty(PropertyName = "add", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Add { get; set; }

        /// <summary>The shape to be composited with others to form the region.</summary>
        [JsonProperty(PropertyName = "shape", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CvsCogShape Shape { get; set; }
    }
}