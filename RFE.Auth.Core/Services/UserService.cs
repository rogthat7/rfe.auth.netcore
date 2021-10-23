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
    public class UserService : IAuthUserService
    {
        private readonly string USER_CONFIRMATION_SUBJECT = "USER_CONFIRMATION_SUBJECT";
        private readonly string USER_CONFIRMATION_BODY = "USER_CONFIRMATION_BODY";
        private IAuthUserRepository _userRepository;
        private IEmailSender _emailSenderService;
        private string _unconfirmedUserName = null;
        private Timer userDestructionTimer;

        /// <summary>
        /// UserService
        /// </summary>
        /// <param name="userRepository"></param>
        /// <returns></returns>
        public UserService(IAuthUserRepository userRepository, IEmailSender emailSenderService ) 
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailSenderService = emailSenderService ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<bool> AddNewAuthUser(UnconfirmedAuthUser unconfirmedUser)
        {
            await _userRepository.DeleteUnconfirmedUser(unconfirmedUser.Username);
            var userCreated =  await _userRepository.AddNewUnconfirmedUser(unconfirmedUser);
            if (userCreated)
            {
                Message message = new Message(new string[] {unconfirmedUser.Email}, USER_CONFIRMATION_SUBJECT, USER_CONFIRMATION_BODY); 
                await _emailSenderService.SendEmail(message);
                this._unconfirmedUserName = unconfirmedUser.Username;
                SetUserDestructionTimer(unconfirmedUser);
                return true;
            }
            else
                return false;
        }


        public async Task<List<AuthUserByIdGetResponse>> GetAllRegisteredUsers()
        {
            return await _userRepository.All() as List<AuthUserByIdGetResponse>;
        }

        public async Task<AuthUserByIdGetResponse> GetUserById(int id)
        {
             return await _userRepository.GetById(id) as AuthUserByIdGetResponse;
        }

        
        private  void SetUserDestructionTimer(UnconfirmedAuthUser unconfirmedUser)
        {
            userDestructionTimer = new Timer(60000);
            userDestructionTimer.AutoReset = false;
            userDestructionTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            userDestructionTimer.Start();
            
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            var userDeleted = await _userRepository.DeleteUnconfirmedUser(this._unconfirmedUserName);
            userDestructionTimer.Stop();
            userDestructionTimer.Dispose();
        }
    }
}