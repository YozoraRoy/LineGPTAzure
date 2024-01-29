using Newtonsoft.Json;
using System.Collections.Generic;

namespace LineFunctionApp.Models.Web
{
    public class GoogleSearchResult
    {
        [JsonProperty("items")]
        public List<GoogleSearchResultItem> Items { get; set; }
    }
}
