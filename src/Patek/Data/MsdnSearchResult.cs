using Newtonsoft.Json;

namespace Patek.Data
{
    [JsonObject]
    public class MsdnSearchResult
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("itemType")]
        public string ItemType { get; set; }
        [JsonProperty("itemKind")]
        public string ItemKind { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}