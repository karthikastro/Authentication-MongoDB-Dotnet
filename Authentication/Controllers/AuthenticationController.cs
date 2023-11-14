using AspNetCore.Identity.MongoDbCore.Models;
using Authentication.Dtos;
using Authentication.Models;
using Authentication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Authorization;

namespace Authentication.Controllers
{
  
        [ApiController]
        [Route("api/v1/authenticate")]
        public class AuthenticationController : ControllerBase
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<ApplicationRole> _roleManager;
            private readonly IAuthenticateService _authenticateService;
        public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IAuthenticateService authenticateService)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _authenticateService = authenticateService;
            }

            [HttpPost]
            [Route("roles")]
            public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
            {
                var appRole = new ApplicationRole { Name = request.Role };
                var createRole = await _roleManager.CreateAsync(appRole);

                return Ok(new { message = "role created succesfully" });
            }

            [HttpPost]
            [Route("register")]
            public async Task<IActionResult> Register([FromBody] RegisterRequest request)
            {
                var result = await RegisterAsync(request);
                return result.Success ? Ok(result) : BadRequest(result.Message);
            }

            private async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
            {
                try
                {
                    
                    var userExists = await _userManager.FindByEmailAsync(request.Email);
                    if (userExists != null) return new RegisterResponse { Message = "User already exists", Success = false };
                    userExists = new ApplicationUser
                    {
                        
                        FullName = request.FullName,
                        Email = request.Email,
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        UserName = request.Email,

                    };
                    var createUserResult = await _userManager.CreateAsync(userExists, request.Password);
                    if (!createUserResult.Succeeded) return new RegisterResponse { Message = $"Create user failed {createUserResult?.Errors?.First()?.Description}", Success = false };
                    //user is created...
                    //then add user to a role...
                    var addUserToRoleResult = await _userManager.AddToRoleAsync(userExists, "Applicant");
                    if (!addUserToRoleResult.Succeeded) return new RegisterResponse { Message = $"Create user succeeded but could not add user to role {addUserToRoleResult?.Errors?.First()?.Description}", Success = false };

                    //all is still well..
                    return new RegisterResponse
                    {
                        Success = true,
                        Message = "User registered successfully"
                    };



                }
                catch (Exception ex)
                {
                    return new RegisterResponse { Message = ex.Message, Success = false };
                }
            }

            [HttpPost]
            [Route("login")]
            [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LoginResponse))]
            public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
            {
                var result = await LoginAsync(request, cancellationToken);

                return result.Success ? Ok(result) : BadRequest(result.Message);


            }

            private async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
            {
                try
                {

                    var user = await _userManager.FindByEmailAsync(request.Email);
                    if (user is null) return new LoginResponse { Message = "Invalid email/password", Success = false };
                    bool passswordcheck = await _userManager.CheckPasswordAsync(user, request.Password);
                    if (!passswordcheck)
                    {
                        return new LoginResponse { Success = false, Message = "Invalid password" };
                    }
                //all is well if ew reach here
                var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };
                    var roles = await _userManager.GetRolesAsync(user);
                    var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x));
                    claims.AddRange(roleClaims);

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("my_too_strong_access_secret_key"));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var expires = DateTime.Now.AddMinutes(30);

                    var token = new JwtSecurityToken(
                        issuer: "https://localhost:5001",
                        audience: "https://localhost:5001",
                        claims: claims,
                        expires: expires,
                        signingCredentials: creds

                        );

                var message = await _authenticateService.Authenticate(user, cancellationToken);
                return new LoginResponse { AccessToken=message.AccessToken,Message=message.RefreshToken,Success=true ,Email=user?.Email,UserId=user?.Id.ToString()};
                //return new LoginResponse
                //{
                //    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                //    Message = "Login Successful",
                //    Email = user?.Email,
                //    Success = true,
                //    UserId = user?.Id.ToString()
                //};
            }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new LoginResponse { Success = false, Message = ex.Message };
                }


            }
        [AllowAnonymous]
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPwd)
        {
            var user = await _userManager.FindByEmailAsync(forgotPwd.email);
            if (user == null) return NotFound($"user with {forgotPwd.email} not found!! enter correct email");
            var resettoken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var client = new SendGridClient("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");
            var fromEmail = new EmailAddress("kannans2141997@gmail.com","IceApple Technologies");
            var toEmails = new EmailAddress(user.Email);
            var subject = "Forgot Password Request in Italent -Hiring App  ";
            string C_Name = "IceApple Technologies";
            string C_mail = "careers@iceapple.tech";
            string C_num = "+91-9361954376";
            string htmlContent = string.Format("<p>Hi {0},</p><p>This email is regarding the  notification from the ITalent Hiring App based on your request for forgot Password; Please find the below token and paste it in reset password page</p><p><b>Token:</b></p><p>{1}<br></p><br><p>Do not share this token; The token valid only for 2 hours</p><p>If you do not opt for forgot password mail; please contact our support <br> </p><p><b>Contact :</b></p><p> {2}</p><p>{3}</p><p>{4}</p>", user.FullName, resettoken, C_Name, C_mail, C_num); ;
            var msg = MailHelper.CreateSingleEmail(fromEmail, toEmails, subject, null, htmlContent);
            var response = client.SendEmailAsync(msg);
            return Ok("Mail has been sent to your mail id regarding the reset password");
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetpwd)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Input not valid");
                var user = await _userManager.FindByEmailAsync(resetpwd.email);
                if (user == null) return NotFound($"user with {resetpwd.email} not found!! enter correct email");
                var resetpassResult = await _userManager.ResetPasswordAsync(user, resetpwd.token, resetpwd.password);
                if (!resetpassResult.Succeeded)
                {
                    foreach (var error in resetpassResult.Errors)
                    {
                        return BadRequest(error.Code + error.Description);
                    }
                }
                return Ok("Password changed sucessfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
    
}
