using Microsoft.EntityFrameworkCore;
using CMCSApplication.Models;
using System.Collections.Generic;

namespace CMCSApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }

        public DbSet<ModuleAssignment> ModuleAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "IT", HourlyRate = 0 },
                new Department { Id = 2, Name = "Business", HourlyRate = 0 },
                new Department { Id = 3, Name = "Engineering", HourlyRate = 0 },
                new Department { Id = 4, Name = "Law", HourlyRate = 0 },
                new Department { Id = 5, Name = "Medicine", HourlyRate = 0 },
                new Department { Id = 6, Name = "Education", HourlyRate = 0 },
                new Department { Id = 7, Name = "Psychology", HourlyRate = 0 },
                new Department { Id = 8, Name = "Accounting", HourlyRate = 0 },
                new Department { Id = 9, Name = "Humanities", HourlyRate = 0 }
            );
        }

    }
}
