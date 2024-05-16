namespace Sabancı_ENS491_492_Website.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int? SchoolID { get; set; }          // Nullable integer for SchoolID
        public string Program { get; set; }         // Nullable string for Program
        public string PDFFilePath { get; set; }     // Nullable string for PDF file path
        public string ImgFilePath { get; set; }     // Nullable string for Img file path
        public ICollection<ProjectSupervisor> ProjectSupervisors { get; set; }
    }
}
