// Copyright (c) 2015-2021 Cognex Corporation. All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates an In-Sight image layer.
    /// </summary>
    [CvsSerializable(JsonName = "ImageLayer")]
    public class CvsCogImageLayer
    {
        /// <summary>Gets or sets the result url.</summary>
        [JsonProperty(PropertyName = "url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url { get; set; }

        /// <summary>Gets or sets the height.</summary>
        [JsonProperty(PropertyName = "height", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Height { get; set; }

        /// <summary>Gets or sets the image object associated with this image layer.</summary>
        [JsonProperty(PropertyName = "image", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CvsCogImage Image { get; set; }

        /// <summary>
        /// Gets or sets the static transform.
        /// </summary>
        /// <remarks>
        /// Statically sized images do not support the Crop/Scale architecture on the camera. The camera only 
        /// provides a (typically very small) image file that represents the entirety of the image. The client 
        /// is responsible for transforming it into actual camera co-ordinates to fit its display.
        /// 
        /// This transform maps the image's co-ordinate system onto the ViewPort's co-ordinate system. The 
        /// static transform may involve scale, rotation, and offset.
        /// </remarks>
        [JsonProperty(PropertyName = "staticTransform", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CvsCogLinearTransform StaticTransform { get; set; }

        /// <summary>
        /// Gets or sets the transform.
        /// </summary>
        /// <remarks>
        /// This transform is for processed images that support the Crop/Scale architecture on the camera. It
        /// maps the image's co-ordinate system onto the ViewPort's co-ordinate system. The transform may only
        /// involve rotation and offset, not scale.
        /// </remarks>
        [JsonProperty(PropertyName = "transform", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CvsCogLinearTransform Transform { get; set; }

        /// <summary>Gets or sets the width.</summary>
        [JsonProperty(PropertyName = "width", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Width { get; set; }
    }
}