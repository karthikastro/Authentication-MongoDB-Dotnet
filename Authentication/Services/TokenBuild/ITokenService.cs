using Authentication.Models;

namespace Authentication.Services.TokenBuild
{
    public interface ITokenService
    {
        string Generate(ApplicationUser user);
    }
    public interface IRefreshTokenService : ITokenService { }
    public interface IAccessTokenService : ITokenService { }
}
