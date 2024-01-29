using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models
{
    public class UploadCognitiveIndex
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Author { get; set; }
        public string Location { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }
}
