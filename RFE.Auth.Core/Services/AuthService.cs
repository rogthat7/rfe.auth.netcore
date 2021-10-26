using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository) 
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        }
        public async Task<AuthUser> AuthenticateAuthUser(AuthenticateRequest authenticateRequestModel)
        {
            return await _authRepository.AuthenticateAuthUser(authenticateRequestModel.Username, authenticateRequestModel.Password) as AuthUser;
        }

        public async  Task<List<UserAppPermission>> GetUserAppPermissions(int? UserId)
        {
             return await _authRepository.GetUserAppPermissionsByUserId(UserId) ;
        }
    }
}