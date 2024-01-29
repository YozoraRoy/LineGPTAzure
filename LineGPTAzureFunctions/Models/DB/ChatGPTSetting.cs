using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.DB
{
    public class ChatGPTSetting
    {
        public int Id { get; set; }
        public string DeploymentName { get; set; }

        public List<SelectListItem> BotNameViewModel { get; set; }

        public string BotName { get; set; }

        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public int MaxResponseLength { get; set; }
        public double Temperature { get; set; }
        public string TopProbabilities { get; set; }
        public string StopSequences { get; set; }
        public int PastMessagesToInclude { get; set; }
        public double FrequencyPenalty { get; set; }
        public double PresencePenalty { get; set; }
        public string SystemPrompt { get; set; }
        public string Editor { get; set; }
        public DateTime UpdateTime { get; set; }
    }

     
}
