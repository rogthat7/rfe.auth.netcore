using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;

        /// <summary>
        /// UserService
        /// </summary>
        /// <param name="userRepository"></param>
        /// <returns></returns>
        public UserService(IUserRepository userRepository) 
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<List<AppUser>> GetAllRegisteredUsers()
        {
            return await _userRepository.All() as List<AppUser>;
        }
    }
}