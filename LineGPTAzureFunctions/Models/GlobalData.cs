using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models
{
    public class GlobalData
    {
        private static GlobalData _instance;

        public string LoginUserName { get; set; }

        public static GlobalData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalData();
                }
                return _instance;
            }
        }
    }
}
