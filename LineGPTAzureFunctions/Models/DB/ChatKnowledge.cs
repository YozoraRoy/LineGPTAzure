using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.DB
{
    public class ChatKnowledge
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Tag { get; set; }
        public string Author { get; set; }
        public string Reviewer { get; set; }
        public string Location { get; set; }
        public string Extensions { get; set; }
        public string Description { get; set; }
        public string CognitiveIndexName { get; set; }
        public bool IsUpload { get; set; }
        public bool IsDelete { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Editor { get; set; }

    }
}
