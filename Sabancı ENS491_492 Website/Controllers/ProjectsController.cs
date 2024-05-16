using Microsoft.AspNetCore.Mvc;
using Sabancı_ENS491_492_Website.Data;
using System.Linq;
using System.Collections.Generic;
using Sabancı_ENS491_492_Website.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Sabancı_ENS491_492_Website.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ProjectContext _context;

        public ProjectsController(ProjectContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ProjectsListView(string disciplines, string projectType, bool hideFullProjects = false)
        {

            // Retrieve email from claims
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Fetch the user details based on the email
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            if (currentUser == null)
            {
                // Handle the case where the user is not found
                return RedirectToAction("Login", "Account");
            }
            // Start with a queryable that includes related supervisors
            var projects = _context.Projects
                .Include(p => p.ProjectSupervisors)
                    .ThenInclude(ps => ps.User)
                .AsQueryable();

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

            var viewModel = new ProjectViewModel // Assume you create this ViewModel
            {
                Projects = await projects.ToListAsync(),
                CurrentUser = currentUser // Pass current user data to the view
            };

            return View(viewModel); // Pass the list of projects to the view
        }

        public async Task<IActionResult> ProjectAddView()
        {
            // Load projects and include necessary navigation properties
            var projects = _context.Projects
                                   .Include(p => p.ProjectSupervisors)
                                       .ThenInclude(ps => ps.User)
                                   .AsQueryable();

            // Create the view model and populate it with projects and available supervisors
            var viewModel = new ProjectViewModel
            {
                NewProject = new Project(), // Initialize new project for the form
                Projects = await projects.ToListAsync(), // Async loading of projects
                AvailableSupervisors = await _context.Users
                                                     .Where(u => u.Role == "instructor")
                                                     .ToListAsync() // Async loading of instructors
            };

            // Check TempData for any success message and assign it to ViewBag if available
            if (TempData["Success"] != null)
            {
                ViewBag.Success = TempData["Success"].ToString();
            }

            // Return the view with the populated view model
            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> AddProject(ProjectViewModel viewModel)
        {
            // Create and add the new project
            var newProject = new Project
            {
                Title = viewModel.NewProject.Title,
                Description = viewModel.NewProject.Description,
                MajorDesign = viewModel.NewProject.MajorDesign,
                RecommendedDisciplines = viewModel.NewProject.RecommendedDisciplines,
                ProjectType = viewModel.NewProject.ProjectType,
                Company = viewModel.NewProject.Company,
                RecommendedNumberOfStudents = viewModel.NewProject.RecommendedNumberOfStudents,
                IsFull = viewModel.NewProject.IsFull
            };

            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync(); // This will generate newProject.ProjectId

            // Conditionally link supervisors after the project has been saved
            if (User.IsInRole("admin") && viewModel.SelectedSupervisors != null)
            {
                foreach (var supervisorId in viewModel.SelectedSupervisors)
                {
                    _context.ProjectSupervisors.Add(new ProjectSupervisor { ProjectId = newProject.ProjectId, UserId = supervisorId });

                }
            }
            else if (User.IsInRole("instructor"))
            {
                var supervisorId = _context.Users.FirstOrDefault(u => u.EmailAddress == User.Identity.Name)?.UserId;
                if (supervisorId != null)
                {
                    _context.ProjectSupervisors.Add(new ProjectSupervisor { ProjectId = newProject.ProjectId, UserId = supervisorId.Value });
                }
            }

            await _context.SaveChangesAsync(); // Save changes again to update the links
            TempData["SuccessMessage"] = "Project added successfully.";
            return RedirectToAction("ProjectAddView");
        }





        public IActionResult ManageProjects()
        {
                var loggedInSupervisorName = User.Identity.Name; // or however you're identifying the supervisor

                var viewModel = new ProjectViewModel
                {
                    NewProject = new Project(),
                    SupervisedProjects = _context.Projects
                        .Include(p => p.ProjectSupervisors)
                            .ThenInclude(ps => ps.User)
                        .Where(p => p.ProjectSupervisors.Any(ps => ps.User.Name == loggedInSupervisorName))
                        .ToList() ?? new List<Project>() // Use ?? to provide an empty list if the result is null
                };

                return View(viewModel);

        }




        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects
                                        .Include(p => p.ProjectSupervisors) // Include supervisors to access them directly
                                        .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project != null)
            {
                // Remove supervisor associations first
                _context.ProjectSupervisors.RemoveRange(project.ProjectSupervisors);

                // Now remove the project
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Project deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Project not found.";
            }

            return RedirectToAction("ProjectAddView");
        }

    }
}
