// Copyright (c) 2021 Cognex Corporation. All Rights Reserved

using Newtonsoft.Json;

namespace Cognex.InSight.Remoting.Serialization
{
    /// <summary>
    /// A class that holds information about what should be included in the
    /// HmiSession results.
    /// </summary>
    [CvsSerializable(JsonName = "HmiSessionInfo")]
    public class HmiSessionInfo
    {
        [JsonProperty(PropertyName = "sheetName")]
        public string SheetName { get; set; }
        [JsonProperty(PropertyName = "cellNames")]
        public string[] CellNames { get; set; }
        [JsonProperty(PropertyName = "enableQueuedResults")]
        public bool EnableQueuedResults { get; set; }
        [JsonProperty(PropertyName = "includeCustomView")]
        public bool IncludeCustomView { get; set; }
        [JsonProperty(PropertyName = "autoReady")]
        public bool AutoReady { get; set; }
    }
}
