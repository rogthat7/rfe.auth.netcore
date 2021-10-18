using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Users;

namespace RFE.Auth.Core.Services
{
    public class UserService : IUserService
    {
        public Task<List<AppUser>> GetAll()
        {
            throw new System.NotImplementedException();
        }
    }
}