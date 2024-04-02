using Microsoft.AspNetCore.Mvc;
using Sabancı_ENS491_492_Website.Data;
using System.Linq;
using System.Collections.Generic;
using Sabancı_ENS491_492_Website.Models;

namespace Sabancı_ENS491_492_Website.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ProjectContext _context;

        public ProjectsController(ProjectContext context)
        {
            _context = context;
        }

        public IActionResult ProjectsListView(string disciplines, string projectType, bool hideFullProjects = false)
        {
            var projects = _context.Projects.AsQueryable(); // Start with a queryable to enable filtering

            // Filter by disciplines if any disciplines are checked
            if (!string.IsNullOrEmpty(disciplines))
            {
                projects = projects.Where(p => p.RecommendedDisciplines.Contains(disciplines));
            }

            // Filter by project type if it's set
            if (!string.IsNullOrEmpty(projectType))
            {
                projects = projects.Where(p => p.ProjectType == projectType);
            }

            // Hide full projects if the checkbox is checked
            if (hideFullProjects)
            {
                projects = projects.Where(p => !p.IsFull); // Assuming there is a boolean property IsFull
            }
           
            return View(projects.ToList()); // Materialize the query with ToList after filtering
        }
        public IActionResult ProjectAddView()
        {
            // Check if the user is an instructor
            // You need to implement user authentication and role checks here

            ViewBag.Projects = _context.Projects
                .Where(p => p.Supervisors == "Name of the Logged-in Instructor").ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProject(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                ViewBag.Success = "Project added successfully.";
            }
            return RedirectToAction(nameof(ProjectAddView));
        }

        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                ViewBag.Success = "Project deleted successfully.";
            }
            else
            {
                ViewBag.Error = "Project not found.";
            }
            return RedirectToAction(nameof(ProjectAddView));
        }
    }
}
