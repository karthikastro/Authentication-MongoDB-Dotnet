using Authentication.Models;
using Authentication.Services.TokenBuild;
using MongoDB.Driver;

namespace Authentication.Services
{
    public class AuthenticateService:IAuthenticateService
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IApplicationDbContext _context;
        public AuthenticateService(IAccessTokenService accessTokenService, IRefreshTokenService refreshTokenService, IApplicationDbContext context)
        {
            _accessTokenService = accessTokenService;
            _refreshTokenService = refreshTokenService;
            _context = context;
        }

        public async Task<AuthenticateResponse> Authenticate(ApplicationUser user, CancellationToken cancellationToken)
        {
            var existingRefreshToken = await _context.RefreshTokens
                                                  .Find(x => x.UserId == user.Id)
                                                  .FirstOrDefaultAsync(cancellationToken);
            if (existingRefreshToken != null)
            {
                // Update the existing refresh token with a new one
                existingRefreshToken.Token = _refreshTokenService.Generate(user);
                var filter = Builders<RefreshToken>.Filter.Eq(x => x.UserId, user.Id);
                var update = Builders<RefreshToken>.Update.Set(x => x.Token, existingRefreshToken.Token);

                await _context.RefreshTokens.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true },cancellationToken);
                //var filter = Builders<RefreshToken>.Filter.Eq(x => x.UserId, user.Id);
                //var update = Builders<RefreshToken>.Update.Set(x => x.Token, new RefreshToken);

                //var result = await _context.RefreshTokens.ReplaceOneAsync(filter, new RefreshToken
                //{
                //    UserId = user.Id,
                //    Token = newRefreshToken.Token
                //}, new ReplaceOptions { IsUpsert = true }, cancellationToken);
                // Return the updated refresh token
                return new AuthenticateResponse
                {
                    AccessToken = _accessTokenService.Generate(user),
                    RefreshToken = existingRefreshToken.Token
                };
            }
            else
            {
                var refreshToken = _refreshTokenService.Generate(user);
                //await _context.RefreshTokens.AddAsync(new RefreshToken(user.Id, refreshToken), cancellationToken);
                //await _context.SaveChangesAsync(cancellationToken);
                var refreshTokenDocument = new RefreshToken(user.Id, refreshToken);
                await _context.RefreshTokens.InsertOneAsync(refreshTokenDocument, cancellationToken);

                return new AuthenticateResponse
                {
                    AccessToken = _accessTokenService.Generate(user),
                    RefreshToken = refreshToken
                };
            }

           
        }
    }
}
