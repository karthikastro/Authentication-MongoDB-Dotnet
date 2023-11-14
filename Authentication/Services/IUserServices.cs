using Authentication.Dtos;
using Authentication.Models;


namespace Authentication.Services
{
    public interface IUserServices
    {
        public List<ApplicationUser> GetUsers();
        public ApplicationUser GetUserById(string id); 
        //public  Task<ApplicationUser> CreateUser(AdminCreateUserDTOcs user);
        public void  Delete(string id);

    }
}
