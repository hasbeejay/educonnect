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
            // =========================
            // ADMIN
            // =========================
            var admin = new Admin
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FullName = "Haseeb",
                Email = "haseeb@edu.pk",
                PasswordHash = "admin123"
            };

            // =========================
            // FACULTY
            // =========================
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

            // =========================
            // STUDENT
            // =========================
            var uzair = new Student
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                FullName = "Uzair",
                Email = "uzair@student.edu.pk",
                PasswordHash = "student123",
                Semester = 3,
                CGPA = 3.6m
            };

            // =========================
            // REGISTER USERS
            // =========================
            Users.AddRange(new Person[] { admin, taimur, obaid, junaid, amna, eman, uzair });

            Faculty.AddRange(new[]
            {
                taimur, obaid, junaid, amna, eman
            });

            Students.AddRange(new[] { uzair });

            // =========================
            // COURSES (kept as-is)
            // =========================
            var cs101 = new Course
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Code = "CS-101",
                Title = "Introduction to Programming",
                CreditHours = 3,
                MaxCapacity = 30,
                AssignedFacultyId = taimur.Id
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

            var cs401 = new Course
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Code = "CS-401",
                Title = "Artificial Intelligence",
                CreditHours = 3,
                MaxCapacity = 2,
                AssignedFacultyId = taimur.Id
            };

            Courses.AddRange(new[] { cs101, cs201, cs401 });

            taimur.AssignedCourses.AddRange(new[] { cs101, cs201, cs401 });

            cs101.Enrolled.Add(uzair);
            cs201.Enrolled.Add(uzair);
            cs401.Enrolled.Add(uzair);

            uzair.Enrollments.AddRange(new[] { cs101, cs201, cs401 });
        }
    }
}