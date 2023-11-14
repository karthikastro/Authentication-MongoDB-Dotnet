namespace Authentication.Services.TokenBuild
{
    public interface IRefreshTokenValidator
    {
        bool Validate(string refreshToken);
    }
}
