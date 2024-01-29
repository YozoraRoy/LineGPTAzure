using System;

namespace LineFunctionApp.Models.DB
{

    public class PlatformAdmin
    {
        // [Key] 自動產生
        // public long Id { get; set; }

        //[StringLength(255)]
        public string Email { get; set; }

        //[StringLength(50)]
        public string UserName { get; set; }

        //[StringLength(500)]
        public string Token { get; set; }

        public DateTime? LoginTime { get; set; }
    }
}
