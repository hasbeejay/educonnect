using Microsoft.EntityFrameworkCore;
using EduConnect.Models;

namespace EduConnect.Data
{
    public class EduConnectContext : DbContext
    {
        public EduConnectContext(DbContextOptions<EduConnectContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Faculty> Faculty { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<GradeRecord> Grades { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-many between Students and Courses
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Enrollments)
                .WithMany(c => c.Enrolled);
        }
    }
}
