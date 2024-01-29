using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.Web
{
    public class HospitalSearchResult
    {
        [JsonProperty("items")]
        public List<HospitalSearchResultItem> Items { get; set; }
    }

     

}
