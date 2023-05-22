using Newtonsoft.Json;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;

namespace CosmosGettingStartedTutorial
{
    public class CosmosDBMessageRecorder
    {
        // Line User Id
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        // public string? lineConversionId { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }
        public string? userName { get; set; }
        public string? finalDataTime { get; set; }
        // public List<string> messageList { get; set; }
        public List<ChatMessage> chatMessage { get; set; }        

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
