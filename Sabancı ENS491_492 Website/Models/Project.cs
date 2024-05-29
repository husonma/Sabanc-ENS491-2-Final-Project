using System;
using System.Collections.Generic;

namespace Sabancı_ENS491_492_Website.Models
{
    public class Project
    {
        public int ProjectId { get; set; } // Unique ID for the project
        public string Title { get; set; } // Title of the project
        public string Description { get; set; } // Description of the project
        public string MajorDesign { get; set; } // Details about the major design
        public string RecommendedDisciplines { get; set; } // Recommended disciplines
        public string ProjectType { get; set; } // Type of the project
        public string? Company { get; set; } // Company associated with the project (nullable)
        public int RecommendedNumberOfStudents { get; set; } // Recommended number of students (nullable)
        public string? relatedCourse { get; set; }
        public bool IsFull { get; set; } // Indicates whether the project is full
        public ICollection<ProjectSupervisor> ProjectSupervisors { get; set; }
    }
}
