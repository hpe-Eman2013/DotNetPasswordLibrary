using System;

namespace PasswordCoreLibrary.Models
{
    public class Passwords
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public string ApplicationPath { get; set; }
        public string SpecialNotes { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int UserId { get; set; }
    }
}
