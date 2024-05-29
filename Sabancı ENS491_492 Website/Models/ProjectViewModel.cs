using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sabancı_ENS491_492_Website.Models
{
    public class ProjectViewModel
    {
        public Project NewProject { get; set; }
        public IEnumerable<Project> Projects { get; set; }
        public User CurrentUser { get; set; }
        public IEnumerable<User> AvailableSupervisors { get; set; }
        public IEnumerable<int> SelectedSupervisors { get; set; }
        public IEnumerable<Project> SupervisedProjects { get; set; }
        public IFormFile UploadedFile { get; set; }
        public List<User> Users { get; set; }
        public List<ChatMessage> RecentMessages { get; set; }
        public string relatedCourse { get; set; }
    }
}
