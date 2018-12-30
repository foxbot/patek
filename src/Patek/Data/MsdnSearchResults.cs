using System.Collections.Generic;
using LiteDB;
using Newtonsoft.Json;

namespace Patek.Data
{
    [JsonObject]
    public class MsdnSearchResults
    {
        // used in caching results
        [BsonId]
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public string SearchTerm { get; set; }
        [JsonProperty("results")]
        public MsdnSearchResult[] Results { get; set; }
    }
}