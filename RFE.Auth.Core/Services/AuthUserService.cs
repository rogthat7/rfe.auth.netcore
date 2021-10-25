using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Email;
using RFE.Auth.Core.Models.User;

namespace RFE.Auth.Core.Services
{
    public class AuthUserService : IAuthUserService
    {
        private IAuthUserRepository _userRepository;
        private IEmailSender _emailSenderService;
        // private string _unconfirmedUserName = null;
        //private Timer userDestructionTimer;

        /// <summary>
        /// UserService
        /// </summary>
        /// <param name="userRepository"></param>
        /// <returns></returns>
        public AuthUserService(IAuthUserRepository userRepository, IEmailSender emailSenderService ) 
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailSenderService = emailSenderService ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task AddNewAuthUser(AuthUser confirmedUser)
        {
            await _userRepository.AddNewConfirmedUser(confirmedUser);
        }

        public async Task<List<AuthUserByIdGetResponse>> GetAllRegisteredUsers()
        {
            return await _userRepository.All() as List<AuthUserByIdGetResponse>;
        }

        public async Task<AuthUserByIdGetResponse> GetUserById(int id)
        {
             return await _userRepository.GetById(id) as AuthUserByIdGetResponse;
        }

        public async Task<AuthUser> AuthenticateAuthUser(string username, string password)
        {
            return await _userRepository.AuthenticateAuthUser(username, password) as AuthUser;
        }
    }
}