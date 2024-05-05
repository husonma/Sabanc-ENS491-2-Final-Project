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
        public DbSet<ProjectSupervisor> ProjectSupervisors { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the ProjectSupervisor join entity
            modelBuilder.Entity<ProjectSupervisor>()
                .HasKey(ps => new { ps.ProjectId, ps.UserId }); // Composite key

            // Configure the relationship from ProjectSupervisor to Project
            modelBuilder.Entity<ProjectSupervisor>()
                .HasOne<Project>(ps => ps.Project)
                .WithMany(p => p.ProjectSupervisors) // Here, ensure you have a collection for join entities
                .HasForeignKey(ps => ps.ProjectId);

            // Configure the relationship from ProjectSupervisor to User
            modelBuilder.Entity<ProjectSupervisor>()
                .HasOne<User>(ps => ps.User)
                .WithMany(u => u.ProjectSupervisors) // Here too, a collection for join entities
                .HasForeignKey(ps => ps.UserId);
        }

    }
}
