using System.Collections.Generic;
using Newtonsoft.Json;

namespace Patek.Data
{
    [JsonObject]
    public class SearchResults
    {
        [JsonProperty("results")]
        public IReadOnlyList<SearchResult> Results { get; set; }
    }
}