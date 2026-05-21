// Copyright (c) 2017-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    [CvsSerializable(JsonName = "BlobChain")]
    public class CvsCogBlobChain : CvsCogShape
    {
        [JsonProperty(PropertyName = "index", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "points", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double[] Points { get; set; }

        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }
    }
}