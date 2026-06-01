using System;
using System.Collections.Generic;
using EduConnect.Models;

namespace EduConnect.Services
{
    public static class SeedData
    {
        public static List<Person> Users { get; } = new();
        public static List<Student> Students { get; } = new();
        public static List<Course> Courses { get; } = new();
        public static List<Faculty> Faculty { get; } = new();

        static SeedData()
        {
                // Admin
                var admin = new Admin
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FullName = "Haseeb",
                    Email = "haseeb@edu.pk",
                    PasswordHash = "admin123"
                };

                // Faculty
                var taimur = new Faculty
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    FullName = "Mr. Taimur",
                    Email = "taimur@edu.pk",
                    PasswordHash = "faculty123"
                };

                var obaid = new Faculty
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FullName = "Mr. Obaid",
                    Email = "obaid@edu.pk",
                    PasswordHash = "faculty123"
                };

                var junaid = new Faculty
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    FullName = "Mr. Junaid",
                    Email = "junaid@edu.pk",
                    PasswordHash = "faculty123"
                };

                var amna = new Faculty
                {
                    Id = Guid.Parse("55555555-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    FullName = "Ms. Amna",
                    Email = "amna@edu.pk",
                    PasswordHash = "faculty123"
                };

                var eman = new Faculty
                {
                    Id = Guid.Parse("66666666-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    FullName = "Ms. Eman",
                    Email = "eman@edu.pk",
                    PasswordHash = "faculty123"
                };

                // Courses
                var cs101 = new Course
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Code = "CS-101",
                    Title = "Introduction to Programming",
                    CreditHours = 3,
                    MaxCapacity = 30,
                    AssignedFacultyId = taimur.Id
                };

                var cs102 = new Course
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab"),
                    Code = "CS-102",
                    Title = "Programming Lab",
                    CreditHours = 1,
                    MaxCapacity = 25,
                    AssignedFacultyId = obaid.Id
                };

                var cs201 = new Course
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Code = "CS-201",
                    Title = "Data Structures",
                    CreditHours = 3,
                    MaxCapacity = 30,
                    AssignedFacultyId = taimur.Id
                };

                var cs202 = new Course
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc"),
                    Code = "CS-202",
                    Title = "Algorithms",
                    CreditHours = 3,
                    MaxCapacity = 25,
                    AssignedFacultyId = obaid.Id
                };

                var math101 = new Course
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Code = "MATH-101",
                    Title = "Calculus I",
                    CreditHours = 3,
                    MaxCapacity = 40,
                    AssignedFacultyId = junaid.Id
                };

                var eng101 = new Course
                {
                    Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    Code = "ENG-101",
                    Title = "English Composition",
                    CreditHours = 2,
                    MaxCapacity = 50,
                    AssignedFacultyId = amna.Id
                };

                var cs301 = new Course
                {
                    Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    Code = "CS-301",
                    Title = "Databases",
                    CreditHours = 3,
                    MaxCapacity = 20,
                    AssignedFacultyId = eman.Id
                };

                var cs401 = new Course
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Code = "CS-401",
                    Title = "Artificial Intelligence",
                    CreditHours = 3,
                    MaxCapacity = 2,
                    AssignedFacultyId = taimur.Id
                };

                Courses.AddRange(new[] { cs101, cs102, cs201, cs202, math101, eng101, cs301, cs401 });

                // Assign courses to faculties
                taimur.AssignedCourses.AddRange(new[] { cs101, cs201, cs401 });
                obaid.AssignedCourses.AddRange(new[] { cs102, cs202 });
                junaid.AssignedCourses.AddRange(new[] { math101 });
                amna.AssignedCourses.AddRange(new[] { eng101 });
                eman.AssignedCourses.AddRange(new[] { cs301 });

                // Students
                var uzair = new Student
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    FullName = "Uzair Khan",
                    Email = "uzair@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 3,
                    CGPA = 3.6m
                };

                var ali = new Student
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    FullName = "Ali Raza",
                    Email = "ali@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 2,
                    CGPA = 3.2m
                };

                var sara = new Student
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    FullName = "Sara Ahmed",
                    Email = "sara@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 4,
                    CGPA = 3.9m
                };

                var bilal = new Student
                {
                    Id = Guid.Parse("aaaaaaaa-1111-aaaa-1111-aaaaaaaa1111"),
                    FullName = "Bilal Qureshi",
                    Email = "bilal@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 1,
                    CGPA = 2.8m
                };

                var aisha = new Student
                {
                    Id = Guid.Parse("bbbbbbbb-2222-bbbb-2222-bbbbbbbb2222"),
                    FullName = "Aisha Khan",
                    Email = "aisha@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 5,
                    CGPA = 3.4m
                };

                var omar = new Student
                {
                    Id = Guid.Parse("cccccccc-3333-cccc-3333-cccccccc3333"),
                    FullName = "Omar Farooq",
                    Email = "omar@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 6,
                    CGPA = 3.1m
                };

                var fatima = new Student
                {
                    Id = Guid.Parse("dddddddd-4444-dddd-4444-dddddddd4444"),
                    FullName = "Fatima Noor",
                    Email = "fatima@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 2,
                    CGPA = 3.7m
                };

                var noor = new Student
                {
                    Id = Guid.Parse("eeeeeeee-5555-eeee-5555-eeeeeeee5555"),
                    FullName = "Noor Malik",
                    Email = "noor@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 3,
                    CGPA = 3.0m
                };

                var hassan = new Student
                {
                    Id = Guid.Parse("ffffffff-6666-ffff-6666-ffffffff6666"),
                    FullName = "Hassan Ali",
                    Email = "hassan@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 4,
                    CGPA = 2.9m
                };

                var maria = new Student
                {
                    Id = Guid.Parse("11112222-7777-1111-7777-111122227777"),
                    FullName = "Maria Gomes",
                    Email = "maria@student.edu.pk",
                    PasswordHash = "student123",
                    Semester = 1,
                    CGPA = 3.5m
                };

                // REGISTER USERS
                Users.AddRange(new Person[] { admin, taimur, obaid, junaid, amna, eman,
                    uzair, ali, sara, bilal, aisha, omar, fatima, noor, hassan, maria });

                Faculty.AddRange(new[] { taimur, obaid, junaid, amna, eman });

                Students.AddRange(new[] { uzair, ali, sara, bilal, aisha, omar, fatima, noor, hassan, maria });

                // Enroll students in courses (varied to make UI interesting)
                cs101.Enrolled.AddRange(new[] { uzair, ali, sara, bilal, aisha, fatima, noor });
                cs102.Enrolled.AddRange(new[] { uzair, ali, bilal, maria });
                cs201.Enrolled.AddRange(new[] { sara, aisha, omar, hassan });
                cs202.Enrolled.AddRange(new[] { ali, omar, noor });
                math101.Enrolled.AddRange(new[] { uzair, sara, omar, ali, hassan, fatima });
                eng101.Enrolled.AddRange(new[] { maria, bilal, aisha, noor });
                cs301.Enrolled.AddRange(new[] { sara });
                cs401.Enrolled.AddRange(new[] { uzair, sara });

                // Add enrollments to student records
                uzair.Enrollments.AddRange(new[] { cs101, cs102, math101, cs401 });
                ali.Enrollments.AddRange(new[] { cs101, cs102, cs202, math101 });
                sara.Enrollments.AddRange(new[] { cs101, cs201, math101, cs401 });
                bilal.Enrollments.AddRange(new[] { cs101, cs102, eng101 });
                aisha.Enrollments.AddRange(new[] { cs101, cs201, eng101 });
                omar.Enrollments.AddRange(new[] { cs201, cs202, math101 });
                fatima.Enrollments.AddRange(new[] { cs101, math101 });
                noor.Enrollments.AddRange(new[] { cs101, cs202, eng101 });
                hassan.Enrollments.AddRange(new[] { cs201, math101 });
                maria.Enrollments.AddRange(new[] { cs102, eng101 });

                // Grade records (some sample grades)
                uzair.Grades.Add(new GradeRecord { StudentId = uzair.Id, CourseId = cs101.Id, CourseTitle = cs101.Title, CreditHours = cs101.CreditHours, Marks = 88 });
                uzair.Grades.Add(new GradeRecord { StudentId = uzair.Id, CourseId = math101.Id, CourseTitle = math101.Title, CreditHours = math101.CreditHours, Marks = 78 });

                sara.Grades.Add(new GradeRecord { StudentId = sara.Id, CourseId = cs101.Id, CourseTitle = cs101.Title, CreditHours = cs101.CreditHours, Marks = 95 });
                sara.Grades.Add(new GradeRecord { StudentId = sara.Id, CourseId = cs401.Id, CourseTitle = cs401.Title, CreditHours = cs401.CreditHours, Marks = 84 });

                ali.Grades.Add(new GradeRecord { StudentId = ali.Id, CourseId = cs201.Id, CourseTitle = cs201.Title, CreditHours = cs201.CreditHours, Marks = 67 });
                ali.Grades.Add(new GradeRecord { StudentId = ali.Id, CourseId = cs102.Id, CourseTitle = cs102.Title, CreditHours = cs102.CreditHours, Marks = 72 });

                bilal.Grades.Add(new GradeRecord { StudentId = bilal.Id, CourseId = eng101.Id, CourseTitle = eng101.Title, CreditHours = eng101.CreditHours, Marks = 59 });

                // A few students with no grades yet (current semester)

                // Update simple CGPAs (approximate) based on grade points above
                // (These are just sample numbers for UI realism)
                uzair.CGPA = 3.6m;
                sara.CGPA = 3.95m;
                ali.CGPA = 3.1m;
                bilal.CGPA = 2.8m;

                // Ensure student lists for courses are unique and realistic
                foreach (var c in Courses)
                {
                    c.Enrolled = new List<Student>(new HashSet<Student>(c.Enrolled));
                }
        }
    }
}