using Authentication.Models;

namespace Authentication.Services
{
    public interface IAuthenticateService
    {
        Task<AuthenticateResponse> Authenticate(ApplicationUser user, CancellationToken cancellationToken);
    }
}
