using Authentication.Models;
using Authentication.Services.TokenBuild;
using Authentication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Authentication.Controllers
{
    [Route("api/RefreshRequest")]
    [ApiController]
    public class RefreshRequestController : ControllerBase
    {
        private readonly IAuthenticateService _authenticateService;
        private readonly IRefreshTokenValidator _refreshTokenValidator;
        private readonly IApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RefreshRequestController(IRefreshTokenValidator refreshTokenValidator, IApplicationDbContext context,
       UserManager<ApplicationUser> userManager, IAuthenticateService authenticateService)
        {
            _refreshTokenValidator = refreshTokenValidator;
            _context = context;
            _userManager = userManager;
            _authenticateService = authenticateService;
        }

        [HttpPost]
        [Route("refresh")]
        //public async Task<IActionResult> RefreshRequest([FromBody]RefreshRequest refreshRequest, CancellationToken cancellationToken = new())
        //{
        //    var result = Handle(refreshRequest, cancellationToken);
            
        //    return Ok(result.Result);
        //}

        public async Task<IActionResult> Handle(RefreshRequest request, CancellationToken cancellationToken)
        {
            var refreshRequest = request;
            var isValidRefreshToken = _refreshTokenValidator.Validate(refreshRequest.RefreshToken);
            if (!isValidRefreshToken)
                return BadRequest("Invalid Refresh Token");
            //var refreshToken =
            //    await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshRequest.RefreshToken,
            //        cancellationToken);
            var refreshTokenFilter = Builders<RefreshToken>.Filter.Eq(x => x.Token, refreshRequest.RefreshToken);
            var refreshToken = await _context.RefreshTokens.Find(refreshTokenFilter).FirstOrDefaultAsync(cancellationToken);

            if (refreshToken is null)
                return NotFound("Refresh Token Not Found");

            await _context.RefreshTokens.DeleteOneAsync(refreshTokenFilter, cancellationToken);
            //await _context.SaveChangesAsync(cancellationToken);

            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user is null) return NotFound("User Not Found");

            var msg = await _authenticateService.Authenticate(user, cancellationToken);
            return Ok(msg);
        }
    }

}
