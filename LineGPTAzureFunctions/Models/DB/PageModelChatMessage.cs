using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.DB
{
    public class PageModelChatMessage
    {
        public int Id { get; set; }
        public string ChatUserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Response { get; set; }
        public DateTime DataTime { get; set; }
        public string BotName { get; set; }
        public string MessageTag { get; set; }
        public bool IsCheck { get; set; }
        public string ChatToolVersion { get; set; }
    }
}
