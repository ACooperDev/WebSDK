// Copyright (c) 2022 Cognex Corporation. All Rights Reserved

using System;
using System.Linq;
using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>Base class for those encapsulating an InSight cell.</summary>
    [CvsSerializable(JsonName = "HmiCellResult")]
    public class HmiCellResult : ICloneable
    {
        public HmiCellResult()
        {
        }

        /// <summary>
        /// Copies the cell result.
        /// </summary>
        /// <returns>A new instance of the cell result.</returns>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        [JsonProperty(PropertyName = "location", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "data", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Value { get; set; }

        [JsonProperty(PropertyName = "disabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Disabled { get; set; }

        [JsonProperty(PropertyName = "error", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Error { get; set; }

        [JsonProperty(PropertyName = "editable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Editable { get; set; }

        public static bool LocationParse(string cellLocation, out int rowIndex, out int colIndex)
        {
            colIndex = -1;
            rowIndex = -1;
            if (cellLocation.Length <= 0)
                return false;

            char firstChar = cellLocation.ElementAt<char>(0);
            colIndex = (int)firstChar - (int)'A';
            string strRow = cellLocation.Substring(1);
            if (!int.TryParse(strRow, out rowIndex))
                return false;

            return true;
        }
    }

    /// <summary>InSight float cell.</summary>
    [CvsSerializable(JsonName = "HmiFloatResult")]
    public class HmiFloatResult : HmiCellResult
    {
        public HmiFloatResult()
        {
        }
    }

    /// <summary>InSight Float cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "FloatResult")]
    public class FloatResult : HmiFloatResult
    {
    }

    /// <summary>InSight EditFloat cell.</summary>
    [CvsSerializable(JsonName = "HmiEditFloatResult")]
    public class HmiEditFloatResult : HmiCellResult
    {
        public HmiEditFloatResult()
        {
        }

        [JsonProperty(PropertyName = "min", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float Min { get; set; }

        [JsonProperty(PropertyName = "max", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float Max { get; set; }
    }

    /// <summary>InSight EditFloat cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditFloatResult")]
    public class EditFloatResult : HmiEditFloatResult
    {
    }

    /// <summary>InSight EditInt cell.</summary>
    [CvsSerializable(JsonName = "HmiEditIntResult")]
    public class HmiEditIntResult : HmiCellResult
    {
        public HmiEditIntResult()
        {
        }

        [JsonProperty(PropertyName = "min", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Min { get; set; }

        [JsonProperty(PropertyName = "max", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Max { get; set; }
    }

    /// <summary>InSight EditInt cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditIntResult")]
    public class EditIntResult : HmiEditIntResult
    {
    }

    /// <summary>Base class for those encapsulating an InSight string cell.</summary>
    [CvsSerializable(JsonName = "HmiStringResult")]
    public class HmiStringResult : HmiCellResult
    {
        public HmiStringResult()
        {
        }
    }

    /// <summary>InSight string cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "StringResult")]
    public class StringResult : HmiStringResult
    {
    }

    /// <summary>InSight EditString cell.</summary>
    [CvsSerializable(JsonName = "HmiEditStringResult")]
    public class HmiEditStringResult : HmiCellResult
    {
        public HmiEditStringResult()
        {
        }

        [JsonProperty(PropertyName = "maxLength", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int MaxLength { get; set; }

        [JsonProperty(PropertyName = "maskInput", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool MaskInput { get; set; }
    }

    /// <summary>InSight EditString cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditStringResult")]
    public class EditStringResult : HmiEditIntResult
    {
    }

    /// <summary>InSight Button cell.</summary>
    /// <remarks>With the API 3.0, any edit cell that is rendered as a Button will be returned as this type.</remarks>
    [CvsSerializable(JsonName = "HmiButtonResult")]
    public class HmiButtonResult : HmiCellResult
    {
        public HmiButtonResult()
        {
        }

        [JsonProperty(PropertyName = "caption", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Caption { get; set; }
    }

    /// <summary>InSight Button cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "ButtonResult")]
    public class ButtonResult : HmiButtonResult
    {
    }

    /// <summary>InSight EditRegion cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditRegionResult")]
    public class EditRegionResult : ButtonResult
    {
    }

    /// <summary>InSight EditCircle cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditCircleResult")]
    public class EditCircleResult : ButtonResult
    {
    }

    /// <summary>InSight EditLine cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditLineResult")]
    public class EditLineResult : ButtonResult
    {
    }

    /// <summary>InSight EditPoint cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditPointResult")]
    public class EditPointResult : ButtonResult
    {
    }

    /// <summary>InSight EditAnnulus cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditAnnulusResult")]
    public class EditAnnulusResult : ButtonResult
    {
    }

    /// <summary>InSight EditPolygon cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditPolygonResult")]
    public class EditPolygonResult : ButtonResult
    {
    }

    /// <summary>InSight EditPolylinePath cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditPolylinePathResult")]
    public class EditPolylinePathResult : ButtonResult
    {
    }

    /// <summary>InSight EditCompositeRegion cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditCompositeRegionResult")]
    public class EditCompositeRegionResult : ButtonResult
    {
    }

    /// <summary>InSight EditMaskedRegion cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "EditMaskedRegionResult")]
    public class EditMaskedRegionResult : ButtonResult
    {
    }

    /// <summary>InSight CheckBox cell.</summary>
    [CvsSerializable(JsonName = "HmiCheckBoxResult")]
    public class HmiCheckBoxResult : HmiCellResult
    {
        public HmiCheckBoxResult()
        {
        }

        [JsonProperty(PropertyName = "caption", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Caption { get; set; }
    }

    /// <summary>InSight CheckBox cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "CheckBoxResult")]
    public class CheckBoxResult : HmiCheckBoxResult
    {
    }

    /// <summary>InSight ListBox cell.</summary>
    [CvsSerializable(JsonName = "HmiListBoxResult")]
    public class HmiListBoxResult : HmiCellResult
    {
        public HmiListBoxResult()
        {
        }

        [JsonProperty(PropertyName = "options", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Options { get; set; }
    }

    /// <summary>InSight CheckBox cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "ListBoxResult")]
    public class ListBoxResult : HmiListBoxResult
    {
    }

    /// <summary>InSight Status cell (post 3.0).</summary>
    [CvsSerializable(JsonName = "HmiStatusResult")]
    public class HmiStatusResult : HmiCellResult
    {
        public HmiStatusResult()
        {
        }

        [JsonProperty(PropertyName = "caption", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Caption { get; set; }


        [JsonProperty(PropertyName = "color", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Color { get; set; }
    }

    /// <summary>InSight Status cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "StatusResult")]
    public class StatusResult : HmiCellResult
    {
        public StatusResult()
        {
        }

        [JsonProperty(PropertyName = "caption", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Caption { get; set; }
    }

    /// <summary>InSight MultiStatus cell.</summary>
    [CvsSerializable(JsonName = "HmiMultiStatusResult")]
    public class HmiMultiStatusResult : HmiCellResult
    {
        public HmiMultiStatusResult()
        {
        }

        [JsonProperty(PropertyName = "color0", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Color0 { get; set; }

        [JsonProperty(PropertyName = "color1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Color1 { get; set; }

        [JsonProperty(PropertyName = "numBits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumBits { get; set; }

        [JsonProperty(PropertyName = "reverse", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Reverse { get; set; }

        [JsonProperty(PropertyName = "startBit", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int StartBit { get; set; }
    }

    /// <summary>InSight MultiStatus cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "MultiStatusResult")]
    public class MultiStatusResult : HmiMultiStatusResult
    {
    }

    /// <summary>InSight StatusLight cell.</summary>
    [CvsSerializable(JsonName = "HmiStatusLightResult")]
    public class HmiStatusLightResult : HmiCellResult
    {
        public HmiStatusLightResult()
        {
        }

        [JsonProperty(PropertyName = "caption", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Caption { get; set; }

        [JsonProperty(PropertyName = "color", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Color { get; set; }
    }

    /// <summary>InSight StatusLight cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "StatusLightResult")]
    public class StatusLightResult : HmiStatusLightResult
    {
    }

    /// <summary>InSight ColorLabel cell.</summary>
    [CvsSerializable(JsonName = "HmiColorLabelResult")]
    public class HmiColorLabelResult : HmiCellResult
    {
        public HmiColorLabelResult()
        {
        }

        [JsonProperty(PropertyName = "foreColor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long ForeColor { get; set; }

        [JsonProperty(PropertyName = "backColor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long BackColor { get; set; }
    }

    /// <summary>InSight ColorLabel cell (Prior to 3.0).</summary>
    [CvsSerializable(JsonName = "ColorLabelResult")]
    public class ColorLabelResult : HmiColorLabelResult
    {
    }

}
