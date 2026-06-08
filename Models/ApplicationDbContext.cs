using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Faculty> Faculty { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<GradeRecord> Grades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPC (Table-per-concrete-type) for Person
            // This maps Admin, Faculty, and Student to separate tables
            modelBuilder.Entity<Person>().UseTpcMappingStrategy();

            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Faculty>().ToTable("Faculty");
            modelBuilder.Entity<Student>().ToTable("Students");

            modelBuilder.Entity<Course>()
                .HasOne<Faculty>()
                .WithMany(f => f.AssignedCourses)
                .HasForeignKey(c => c.AssignedFacultyId)
                .IsRequired(false) // Sometimes not assigned
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Enrolled)
                .WithMany(s => s.Enrollments)
                .UsingEntity(j => j.ToTable("CourseStudent"));

            modelBuilder.Entity<GradeRecord>()
                .HasOne<Student>()
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
