using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace RFE.Auth.Core.Models.User
{
    [Serializable]
    public class AuthUser
    {
        [Key]
        public int?     UserId      { get; set; }
        public string   Username    { get; set; }
        public string   Email       { get; set; }
        public long?    Phone       { get; set; }
        public virtual bool Confirmed { get; set; } = false;
        public virtual string Password { get; set; }
    }
}