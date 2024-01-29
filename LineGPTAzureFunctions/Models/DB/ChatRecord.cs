using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.DB
{
    public class ChatRecord
    {
        // public long Id { get; set; } 自動編號

        // not null
        public string ChatUserId { get; set; }
        // not null
        public string Role { get; set; }
        public string Response { get; set; }
        // not null
        public DateTime DataTime { get; set; }
        public string BotName { get; set; }

        public string MessageTag { get; set; }
        public int? IsCheck { get; set; }
        public string Editor { get; set; }
        public string IdealAnswer { get; set; }
        // not null
        /// <summary>
        /// Not null & 0 = DEV 1 = UAT 2 = PRD
        /// </summary>
        public int ChatToolVersion { get; set; }

        /// <summary>
        /// Open AI Assistant 專用ID
        /// </summary>
        public string ThreadId { get; set; }

        public DateTime? UpdatedDateTime { get; set; }
    }
}
