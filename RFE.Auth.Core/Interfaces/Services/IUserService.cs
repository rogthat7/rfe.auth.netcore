using System.Collections.Generic;
using System.Threading.Tasks;
using RFE.Auth.Core.Models.Auth;
using RFE.Auth.Core.Models.Users;

namespace RFE.Auth.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<AppUser>> GetAll();
    }
}