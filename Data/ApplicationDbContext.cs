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

        public DbSet<ModuleAssignment> ModuleAssignments { get; set; }

    }
}
