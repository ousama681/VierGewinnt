﻿namespace VierGewinnt.Models
{
    public class EmailConfirmModel
    {
        public string Email { get; set; }
        public bool IsConfirmed { get; set; }
        public bool EmailSent { get; set; }
        public bool EmailVerified { get; set; }
        public string Token { get; set; }
        public string Uid { get; set; }

    }
}
