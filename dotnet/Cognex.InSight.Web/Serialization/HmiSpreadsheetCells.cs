// Copyright (c) 2022 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// A class that holds HMI defined spreadsheet cells.
    /// </summary>
    [CvsSerializable(JsonName = "HmiSpreadsheetCells")]
    public class HmiSpreadsheetCells
    {
        public HmiSpreadsheetCells()
        {
            Type = "HmiSpreadsheetCells";
        }

        /// <summary>The Type of the object.</summary>
        [JsonProperty(PropertyName = "$type")]
        public string Type { get; set; }

        /// <summary>The cells that are defined by the HMI.</summary>
        [JsonProperty(PropertyName = "cells")]
        public HmiSpreadsheetCell[] Cells { get; set; }

        /// <summary>
        /// Holds the path from which this object was loaded.
        /// </summary>
        public string FilePath { get; set; }
    }

    /// <summary>
    /// A class that holds a single HMI defined cell.
    /// </summary>
    [CvsSerializable(JsonName = "HmiSpreadsheetCell")]
    public class HmiSpreadsheetCell
    {
        public HmiSpreadsheetCell()
        {
            Location = "";
        }

        /// <summary>The Type of the object.</summary>
        [JsonProperty(PropertyName = "$type")]
        public string Type { get; set; }
        /// <summary>The location of the cell.</summary>
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }
    }

    /// <summary>Defines the In-Sight Dialog cell.</summary>
    [CvsSerializable(JsonName = "DialogCell")]
    public class HmiDialogCell : HmiSpreadsheetCell
    {
        public HmiDialogCell()
        {
            Type = "DialogCell";
        }

        /// <summary>The label to display on the cell.</summary>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>The title to display on the dialog.</summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>The range of cells for the dialog.</summary>
        [JsonProperty(PropertyName = "range")]
        public string Range { get; set; }
    }

    /// <summary>Defines the In-Sight Wizard cell.</summary>
    [CvsSerializable(JsonName = "WizardCell")]
    public class HmiWizardCell : HmiSpreadsheetCell
    {
        public HmiWizardCell()
        {
            Type = "WizardCell";
        }

        /// <summary>The label to display on the cell.</summary>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>The array of dialog locations for the wizard.</summary>
        [JsonProperty(PropertyName = "dialogs")]
        public string[] Dialogs { get; set; }
    }

    /// <summary>Defines the In-Sight Bitmap cell.</summary>
    [CvsSerializable(JsonName = "BitmapCell")]
    public class HmiBitmapCell : HmiSpreadsheetCell
    {
        public HmiBitmapCell()
        {
            Type = "BitmapCell";
        }

        /// <summary>The file name for the file that holds the bitmap.</summary>
        [JsonProperty(PropertyName = "file")]
        public string File { get; set; }
    }
}
