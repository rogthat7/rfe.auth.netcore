using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class UserService : IAuthUserService
    {
        private IAuthUserRepository _userRepository;

        /// <summary>
        /// UserService
        /// </summary>
        /// <param name="userRepository"></param>
        /// <returns></returns>
        public UserService(IAuthUserRepository userRepository) 
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public Task<UnconfirmedAuthUser> AddNewAuthUser(UnconfirmedAuthUser authUser)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AuthUser>> GetAllRegisteredUsers()
        {
            return await _userRepository.All() as List<AuthUser>;
        }

        public async Task<AuthUser> GetUserById(int id)
        {
             return await _userRepository.GetById(id) as AuthUser;
        }
    }
}