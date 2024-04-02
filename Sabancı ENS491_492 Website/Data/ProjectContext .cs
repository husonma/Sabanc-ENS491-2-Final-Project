using Microsoft.EntityFrameworkCore;
using Sabancı_ENS491_492_Website.Models;
using System.Collections.Generic;

namespace Sabancı_ENS491_492_Website.Data
{
    public class ProjectContext : DbContext
    {
        public ProjectContext()
        {
        }

        public ProjectContext(DbContextOptions<ProjectContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
