using System;
using System.IO;
using System.Threading.Tasks;
using EduConnect.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduConnect.Services
{
    public class DataMigrationService
    {
        public static async Task MigrateFromSqliteAsync(IServiceProvider serviceProvider, string sqliteDbPath)
        {
            if (!File.Exists(sqliteDbPath))
            {
                return; // Nothing to migrate
            }

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataMigrationService>>();

            logger.LogInformation("Starting data migration from SQLite...");

            using var connection = new SqliteConnection($"Data Source={sqliteDbPath}");
            await connection.OpenAsync();

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Migrate Admins
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FullName, Email, PasswordHash FROM Admins";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var id = Guid.Parse(reader.GetString(0));
                        if (!await context.Admins.AnyAsync(a => a.Id == id))
                        {
                            context.Admins.Add(new Admin
                            {
                                Id = id,
                                FullName = reader.GetString(1),
                                Email = reader.GetString(2),
                                PasswordHash = reader.GetString(3)
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Migrate Faculty
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FullName, Email, PasswordHash FROM Faculty";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var id = Guid.Parse(reader.GetString(0));
                        if (!await context.Faculty.AnyAsync(f => f.Id == id))
                        {
                            context.Faculty.Add(new Faculty
                            {
                                Id = id,
                                FullName = reader.GetString(1),
                                Email = reader.GetString(2),
                                PasswordHash = reader.GetString(3)
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Migrate Students
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FullName, Email, PasswordHash, Semester, CGPA FROM Students";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var id = Guid.Parse(reader.GetString(0));
                        if (!await context.Students.AnyAsync(s => s.Id == id))
                        {
                            context.Students.Add(new Student
                            {
                                Id = id,
                                FullName = reader.GetString(1),
                                Email = reader.GetString(2),
                                PasswordHash = reader.GetString(3),
                                Semester = reader.GetInt32(4),
                                CGPA = reader.GetDecimal(5) // In sqlite might be stored as text/real
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Migrate Courses
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Code, Title, CreditHours, MaxCapacity, IsActive, AssignedFacultyId FROM Courses";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var id = Guid.Parse(reader.GetString(0));
                        if (!await context.Courses.AnyAsync(c => c.Id == id))
                        {
                            context.Courses.Add(new Course
                            {
                                Id = id,
                                Code = reader.GetString(1),
                                Title = reader.GetString(2),
                                CreditHours = reader.GetInt32(3),
                                MaxCapacity = reader.GetInt32(4),
                                IsActive = reader.GetInt32(5) != 0,
                                AssignedFacultyId = Guid.TryParse(reader.GetString(6), out var fid) ? fid : Guid.Empty
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Migrate Grades
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, StudentId, CourseId, CourseTitle, CreditHours, Marks FROM Grades";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var id = Guid.Parse(reader.GetString(0));
                        if (!await context.Grades.AnyAsync(g => g.Id == id))
                        {
                            context.Grades.Add(new GradeRecord
                            {
                                Id = id,
                                StudentId = Guid.Parse(reader.GetString(1)),
                                CourseId = Guid.Parse(reader.GetString(2)),
                                CourseTitle = reader.GetString(3),
                                CreditHours = reader.GetInt32(4),
                                Marks = reader.GetInt32(5)
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Migrate CourseStudent (Enrollments)
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT EnrolledId, EnrollmentsId FROM CourseStudent";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var studentId = Guid.Parse(reader.GetString(0));
                        var courseId = Guid.Parse(reader.GetString(1));

                        var course = await context.Courses.Include(c => c.Enrolled).FirstOrDefaultAsync(c => c.Id == courseId);
                        var student = await context.Students.FindAsync(studentId);

                        if (course != null && student != null && !course.Enrolled.Any(s => s.Id == studentId))
                        {
                            course.Enrolled.Add(student);
                        }
                    }
                }
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
                logger.LogInformation("Data migration from SQLite completed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Failed to migrate data from SQLite.");
                throw;
            }
        }
    }
}
