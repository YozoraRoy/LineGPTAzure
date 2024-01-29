using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.DB
{
    public class MedicalInstitution
    {
        public long Id { get; set; }
        public string? District { get; set; }
        public string? Hospital { get; set; }
        public string? State { get; set; }
        public string? Director { get; set; }
        public string? Title { get; set; }
        public string? HighestDegree { get; set; }
        public string? Specialty { get; set; }
        public string? Services { get; set; }
        public double? GoogleRating { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }

        public string? Author { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? UpdateTime { get; set; }

        public DateTime? CreatedTime { get; set; }

        public string? Editor { get; set; }

    }
}
