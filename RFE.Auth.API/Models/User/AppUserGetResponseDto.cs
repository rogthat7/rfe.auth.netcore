namespace RFE.Auth.API.Models.User
{
    public class AppUserGetResponseDto 
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Phone { get; set; }
    }
}