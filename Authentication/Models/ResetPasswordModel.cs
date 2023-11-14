using System.ComponentModel.DataAnnotations;

namespace Authentication.Models
{
    public class ResetPasswordModel
    {
        [Required, EmailAddress]
        public string email { get; set; }
        [Required]
        public string token { get; set; }
        [Required]
        public string password { get; set; }
       
    }
}
