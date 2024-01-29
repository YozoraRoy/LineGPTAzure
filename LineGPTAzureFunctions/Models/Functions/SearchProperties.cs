using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.Functions
{
    public class SearchProperties
    {
        public SearchProperty Title { get; set; }

        public SearchProperty DisplayLink { get; set; }
        public SearchProperty Snippet { get; set; }
       
        public SearchProperty FormattedUrl { get; set; }
      
    }
}
