namespace Sabancı_ENS491_492_Website.Models
{
    public class ProjectSupervisor
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Project Project { get; set; }
        public User User { get; set; }
    }

}
