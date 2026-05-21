// Copyright (c) 2022 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Globalization;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// A class that holds the formatting for a cell.
    /// </summary>
    [CvsSerializable(JsonName = "HmiCellFormat")]
    public class HmiCellFormat
    {
        public HmiCellFormat()
        {
            Location = "";
            CellStyle = "";
            GraphicsStyle = "";
        }

        /// <summary>The Type of the object.</summary>
        [JsonProperty(PropertyName = "$type")]
        public string Type { get; set; }
        /// <summary>The location of the cell.</summary>
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }
        /// <summary>The style for the cell.</summary>
        [JsonProperty(PropertyName = "cellStyle")]
        public string CellStyle { get; set; }
        /// <summary>The style for the graphics for the cell.</summary>
        [JsonProperty(PropertyName = "graphicsStyle")]
        public string GraphicsStyle { get; set; }

        public Nullable<System.Drawing.Color> CellForeColor
        {
            get
            {
                if (CellStyle.Length == 0)
                    return null;

                int index = CellStyle.IndexOf("color:rgba(");
                if (index < 0)
                    return null;

                int left = CellStyle.IndexOf('(', index);
                int right = CellStyle.IndexOf(')', index);
                string noBrackets = CellStyle.Substring(left + 1, right - left - 1);

                string[] parts = noBrackets.Split(',');

                int r = int.Parse(parts[0], CultureInfo.InvariantCulture);
                int g = int.Parse(parts[1], CultureInfo.InvariantCulture);
                int b = int.Parse(parts[2], CultureInfo.InvariantCulture);

                return System.Drawing.Color.FromArgb(r, g, b);
            }
        }

        public Nullable<System.Drawing.Color> CellBackColor
        {
            get
            {
                if (CellStyle.Length == 0)
                    return null;

                int index = CellStyle.IndexOf("background-color:rgba(");
                if (index < 0)
                    return null;

                int left = CellStyle.IndexOf('(', index);
                int right = CellStyle.IndexOf(')', index);
                string noBrackets = CellStyle.Substring(left + 1, right - left - 1);

                string[] parts = noBrackets.Split(',');

                int r = int.Parse(parts[0], CultureInfo.InvariantCulture);
                int g = int.Parse(parts[1], CultureInfo.InvariantCulture);
                int b = int.Parse(parts[2], CultureInfo.InvariantCulture);

                return System.Drawing.Color.FromArgb(r, g, b);
            }
        }

        public System.Drawing.Font CellFont
        {
            get
            {
                if (CellStyle.Length == 0)
                    return null;

                int index = CellStyle.IndexOf("font:");
                if (index < 0)
                    return null;

                int left = CellStyle.IndexOf(':', index);
                int right = CellStyle.IndexOf(';', index);
                string fontStr = CellStyle.Substring(left + 1, right - left - 1);
                fontStr = fontStr.Trim(new char[1] { ' ' });

                string family = "Arial";
                float fntSize = 9.0f;
                FontStyle fs = FontStyle.Regular;

                if (fontStr.StartsWith("bold italic"))
                    fs = FontStyle.Bold | FontStyle.Italic;
                else if (fontStr.StartsWith("bold"))
                    fs = FontStyle.Bold;
                else if (fontStr.StartsWith("italic"))
                    fs = FontStyle.Italic;

                int firstQuote = fontStr.IndexOf("\"");
                if (firstQuote >= 0)
                {
                    int lastQuote = fontStr.IndexOf("\"", firstQuote + 1);
                    family = fontStr.Substring(firstQuote + 1, lastQuote - firstQuote - 1);

                    string numStr = fontStr.Substring(lastQuote + 2);
                    try
                    {
                        fntSize = (float)int.Parse(numStr.Split('p')[0]);
                    }
                    catch (Exception)
                    {
                    }
                }

                System.Drawing.Font fnt = new System.Drawing.Font(family, fntSize, fs);

                return fnt;
            }
        }

        public string TextAlign
        {
            get
            {
                if (CellStyle.Length == 0)
                    return null;

                int index = CellStyle.IndexOf("text-align:");
                if (index < 0)
                    return null;

                int left = CellStyle.IndexOf(':', index);
                int right = CellStyle.IndexOf(';', index);
                string textAlignStr = CellStyle.Substring(left + 1, right - left - 1);
                textAlignStr = textAlignStr.Trim(new char[1] { ' ' });
                return textAlignStr;
            }
        }

        public string VerticalAlign
        {
            get
            {
                if (CellStyle.Length == 0)
                    return null;

                int index = CellStyle.IndexOf("vertical-align:");
                if (index < 0)
                    return null;

                int left = CellStyle.IndexOf(':', index);
                int right = CellStyle.IndexOf(';', index);
                string vertAlignStr = CellStyle.Substring(left + 1, right - left - 1);
                vertAlignStr = vertAlignStr.Trim(new char[1] { ' ' });
                return vertAlignStr;
            }
        }
    }

    /// <summary>
    /// A class that holds the formatting for a sheet and it's cells.
    /// </summary>
    [CvsSerializable(JsonName = "HmiSheetFormat")]
    public class HmiSheetFormat
    {
        public HmiSheetFormat()
        {
            CellFormats = new HmiCellFormat[0];
            ColumnWidths = new int[0];
            RowHeights = new int[0];
        }

        /// <summary>An array of cell formats.</summary>
        [JsonProperty(PropertyName = "cellFormats")]
        public HmiCellFormat[] CellFormats { get; set; }
        /// <summary>An array of integers that represent the columns widths in pixels.</summary>
        [JsonProperty(PropertyName = "columnWidths")]
        public int[] ColumnWidths { get; set; }
        /// <summary>An array of integers that represent the row heights in pixels.</summary>
        [JsonProperty(PropertyName = "rowHeights")]
        public int[] RowHeights { get; set; }

    }
}
