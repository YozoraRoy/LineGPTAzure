//using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.Functions
{
    public class GoogleSearchFormat
    {
        public string Name { get; set; }

        public string type { get; set; }

        public SearchParameters Parameters { get; set; }
        public string Description { get; set; }
    }
}
