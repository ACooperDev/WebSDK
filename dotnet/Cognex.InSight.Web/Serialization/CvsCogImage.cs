// Copyright (c) 2015-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates metadata for an In-Sight acquisition.
    /// </summary>
    [CvsSerializable(JsonName = "AcquisitionInfo")]
    public class CvsCogAcqInfo
    {
        /// <summary>Gets or sets the duration.</summary>
        [JsonProperty(PropertyName = "acquisitionDuration", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Duration { get; internal set; }

        /// <summary>Gets or sets the timestamp.</summary>
        [JsonProperty(PropertyName = "acquisitionTimestamp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Timestamp { get; internal set; }
    }

    /// <summary>
    /// Encapsulates the data associated with an In-Sight image.
    /// </summary>
    [CvsSerializable(JsonName = "Image")]
    public class CvsCogImage
    {
        /// <summary>Gets or sets the acquisition info.</summary>
        [JsonProperty(PropertyName = "acquisitionInfo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CvsCogAcqInfo Info { get; set; }

        /// <summary>Gets or sets the bits per pixel.</summary>
        [JsonProperty(PropertyName = "bitsPerPixel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int BitsPerPixel { get; set; }

        /// <summary>Gets or sets the frozen.</summary>
        [JsonProperty(PropertyName = "frozen", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Frozen { get; set; }

        /// <summary>Gets or sets the height.</summary>
        [JsonProperty(PropertyName = "height", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Height { get; set; }

        /// <summary>Gets or sets the ID.</summary>
        [JsonProperty(PropertyName = "id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long ID { get; set; }

        /// <summary>Gets or sets the image format identifier integer.</summary>
        [JsonProperty(PropertyName = "imageFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ImageFormat { get; set; }

        /// <summary>Gets or sets a flag that indicates if this is a color image or not.</summary>
        [JsonProperty(PropertyName = "isColor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsColor { get; set; }

        /// <summary>Gets or sets the mask.</summary>
        [JsonProperty(PropertyName = "mask", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CvsCogRegion Mask { get; set; }

        /// <summary>Gets or sets the X Offset of the image.</summary>
        [JsonProperty(PropertyName = "offsetX", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int OffsetX { get; set; }

        /// <summary>Gets or sets the Y Offset of the image.</summary>
        [JsonProperty(PropertyName = "offsetY", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int OffsetY { get; set; }

        /// <summary>Gets or sets the orientation.</summary>
        [JsonProperty(PropertyName = "orientation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Orientation { get; set; }

        /// <summary>Gets or sets a flag indicating if this image is read only or not.</summary>
        [JsonProperty(PropertyName = "readOnly", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ReadOnly { get; set; }

        /// <summary>Gets or sets the image URL.</summary>
        [JsonProperty(PropertyName = "url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url { get; set; }

        /// <summary>Gets or sets the width.</summary>
        [JsonProperty(PropertyName = "width", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Width { get; set; }
    }
}