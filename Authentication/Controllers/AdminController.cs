using Amazon.Runtime.Internal;
using Authentication.Dtos;
using Authentication.Models;
using Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using ZstdSharp.Unsafe;

namespace Authentication.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public AdminController(IUserServices userServices,UserManager<ApplicationUser> userManager,RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userServices = userServices;
        }
        [HttpGet]
        public  ActionResult GetAll()
        {
            var result =  _userServices.GetUsers();
            
            return Ok(result);
        }
        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            var result = _userServices.GetUserById(id);
            if(result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<RegisterResponse> CreateUser(AdminCreateUserDTOcs adminCreateUserDTOcs)
        {
            var result = await AdminRegisterAsync(adminCreateUserDTOcs);
            return result;
            
        }

        private async Task<RegisterResponse> AdminRegisterAsync(AdminCreateUserDTOcs request)
        {
            try
            {

                var userExists = await _userManager.FindByEmailAsync(request.email);
                if (userExists != null) return new RegisterResponse { Message = "User already exists", Success = false };
                userExists = new ApplicationUser
                {

                    FullName = request.userName,
                    Email = request.email,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    UserName = request.email,
                    PhoneNumber= request.mobileNo

                };
                var createUserResult = await _userManager.CreateAsync(userExists, request.password);
                if (!createUserResult.Succeeded) return new RegisterResponse { Message = $"Create user failed {createUserResult?.Errors?.First()?.Description}", Success = false };
                //user is created...
                //then add user to a role...
                var addUserToRoleResult = await _userManager.AddToRoleAsync(userExists,request.roles.ToString());
                if (!addUserToRoleResult.Succeeded) return new RegisterResponse { Message = $"Create user succeeded but could not add user to role {addUserToRoleResult?.Errors?.First()?.Description}", Success = false };

                //all is still well..
                return new RegisterResponse
                {
                    Success = true,
                    Message = $"User registered successfully; User ID : {userExists.Id}"
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponse { Message = ex.Message, Success = false };
            }
        }
        [HttpPatch]
        public async Task<RegisterResponse> Update(string id,AdminCreateUserDTOcs request)
        {
            var existuser = _userServices.GetUserById(id);
            if(existuser == null)
            {
                var message = new RegisterResponse { Message ="Id not found",Success = false};
                return message;
            }
            existuser.UserName = request.userName;
            existuser.Email = request.email;
            existuser.PhoneNumber = request.mobileNo;
            existuser.FullName = request.userName;
            
            var updateresult = await _userManager.UpdateAsync(existuser);
         
            var returnmessage = new RegisterResponse { Message = $"user updated;\n {existuser}", Success = true };
            return returnmessage;
        }
        [HttpPatch("password/{id}")]
        public async Task<RegisterResponse> ChangePassword(string id, string oldPassword,string newPassword)
        {
            var existuser = _userServices.GetUserById(id);
            if (existuser == null)
            {
                var message = new RegisterResponse { Message = "Id not found", Success = false };
                return message;
            }
            var changepasswordresult = _userManager.ChangePasswordAsync(existuser, oldPassword, newPassword);
            var returnmessage = new RegisterResponse { Message = $"Password updated;\n {existuser.UserName}", Success = true };
            return returnmessage;
        }
        [HttpDelete]
        public ActionResult Delete(string id)
        {
            var checkuser = _userServices.GetUserById(id);
            if(checkuser == null)
            {
                return NotFound();
            }
            _userManager.DeleteAsync(checkuser);
            return Ok();
        }
    }
}
