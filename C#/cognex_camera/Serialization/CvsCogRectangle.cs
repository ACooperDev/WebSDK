// Copyright (c) 2016-2021 Cognex Corporation. All Rights Reserved

using System;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// Encapsulates the data associated with an In-Sight rectangle graphic.
    /// </summary>
    [CvsSerializable(JsonName = "Rectangle")]
    public class CvsCogRectangle : CvsCogShape
    {
        /// <summary>Initializes a new instance of the <c>CvsCogRectangle</c> class.</summary>
        public CvsCogRectangle()
        {
        }

        /// <summary>Initializes a new instance of the <c>CvsCogRectangle</c> class.</summary>
        /// <param name="x">The X location of the graphic in pixels.</param>
        /// <param name="y">The Y location of the graphic in pixels.</param>
        /// <param name="w">The width of the graphic in pixels.</param>
        /// <param name="h">The height of the graphic in pixels.</param>
        public CvsCogRectangle(double x, double y, double w, double h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

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

        /// <summary>
        /// Converts to a Rectangle.
        /// </summary>
        public System.Drawing.Rectangle ToRectangle()
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)Math.Round(X, MidpointRounding.AwayFromZero),
                                                                         (int)Math.Round(Y, MidpointRounding.AwayFromZero),
                                                                         (int)Math.Round(Width, MidpointRounding.AwayFromZero),
                                                                         (int)Math.Round(Height, MidpointRounding.AwayFromZero));
            return rect;
        }

        /// <summary>
        /// Converts to a RectangleF.
        /// </summary>
        public System.Drawing.RectangleF ToRectangleF()
        {
            System.Drawing.RectangleF rect = new System.Drawing.RectangleF((float)X, (float)Y, (float)Width, (float)Height);
            return rect;
        }
    }
}