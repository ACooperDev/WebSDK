// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight composite region.
    /// </summary>
    [CvsSerializable(JsonName = "CompositeRegion")]
    public class CvsCogCompositeRegion : CvsCogShape
    {
        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "CompositeRegion"; } }

        /// <summary>gets or sets the sub-regions that comprise the composite region.</summary>
        [JsonProperty(PropertyName = "subregions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CvsCogSubRegion[] SubRegions { get; set; }
    }

    /// <summary>
    /// Encapsulates the data associated with an In-Sight masked region.
    /// </summary>
    [CvsSerializable(JsonName = "MaskedRegion")]
    public class CvsCogMaskedRegion : CvsCogCompositeRegion
    {
    }

    /// <summary>
    /// Encapsulates the data associated with an In-Sight polyline path.
    /// </summary>
    [CvsSerializable(JsonName = "PolylinePath")]
    public class CvsCogPolylinePath : CvsCogCompositeRegion
    {
    }
}