using AutoMapper;
using RFE.Auth.Core.Models.Users;

namespace RFE.Auth.API.Models.User
{
    public class UserMappingProfile: Profile {
     public UserMappingProfile() {
         // Add as many of these lines as you need to map your objects
        this.CreateMap<AppUser, AppUserGetResponseDto>();
     }
    }
}