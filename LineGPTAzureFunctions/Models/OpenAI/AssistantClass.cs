using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.Models.OpenAI
{

    public class ThreadResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long CreatedAt { get; set; }
        public object Metadata { get; set; }
    }

    public class ThreadRun
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long CreatedAt { get; set; }
        public string AssistantId { get; set; }
        public string ThreadId { get; set; }
        public string Status { get; set; }
        public long StartedAt { get; set; }
        public long? ExpiresAt { get; set; }
        public long? CancelledAt { get; set; }
        public long? FailedAt { get; set; }
        public long CompletedAt { get; set; }
        public string LastError { get; set; }
        public string Model { get; set; }
        public string Instructions { get; set; }
        public List<Tool> Tools { get; set; }
        public List<string> FileIds { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class Tool
    {
        public string Type { get; set; }
    }

}
