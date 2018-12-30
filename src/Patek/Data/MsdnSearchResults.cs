using System.Collections.Generic;
using Newtonsoft.Json;

namespace Patek.Data
{
    [JsonObject]
    public class MsdnSearchResults
    {
        [JsonProperty("results")]
        public IReadOnlyList<MsdnSearchResult> Results { get; set; }
    }
}