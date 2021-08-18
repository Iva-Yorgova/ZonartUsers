﻿using System.ComponentModel.DataAnnotations;

namespace ZonartUsers.Models.Users
{
    public class LoginUserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string FullName { get; set; }
    }
}
