namespace PasswordCoreLibrary.Models
{
    public class EmailAccount
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public string PhotoLocation { get; set; }
    }
}
