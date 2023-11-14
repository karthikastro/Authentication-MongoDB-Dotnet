using Authentication.Models;
using System.Security.Claims;

namespace Authentication.Services.TokenBuild
{
    public class AccessTokenService:IAccessTokenService
    {
        private readonly ITokenGenerator _tokenGenerator;
        private readonly JwtSettings _jwtSettings;

        public AccessTokenService(ITokenGenerator tokenGenerator, JwtSettings jwtSettings) =>
            (_tokenGenerator, _jwtSettings) = (tokenGenerator, jwtSettings);

        public string Generate(ApplicationUser user)
        {
            List<Claim> claims = new()
        {
            new Claim("id", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim("UserName", user.UserName),
            new Claim("FullName",user.FullName)
        };
            return _tokenGenerator.Generate(_jwtSettings.AccessTokenSecret, _jwtSettings.Issuer, _jwtSettings.Audience,
                _jwtSettings.AccessTokenExpirationMinutes, claims);
        }
    }
}
