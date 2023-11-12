using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.MessageClass
{
    public class UserProfile
    {
        public string displayName { get; set; }
        public string userId { get; set; }
        public string pictureUrl { get; set; }
        public string statusMessage { get; set; }
    }
}
