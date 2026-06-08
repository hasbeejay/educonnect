using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Interfaces;
using EduConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Services
{
    public class GradeService : IGradeService
    {
        private readonly NotificationService _notifications;
        private readonly ApplicationDbContext _context;

        public GradeService(NotificationService notifications, ApplicationDbContext context)
        {
            _notifications = notifications;
            _context = context;
        }

        public void SubmitGrade(GradeRecord grade)
        {
            if (grade.Marks < 0 || grade.Marks > 100) throw new ArgumentException("Marks must be between 0 and 100.");
            if (grade.CreditHours <= 0) throw new ArgumentException("Credit hours must be positive.");
            if (string.IsNullOrWhiteSpace(grade.CourseTitle)) throw new ArgumentException("Course title is required.");

            var student = _context.Students.Include(s => s.Grades).FirstOrDefault(s => s.Id == grade.StudentId) 
                ?? throw new ArgumentException("Student not found.");
            
            var existing = student.Grades.FirstOrDefault(g => g.StudentId == grade.StudentId && g.CourseId == grade.CourseId);
            if (existing != null)
            {
                existing.Marks = grade.Marks;
                existing.CourseTitle = grade.CourseTitle;
                existing.CreditHours = grade.CreditHours;
            }
            else
            {
                if (grade.Id == Guid.Empty) grade.Id = Guid.NewGuid();
                student.Grades.Add(grade);
            }

            // refresh student's CGPA
            student.CGPA = ComputeCGPA(grade.StudentId);
            
            _context.SaveChanges();
            
            _notifications.AddNotification($"A grade was posted for {grade.CourseTitle}: {grade.LetterGrade} ({grade.Marks}).", NotificationType.GradePosted, student.Id);
        }

        public List<GradeRecord> GetGradesForCourse(Guid courseId) => _context.Grades
            .Where(g => g.CourseId == courseId)
            .OrderBy(g => g.CourseTitle)
            .ToList();
            
        public List<GradeRecord> GetGradesForStudent(Guid studentId) => _context.Grades
            .Where(g => g.StudentId == studentId)
            .OrderBy(g => g.CourseTitle)
            .ToList();

        public decimal ComputeCGPA(Guid studentId)
        {
            // Use local variable to evaluate calculation in memory to avoid EF Translation issues with the custom property GradePoints.
            // Or just use the already fetched studentGrades from context.
            var studentGrades = _context.Grades.Where(g => g.StudentId == studentId).ToList();
            var totalCredits = studentGrades.Sum(g => g.CreditHours);
            if (totalCredits == 0) return 0.0m;
            var weighted = studentGrades.Sum(g => g.GradePoints * g.CreditHours) / totalCredits;
            return Math.Round(weighted, 2);
        }
    }
}
