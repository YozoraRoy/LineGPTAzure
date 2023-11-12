using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.MessageClass
{
    public class LineTextReplyJson
    {
        public string replyToken { get; set; }
        public List<Message> messages { get; set; }
        public bool notificationDisabled { get; set; }
    }
}
