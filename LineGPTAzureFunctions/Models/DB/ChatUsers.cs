using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.DB
{
    public class ChatUsers
    {
        // auto created 
        public long Id { get; set; } 
        // not null
        public string ChatUserId { get; set; }        
        // not null
        public string UserName { get; set; }
        // not null
        public DateTime DataTime { get; set; }
        public string Tag { get; set; }
        public string Remark { get; set; }
        public string Location { get; set; }

        /// <summary>
        /// Not null & default is "Line"
        /// </summary>
        public string ChatTool { get; set; }

        public string Editor { get; set; }
        public DateTime UpdatedDateTime { get; set; }
    }
}