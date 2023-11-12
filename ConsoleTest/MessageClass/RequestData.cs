using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleTest.Program;

namespace ConsoleTest.MessageClass
{
    public class RequestData
    {
        public RequestData(string model, List<ChatGPTMsg> messages, double temperature, int max_tokens, double top_p, int frequency_penalty, int presence_penalty, string stop)
        {
            this.model = model;
            this.messages = messages;
            this.temperature = temperature;
            this.max_tokens = max_tokens;
            this.top_p = top_p;
            this.frequency_penalty = frequency_penalty;
            this.presence_penalty = presence_penalty;
            this.stop = stop;
        }

        public string? model { get; set; }
        public List<ChatGPTMsg> messages { get; set; }
        public double temperature { get; set; }
        public int max_tokens { get; set; }
        public double top_p { get; set; }
        public int frequency_penalty { get; set; }
        public int presence_penalty { get; set; }
        public string? stop { get; set; }
    }
}
