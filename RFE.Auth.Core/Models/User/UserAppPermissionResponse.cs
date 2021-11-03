using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace RFE.Auth.Core.Models.User
{
    public class UserAppPermissionResponse
    {
        public int UAPId { get; set; }
        public string AppName { get; set; }
        public string Username { get; set; }
        public string PermissionName { get; set; }
        public string PermissionType { get; set; }
    }
}