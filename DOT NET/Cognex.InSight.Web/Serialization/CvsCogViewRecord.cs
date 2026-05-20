// Copyright (c) 2015-2021 Cognex Corporation. All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    [CvsSerializable(JsonName = "ViewRecord")]
    public class CvsCogViewRecord
    {
        /// <summary>Gets the title.</summary>
        [JsonProperty(PropertyName = "title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title { get; internal set; }

        /// <summary>Gets the layers.</summary>
        [JsonProperty(PropertyName = "layers", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken[] Layers { get; set; }

        /// <summary>Gets the viewport.</summary>
        [JsonProperty(PropertyName = "viewport", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CvsCogViewPort Viewport { get; internal set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [CvsSerializable(JsonName = "Viewport")]
    public class CvsCogViewPort
    {
        /// <summary>Gets the width.</summary>
        [JsonProperty(PropertyName = "width", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Width { get; set; }

        /// <summary>Gets the height.</summary>
        [JsonProperty(PropertyName = "height", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Height { get; set; }
    }
}