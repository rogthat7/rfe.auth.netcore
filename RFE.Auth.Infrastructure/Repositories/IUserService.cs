namespace RFE.Auth.Infrastructure.Repositories
{
    public interface IUserService
    {
        object Authenticate(AuthUserRequest authenticateRequestModel);
    }
}