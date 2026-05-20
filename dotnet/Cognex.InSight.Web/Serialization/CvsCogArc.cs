// Copyright (c) 2017-2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    [CvsSerializable(JsonName = "Arc")]
    public class CvsCogArc : CvsCogShape
    {
        public CvsCogArc(float centerX,
                         float centerY,
                         float angle0,
                         float angle1,
                         float radius,
                         bool rotateClockwise = false,
                         int color = DefaultColor,
                         string cellLocation = "")
        {
            X = centerX;
            Y = centerY;
            Angle0 = -angle0;
            Angle1 = -angle1;
            Radius = radius;
            RotateClockwise = rotateClockwise;
            Color = color;
            CellLocation = cellLocation;
        }

        [JsonProperty(PropertyName = "angle0", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float Angle0 { get; set; }

        [JsonProperty(PropertyName = "angle1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float Angle1 { get; set; }

        [JsonProperty(PropertyName = "radius", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float Radius { get; set; }

        [JsonProperty(PropertyName = "rotateClockwise", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool RotateClockwise { get; set; }

        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float X { get; set; }

        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float Y { get; set; }
    }
}