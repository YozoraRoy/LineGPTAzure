using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.Functions
{
    public class MedicalInstitutionPropertiesResult
    {
        public string District { get; set; }
        public string Hospital { get; set; }
        public string State { get; set; }
        public string Director { get; set; }
        public string Title { get; set; }
        public string HighestDegree { get; set; }
        public string Specialty { get; set; }
        public string Services { get; set; }
        public double GoogleRating { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }


        public void TrimToFirstChineseCharacter()
        {
            District = GetFirstChineseCharacter(District);
            Hospital = GetFirstChineseCharacter(Hospital);
            State = GetFirstChineseCharacter(State);
            Director = GetFirstChineseCharacter(Director);
            Title = GetFirstChineseCharacter(Title);
            HighestDegree = GetFirstChineseCharacter(HighestDegree);
            Specialty = GetFirstChineseCharacter(Specialty);
            Services = GetFirstChineseCharacter(Services);
            Website = GetFirstChineseCharacter(Website);
            Address = GetFirstChineseCharacter(Address);
        }

        private string GetFirstChineseCharacter(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // 使用正規表達式擷取中文字符
            var regex = new Regex(@"[\u4e00-\u9fa5]");
            var match = regex.Match(input);

            if (match.Success)
            {
                return match.Value;
            }

            return input;
        }
    }

  

}
