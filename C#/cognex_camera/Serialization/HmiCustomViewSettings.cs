// Copyright (c) 2022 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// A class that holds the settings that define the HMI custom view for a job.
    /// </summary>
    [CvsSerializable(JsonName = "HmiCustomViewSettings")]
    public class HmiCustomViewSettings
    {
        public const int DefaultWidth = 64;
        public const int DefaultHeight = 20;

        public HmiCustomViewSettings()
        {
            Type = "HmiCustomViewSettings";
            // Range of cells
            Top = 0;
            Bottom = 599;
            Left = 0;
            Right = 25;
            // Pixel size and location for the view
            X = 0;
            Y = 0;
            Width = 320;
            Height = 240;
            // Additional flags
            ShowGraphics = true;
            ShowImage = true;
            ShowOverlay = true;
        }

        /// <summary>The Type of the object.</summary>
        [JsonProperty(PropertyName = "$type")]
        public string Type { get; set; }
        /// <summary>Top row index of the custom view. (0-599)</summary>
        [JsonProperty(PropertyName = "top")]
        public int Top { get; set; }
        /// <summary>Bottom row index of the custom view. (0-599)</summary>
        [JsonProperty(PropertyName = "bottom")]
        public int Bottom { get; set; }
        /// <summary>Left column index of the custom view. (0-25)</summary>
        [JsonProperty(PropertyName = "left")]
        public int Left { get; set; }
        /// <summary>Right column index of the custom view. (0-25)</summary>
        [JsonProperty(PropertyName = "right")]
        public int Right { get; set; }
        /// <summary>X coordinate where the custom view should be displayed by default. This is in the pixel space of the full resolution image of the sensor.</summary>
        [JsonProperty(PropertyName = "x")]
        public int X { get; set; }
        /// <summary>Y coordinate where the custom view should be displayed by default. This is in the pixel space of the full resolution image of the sensor.</summary>
        [JsonProperty(PropertyName = "y")]
        public int Y { get; set; }
        /// <summary>The width of the custom view in the pixel space of the full resolution image of the sensor.</summary>
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }
        /// <summary>The height of the custom view in the pixel space of the full resolution image of the sensor.</summary>
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }
        /// <summary>A flag that designates whether the graphics should be displayed.</summary>
        [JsonProperty(PropertyName = "showGraphics")]
        public bool ShowGraphics { get; set; }
        /// <summary>A flag that designates whether the image should be displayed.</summary>
        [JsonProperty(PropertyName = "showImage")]
        public bool ShowImage { get; set; }
        /// <summary>A flag that designates whether the overlay should be displayed. This should be set to true.</summary>
        [JsonProperty(PropertyName = "showOverlay")]
        public bool ShowOverlay { get; set; }
        /// <summary>Name of the custom view.</summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        // Convert the cell range fields to a formatted range string.
        public string ToCellRange()
        {
            string cellRange = "";

            if ((Left >= 0) && (Left <= 25) && (Right >= 0) && (Right <= 25))
            {
                cellRange = string.Format("{0}{1}:{2}{3}", (char)('A' + Left), Top, (char)('A' + Right), Bottom);
            }

            return cellRange;
        }

        /// <summary>
        /// Convert the designated cell range into the range fields.
        /// </summary>
        /// <param name="cellRange"></param>
        public void FromCellRange(string cellRange)
        {
            string[] locations = cellRange.Split(new char[1] { ':' });
            if (locations.Length == 2)
            {
                int startRow, startCol;
                int endRow, endCol;
                if (HmiCellResult.LocationParse(locations[0], out startRow, out startCol) &&
                    HmiCellResult.LocationParse(locations[1], out endRow, out endCol))
                {
                    Left = startCol;
                    Right = endCol;
                    Top = startRow;
                    Bottom = endRow;
                }
            }
        }
    }
}

