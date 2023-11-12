using Newtonsoft.Json;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;

namespace CosmosGettingStartedTutorial
{
    public class CosmosDBUserSetting
    {
        // Line User Id
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }
        public string? userName { get; set; }
        public string? dataTime { get; set; }
        public string? gptPromptsSetting { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
