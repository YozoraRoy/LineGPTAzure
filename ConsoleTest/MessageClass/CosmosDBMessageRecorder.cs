using Newtonsoft.Json;

namespace CosmosGettingStartedTutorial
{
    public class CosmosDBMessageRecorder
    {
        [JsonProperty(PropertyName = "LineConversionId")]
        public string LineConversionId { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        // public string PartitionKey { get; set; }
        public string UserName { get; set; }
        public List<string> MessageList { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
