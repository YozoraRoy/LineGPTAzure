using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.Web
{
    public class HospitalSearchResultItem
    {
        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("hospital")]
        public string Hospital { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("director")]
        public string Director { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("highestDegree")]
        public string HighestDegree { get; set; }

        [JsonProperty("specialty")]
        public string Specialty { get; set; }

        [JsonProperty("services")]
        public string Services { get; set; }

        [JsonProperty("googleRating")]
        public double GoogleRating { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("address")]
        public string GoogleMapLink { get; set; }

    }
}
