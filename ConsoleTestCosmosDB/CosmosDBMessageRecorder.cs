using Newtonsoft.Json;
using System;

namespace CosmosGettingStartedTutorial
{
    public class CosmosDBMessageRecorder
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string? lineConversionId { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }
        public string? userName { get; set; }
        public string? finalDataTime { get; set; }
        public List<string>? messageList { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
