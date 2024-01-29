using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.Web
{
    public class GoogleSearchResultItem
    {

        [JsonProperty("title")]
        public string Title { get; set; }

        //[JsonProperty("htmlTitle")]
        //public string HtmlTitle { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("displayLink")]
        public string DisplayLink { get; set; }

        [JsonProperty("snippet")]
        public string Snippet { get; set; }

        //[JsonProperty("htmlSnippet")]
        //public string HtmlSnippet { get; set; }

        [JsonProperty("formattedUrl")]
        public string FormattedUrl { get; set; }

        [JsonProperty("htmlFormattedUrl")]
        public string HtmlFormattedUrl { get; set; }
    }
}
