// Copyright (c) 2022 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// A class that holds the HMI settings that define what will be displayed on the default Web HMI page.
    /// </summary>
    [CvsSerializable(JsonName = "HmiSettings")]
    public class HmiSettings
    {
        public HmiSettings()
        {
            AllowAdjustImage = true;
            AllowFilmstrip = true;
            AllowFilmstripSaveImage = true;
            AllowFocus = true;
            AllowJobLoad = true;
            AllowJobSave = true;
            AllowLocalStorage = true;
            AllowProcessedImages = true;
            AllowSoftOnline = true;
            AllowSwitchView = true;
            AllowTrigger = true;
            DefaultColorScheme = "";
            ImageResolution = 1;
            InactivityTimeout = 0;
            StatusStyle = 0;
        }

        /// <summary>The Type of the object.</summary>
        [JsonProperty(PropertyName = "$type")]
        public string Type { get; set; }
        /// <summary>A Boolean that designates whether the operator should be able to adjust the image (e.g. pan/zoom).</summary>
        [JsonProperty(PropertyName = "allowAdjustImage")]
        public bool AllowAdjustImage { get; set; }
        /// <summary>A Boolean that designates the filmstrip may be shown if the result queue is enabled.</summary>
        [JsonProperty(PropertyName = "allowFilmstrip")]
        public bool AllowFilmstrip { get; set; }
        /// <summary>A Boolean that designates that an operator should be able to save an image if there are sufficient permissions.</summary>
        [JsonProperty(PropertyName = "allowFilmstripSaveImage")]
        public bool AllowFilmstripSaveImage { get; set; }
        /// <summary>A Boolean that designates that the Focus button should be shown.</summary>
        [JsonProperty(PropertyName = "allowFocus")]
        public bool AllowFocus { get; set; }
        /// <summary>A Boolean that designates that the Load Job button should be shown.</summary>
        [JsonProperty(PropertyName = "allowJobLoad")]
        public bool AllowJobLoad { get; set; }
        /// <summary>A Boolean that designates that the Save Job button should be shown.</summary>
        [JsonProperty(PropertyName = "allowJobSave")]
        public bool AllowJobSave { get; set; }
        /// <summary>A Boolean that designates whether adjustments at the terminal may be saved in the browser’s local storage.</summary>
        [JsonProperty(PropertyName = "allowLocalStorage")]
        public bool AllowLocalStorage { get; set; }
        /// <summary>A Boolean that designates that processed images should be shown.</summary>
        [JsonProperty(PropertyName = "allowProcessedImages")]
        public bool AllowProcessedImages { get; set; }
        /// <summary>A Boolean that designates whether the Online/Offline button should be shown.</summary>
        [JsonProperty(PropertyName = "allowSoftOnline")]
        public bool AllowSoftOnline { get; set; }
        /// <summary>A Boolean that designates whether the Switch View button should be shown.</summary>
        [JsonProperty(PropertyName = "allowSwitchView")]
        public bool AllowSwitchView { get; set; }
        /// <summary>A Boolean that designates whether the Trigger button should be shown.</summary>
        [JsonProperty(PropertyName = "allowTrigger")]
        public bool AllowTrigger { get; set; }
        /// <summary>A string that describes the color scheme or theme that should be used on the HMI.</summary>
        [JsonProperty(PropertyName = "defaultColorScheme")]
        public string DefaultColorScheme { get; set; }
        /// <summary>A number that designates the resolution to retrieve the image at. 1=FULL, 2=HALF, 3=QUARTER, 4=EIGHTH</summary>
        [JsonProperty(PropertyName = "imageResolution")]
        public int ImageResolution { get; set; }
        /// <summary>The timeout in seconds after which the logged in user on the HMI should be logged out when not active.</summary>
        [JsonProperty(PropertyName = "inactivityTimeout")]
        public int InactivityTimeout { get; set; }
        /// <summary>
        /// An integer 0 to 2, representing the 3 currently well-defined filmstrip status icon styles
        /// used in the default Web HMI page and VisionView (Geometric, OK/NG, and Check/X).
        /// The default value is zero, representing the standard pass/fail icons.
        /// </summary>
        [JsonProperty(PropertyName = "statusStyle")]
        public int StatusStyle { get; set; }
    }
}