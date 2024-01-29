using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.Functions
{
    public class SearchParameters
    {
        
        public string Type { get; set; }
        public SearchProperties Properties { get; set; }
        public List<string> Required { get; set; }
    }
}
