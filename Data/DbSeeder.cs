using System.Linq;
using EduConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Data
{
    public static class DbSeeder
    {
        public static void EnsureSeedData(EduConnectContext context)
        {
            context.Database.EnsureCreated();

            if (context.Students.Any() || context.Courses.Any()) return; // already seeded

            // Use existing in-memory SeedData to populate DB on first run
            foreach (var user in Services.SeedData.Users)
            {
                switch (user)
                {
                    case Admin admin:
                        context.Admins.Add(admin);
                        break;
                    case Faculty faculty:
                        context.Faculty.Add(faculty);
                        break;
                    case Student student:
                        context.Students.Add(student);
                        break;
                }
            }

            foreach (var course in Services.SeedData.Courses)
            {
                context.Courses.Add(course);
            }

            context.SaveChanges();
        }
    }
}
