using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Handles grade submission and CGPA calculation only. DIP: Receives notification/course dependencies through DI.
    /// </summary>
    public class GradeService : IGradeService
    {
        private readonly NotificationService _notifications;
        public GradeService(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public void SubmitGrade(GradeRecord grade)
        {
            if (grade.Marks < 0 || grade.Marks > 100) throw new ArgumentException("Marks must be between 0 and 100.");
            if (grade.CreditHours <= 0) throw new ArgumentException("Credit hours must be positive.");
            if (string.IsNullOrWhiteSpace(grade.CourseTitle)) throw new ArgumentException("Course title is required.");

            var student = SeedData.Students.FirstOrDefault(s => s.Id == grade.StudentId) ?? throw new ArgumentException("Student not found.");
            var existing = student.Grades.FirstOrDefault(g => g.StudentId == grade.StudentId && g.CourseId == grade.CourseId);
            if (existing != null)
            {
                existing.Marks = grade.Marks;
                existing.CourseTitle = grade.CourseTitle;
                existing.CreditHours = grade.CreditHours;
            }
            else
            {
                student.Grades.Add(grade);
            }

            // refresh student's CGPA
            student.CGPA = ComputeCGPA(grade.StudentId);
            _notifications.AddNotification($"A grade was posted for {grade.CourseTitle}: {grade.LetterGrade} ({grade.Marks}).", NotificationType.GradePosted, student.Id);
        }

        public List<GradeRecord> GetGradesForCourse(Guid courseId) => SeedData.Students.SelectMany(s => s.Grades).Where(g => g.CourseId == courseId).OrderBy(g => g.CourseTitle).ToList();
        public List<GradeRecord> GetGradesForStudent(Guid studentId) => SeedData.Students.FirstOrDefault(s => s.Id == studentId)?.Grades.OrderBy(g => g.CourseTitle).ToList() ?? new List<GradeRecord>();

        public decimal ComputeCGPA(Guid studentId)
        {
            var studentGrades = GetGradesForStudent(studentId);
            var totalCredits = studentGrades.Sum(g => g.CreditHours);
            if (totalCredits == 0) return 0.0m;
            var weighted = studentGrades.Sum(g => g.GradePoints * g.CreditHours) / totalCredits;
            return Math.Round(weighted, 2);
        }
    }
}
