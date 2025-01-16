
using System;

namespace ArabaSatisWeb.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string OldEmail { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = "User";
        public string ProfileImageURL { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
    }
}