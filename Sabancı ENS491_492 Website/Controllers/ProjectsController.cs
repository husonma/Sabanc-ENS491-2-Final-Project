using Microsoft.AspNetCore.Mvc;
using Sabancı_ENS491_492_Website.Data;
using System.Linq;
using System.Collections.Generic;
using Sabancı_ENS491_492_Website.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Sabancı_ENS491_492_Website.Services;
using Microsoft.AspNetCore.SignalR;
using Sabancı_ENS491_492_Website.Hubs;

namespace Sabancı_ENS491_492_Website.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ProjectContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProjectsController(ProjectContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> ProjectsListView(string scrollTo, string disciplines, string projectType, bool hideFullProjects = false, string advanceFilter = "")
        {

            // Convert the comma-separated string 'disciplines' and 'advanceFilter' into lists
            List<string> disciplineList = !string.IsNullOrEmpty(disciplines)
                ? disciplines.Split(',').Select(d => d.Trim()).ToList()
                : new List<string>();
            List<string> advanceFilterList = !string.IsNullOrEmpty(advanceFilter)
                ? advanceFilter.Split(',').Select(af => af.Trim()).ToList()
                : new List<string>();

            // Retrieve user details based on the email from claims
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Initialize the queryable to include related navigation properties
            var projectsQuery = _context.Projects
                .Include(p => p.ProjectSupervisors).ThenInclude(ps => ps.User)
                .AsQueryable();

            // Apply filters based on the disciplines and project type
            if (disciplineList.Any())
            {
                projectsQuery = projectsQuery.Where(p => disciplineList.Any(d => p.RecommendedDisciplines.Contains(d)));
            }

            if (!string.IsNullOrEmpty(projectType))
            {
                projectsQuery = projectsQuery.Where(p => p.ProjectType == projectType);
            }

            if (hideFullProjects)
            {
                projectsQuery = projectsQuery.Where(p => !p.IsFull);
            }

            // Evaluate the query from the database
            var projects = await projectsQuery.ToListAsync();

            // Apply advanced filter in-memory if there are any advanced filter criteria
            if (advanceFilterList.Any())
            {
                projects = projects.Where(p => p.relatedCourse != null && advanceFilterList.Any(af => p.relatedCourse.Contains(af, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            var viewModel = new ProjectViewModel
            {
                Projects = projects,
                CurrentUser = currentUser
            };

            ViewBag.ScrollTo = scrollTo;

            return View(viewModel);
        }
        public async Task<IActionResult> ChatView()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await _context.Users.Where(u => u.UserId != currentUser.UserId).ToListAsync();

            var viewModel = new ProjectViewModel
            {
                CurrentUser = currentUser,
                Users = users
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GetMessages(int userId)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == currentUser.UserId && m.ReceiverId == userId) || (m.SenderId == userId && m.ReceiverId == currentUser.UserId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    senderName = m.Sender.Name,
                    text = m.Message,
                    timestamp = m.Timestamp
                })
                .ToListAsync();

            return Json(messages);
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
        public async Task<IActionResult> ManageProjectsView()
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
                relatedCourse = viewModel.NewProject.relatedCourse,
                ProjectType = viewModel.NewProject.ProjectType,
                Company = viewModel.NewProject.Company,
                RecommendedNumberOfStudents = viewModel.NewProject.RecommendedNumberOfStudents,
                IsFull = viewModel.NewProject.IsFull,
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


        [HttpPost]
        public async Task<IActionResult> UploadTranscript(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Prepare the path for file saving
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string filePath = Path.Combine(uploadsFolder, file.FileName);

                // Ensure the uploads directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Check if file exists and delete it
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Find the user in the database
                var userEmail = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == userEmail);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Update the user's PDF file path
                user.PDFFilePath = filePath;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("ProjectsListView"); // Adjust as needed
            }

            return View(); // Or return an appropriate response if file is not provided
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
    

        [HttpPost]
        public async Task<IActionResult> AnalyzeTranscript()
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == userEmail);

            if (user == null || string.IsNullOrEmpty(user.PDFFilePath))
            {
                return View("Error"); // Handle no user or no file scenario
            }

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, user.PDFFilePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return View("Error"); // File does not exist
            }

            TranscriptAnalyzer analyzer = new TranscriptAnalyzer();
            var highGradeCourses = analyzer.ExtractHighGrades(fullPath);

            // Convert highGradeCourses to a comma-separated string to pass as a query parameter
            string advanceFilter = String.Join(",", highGradeCourses);

            // Redirect to ProjectsListView with the extracted disciplines as a parameter
            return RedirectToAction("ProjectsListView", new { advanceFilter = advanceFilter });
        }




    }
}
