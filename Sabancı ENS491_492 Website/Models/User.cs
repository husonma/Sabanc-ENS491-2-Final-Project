namespace Sabancı_ENS491_492_Website.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; } 
        public string Role { get; set; }
        public int? ProjectId { get; set; }
        public Project Project { get; set; }
    }

}
