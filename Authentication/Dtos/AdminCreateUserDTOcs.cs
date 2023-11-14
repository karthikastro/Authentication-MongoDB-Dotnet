using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos
{
    public enum Roles
    {
        Admin,
        Applicant,
        Interviewer
    }
    public class AdminCreateUserDTOcs
    {
        public string name {  get; set; }
        public string mobileNo { get; set; }
        [DefaultValue("Enter Email or Phone number")]
        public string userName { get; set; }
        [EmailAddress]
        public string email { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public Roles roles { get; set; }
        [Required, DataType(DataType.Password)]
        public string password { get; set; }
        [Required, DataType(DataType.Password), Compare(nameof(password), ErrorMessage = "Passwords do not match")]
        public string retypePassword { get; set; }
            
    }
}
