using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Interfaces;
using EduConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Handles grade submission and CGPA calculation only. DIP: Receives notification/course dependencies through DI.
    /// </summary>
    public class GradeService : IGradeService
    {
        private readonly NotificationService _notifications;
        private readonly Data.EduConnectContext _db;

        public GradeService(NotificationService notifications, Data.EduConnectContext db)
        {
            _notifications = notifications;
            _db = db;
        }

        public void SubmitGrade(GradeRecord grade)
        {
            if (grade.Marks < 0 || grade.Marks > 100) throw new ArgumentException("Marks must be between 0 and 100.");
            if (grade.CreditHours <= 0) throw new ArgumentException("Credit hours must be positive.");
            if (string.IsNullOrWhiteSpace(grade.CourseTitle)) throw new ArgumentException("Course title is required.");
            var existing = _db.Grades.FirstOrDefault(g => g.StudentId == grade.StudentId && g.CourseId == grade.CourseId);
            if (existing != null)
            {
                existing.Marks = grade.Marks;
                existing.CourseTitle = grade.CourseTitle;
                existing.CreditHours = grade.CreditHours;
            }
            else
            {
                _db.Grades.Add(grade);
            }
            _db.SaveChanges();

            var student = _db.Students.Include(s => s.Grades).FirstOrDefault(s => s.Id == grade.StudentId);
            if (student != null)
            {
                // refresh student's grades collection
                student.CGPA = ComputeCGPA(grade.StudentId);
                _db.SaveChanges();
                _notifications.AddNotification($"A grade was posted for {grade.CourseTitle}: {grade.LetterGrade} ({grade.Marks}).", NotificationType.GradePosted, student.Id);
            }
        }

        public List<GradeRecord> GetGradesForCourse(Guid courseId) => _db.Grades.Where(g => g.CourseId == courseId).OrderBy(g => g.CourseTitle).ToList();
        public List<GradeRecord> GetGradesForStudent(Guid studentId) => _db.Grades.Where(g => g.StudentId == studentId).OrderBy(g => g.CourseTitle).ToList();

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
