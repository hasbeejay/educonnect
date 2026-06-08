using System;
using System.Linq;
using System.Threading.Tasks;
using EduConnect.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduConnect.Services
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Fast pass: if the database already has students/courses, skip seeding entirely
            if (await context.Students.AnyAsync() && await context.Courses.AnyAsync())
            {
                return;
            }

            // Seed Admins
            var admins = SeedData.Users.OfType<Admin>().ToList();
            foreach (var admin in admins)
            {
                if (!await context.Admins.AnyAsync(a => a.Id == admin.Id))
                {
                    context.Admins.Add(new Admin { Id = admin.Id, FullName = admin.FullName, Email = admin.Email, PasswordHash = admin.PasswordHash });
                }
            }

            // Seed Faculty
            var faculties = SeedData.Faculty;
            foreach (var faculty in faculties)
            {
                if (!await context.Faculty.AnyAsync(f => f.Id == faculty.Id))
                {
                    context.Faculty.Add(new Faculty { Id = faculty.Id, FullName = faculty.FullName, Email = faculty.Email, PasswordHash = faculty.PasswordHash });
                }
            }

            // Seed Students
            var students = SeedData.Students;
            foreach (var student in students)
            {
                if (!await context.Students.AnyAsync(s => s.Id == student.Id))
                {
                    context.Students.Add(new Student { Id = student.Id, FullName = student.FullName, Email = student.Email, PasswordHash = student.PasswordHash, Semester = student.Semester, CGPA = student.CGPA });
                }
            }
            await context.SaveChangesAsync();

            // Seed Courses
            var courses = SeedData.Courses;
            foreach (var course in courses)
            {
                if (!await context.Courses.AnyAsync(c => c.Id == course.Id))
                {
                    context.Courses.Add(new Course
                    {
                        Id = course.Id,
                        Code = course.Code,
                        Title = course.Title,
                        CreditHours = course.CreditHours,
                        MaxCapacity = course.MaxCapacity,
                        IsActive = course.IsActive,
                        AssignedFacultyId = course.AssignedFacultyId
                    });
                }
            }
            await context.SaveChangesAsync();
            
            // Re-establish Course-Student relationships (Enrollments)
            // Note: Since we are using an idempotent seeder, we only want to seed relationships
            // if we just seeded the courses, but doing a simple check ensures idempotency
            foreach (var course in courses)
            {
                var dbCourse = await context.Courses.Include(c => c.Enrolled).FirstOrDefaultAsync(c => c.Id == course.Id);
                if (dbCourse != null)
                {
                    foreach (var student in course.Enrolled)
                    {
                        if (!dbCourse.Enrolled.Any(s => s.Id == student.Id))
                        {
                            var dbStudent = await context.Students.FindAsync(student.Id);
                            if (dbStudent != null)
                            {
                                dbCourse.Enrolled.Add(dbStudent);
                            }
                        }
                    }
                }
            }
            await context.SaveChangesAsync();

            // Seed Grades
            var grades = students.SelectMany(s => s.Grades).ToList();
            foreach (var grade in grades)
            {
                if (!await context.Grades.AnyAsync(g => g.Id == grade.Id || (g.StudentId == grade.StudentId && g.CourseId == grade.CourseId)))
                {
                    var newGrade = new GradeRecord
                    {
                        Id = grade.Id == Guid.Empty ? Guid.NewGuid() : grade.Id,
                        StudentId = grade.StudentId,
                        CourseId = grade.CourseId,
                        CourseTitle = grade.CourseTitle,
                        CreditHours = grade.CreditHours,
                        Marks = grade.Marks
                    };
                    context.Grades.Add(newGrade);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
