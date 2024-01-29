using System.Collections.Generic;

namespace ShareLibrary.Models
{

    public class FewShotExample
    {
        public string ChatbotResponse { get; set; }
        public string UserInput { get; set; }
    }

    public class ChatParameters
    {
        public string DeploymentName { get; set; }
        public int MaxResponseLength { get; set; }
        public float Temperature { get; set; }
        public float TopProbabilities { get; set; }
        public object StopSequences { get; set; }
        public int PastMessagesToInclude { get; set; }
        public float FrequencyPenalty { get; set; }
        public float PresencePenalty { get; set; }
    }

    public class ChatSetupObject
    {
        public string SystemPrompt { get; set; }
        public List<FewShotExample> FewShotExamples { get; set; }
        public ChatParameters ChatParameters { get; set; }
    }
}
