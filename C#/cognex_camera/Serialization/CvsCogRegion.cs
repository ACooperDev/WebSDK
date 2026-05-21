// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight region graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Region")]
    public class CvsCogRegion : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogRegion</c> class.</summary>
        public CvsCogRegion()
        {
        }

        /// <summary>
        /// Checks to see if the two objects are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as CvsCogRegion;
            return (other != null)
                && (Angle == other.Angle)
                && (CellLocation == other.CellLocation)
                && (Color == other.Color)
                && (Curve == other.Curve)
                && (Font == other.Font)
                && (FontSize == other.FontSize)
                && (GraphicId == other.GraphicId)
                && (Height == other.Height)
                && (Label == other.Label)
                && (LineThickness == other.LineThickness)
                && (Name == other.Name)
                && (ShowAxesLabels == other.ShowAxesLabels)
                && (ShowScanLine == other.ShowScanLine)
                && (ShowXArrow == other.ShowXArrow)
                && (ShowYArrow == other.ShowYArrow)
                && (Width == other.Width)
                && (X == other.X)
                && (Y == other.Y);
        }

        /// <summary>
        /// Gets a hash code for the object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ Angle.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, CellLocation) ? CellLocation.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ Color.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Curve.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, Font) ? Font.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ FontSize.GetHashCode();
                hash = (hash * HashingMultiplier) ^ GraphicId.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Height.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, Label) ? Label.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ LineThickness.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, Name) ? Name.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ ShowAxesLabels.GetHashCode();
                hash = (hash * HashingMultiplier) ^ ShowScanLine.GetHashCode();
                hash = (hash * HashingMultiplier) ^ ShowXArrow.GetHashCode();
                hash = (hash * HashingMultiplier) ^ ShowYArrow.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Width.GetHashCode();
                hash = (hash * HashingMultiplier) ^ X.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Y.GetHashCode();
                return hash;
            }
        }

        /// <summary>Initializes a new instance of the <c>CvsCogRegion</c> class.</summary>
        /// <param name="x">The X location of the graphic in pixels.</param>
        /// <param name="y">The Y location of the graphic in pixels.</param>
        /// <param name="w">The width of the graphic in pixels.</param>
        /// <param name="h">The height of the graphic in pixels.</param>
        /// <param name="a">The angle of the graphic in degrees.</param>
        /// <param name="c">The curve of the graphic in degrees.</param>
        /// <param name="showAxesLabels">true to show axis labels in the graphic</param>
        /// <param name="showScanLine">true to show the scan line (horizontal line through the y-axis centerline of the region) in the graphic</param>
        /// <param name="showXArrow">true to show the X axis arrow in the graphic</param>
        /// <param name="showYArrow">true to show the Y axis arrow in the graphic</param>
        /// <param name="color">32-bit integer ARGB representation of the color of the graphic.</param>
        /// <param name="cellLocation">Cell location of the tool that generated the graphic.</param>
        public CvsCogRegion(double x,
                            double y,
                            double w,
                            double h,
                            double a,
                            double c,
                            bool showAxesLabels = false,
                            bool showScanLine = false,
                            bool showXArrow = false,
                            bool showYArrow = false,
                            int color = DefaultColor,
                            string cellLocation = "")
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Angle = a;
            Curve = c;
            Color = color;
            ShowAxesLabels = showAxesLabels;
            ShowScanLine = showScanLine;
            ShowXArrow = showXArrow;
            ShowYArrow = showYArrow;
            CellLocation = cellLocation;
        }

        /// <summary>The type.</summary>
        [JsonProperty(PropertyName = "$type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get { return "Region"; } }

        /// <summary>Gets or sets the X location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double X { get; set; }

        /// <summary>Gets or sets the Y location of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Y { get; set; }

        /// <summary>Gets or sets the width of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "w", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Width { get; set; }

        /// <summary>Gets or sets the height of the graphic in pixels.</summary>
        [JsonProperty(PropertyName = "h", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Height { get; set; }

        /// <summary>Gets or sets the angle of the graphic in degrees.</summary>
        [JsonProperty(PropertyName = "angle", DefaultValueHandling = DefaultValueHandling.Ignore), DefaultValue(0.0)]
        public double Angle { get; set; }

        /// <summary>Gets or sets the curve of the graphic in degrees.</summary>
        [JsonProperty(PropertyName = "curve", DefaultValueHandling = DefaultValueHandling.Ignore), DefaultValue(0.0)]
        public double Curve { get; set; }

        [JsonProperty(PropertyName = "showAxesLabels", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(true)]
        public bool ShowAxesLabels { get; set; }

        [JsonProperty(PropertyName = "showScanLine", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(false)]
        public bool ShowScanLine { get; set; }

        [JsonProperty(PropertyName = "showXArrow", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(true)]
        public bool ShowXArrow { get; set; }

        [JsonProperty(PropertyName = "showYArrow", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(true)]
        public bool ShowYArrow { get; set; }
    }
}