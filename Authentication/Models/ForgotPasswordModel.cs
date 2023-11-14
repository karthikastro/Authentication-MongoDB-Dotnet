using System.ComponentModel.DataAnnotations;

namespace Authentication.Models
{
    public class ForgotPasswordModel
    {
        [EmailAddress]
        public string email { get; set; }
    }
}
