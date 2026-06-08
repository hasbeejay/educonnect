using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Exceptions;
using EduConnect.Interfaces;
using EduConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Services
{
    public class CourseService : ICourseService
    {
        private readonly NotificationService _notifications;
        private readonly ApplicationDbContext _context;

        public CourseService(NotificationService notifications, ApplicationDbContext context)
        {
            _notifications = notifications;
            _context = context;
        }

        public List<Course> GetAll() => _context.Courses
            .Include(c => c.Enrolled)
            .ToList();

        public Course? GetById(Guid id) => _context.Courses
            .Include(c => c.Enrolled)
            .FirstOrDefault(c => c.Id == id);

        public void Add(Course entity)
        {
            Validate(entity);
            if (_context.Courses.Any(c => c.Code.ToLower() == entity.Code.ToLower())) 
                throw new ArgumentException("A course with this code already exists.");
            
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            
            _context.Courses.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Course entity)
        {
            Validate(entity);
            var existing = _context.Courses.Include(c => c.Enrolled).FirstOrDefault(c => c.Id == entity.Id) 
                ?? throw new ArgumentException("Course not found.");
                
            if (_context.Courses.Any(c => c.Id != entity.Id && c.Code.ToLower() == entity.Code.ToLower())) 
                throw new ArgumentException("A course with this code already exists.");
                
            if (entity.MaxCapacity < existing.Enrolled.Count) 
                throw new ArgumentException($"Cannot reduce capacity below current enrollment count ({existing.Enrolled.Count}).");
            
            existing.Code = entity.Code;
            existing.Title = entity.Title;
            existing.CreditHours = entity.CreditHours;
            existing.MaxCapacity = entity.MaxCapacity;
            existing.AssignedFacultyId = entity.AssignedFacultyId;
            existing.IsActive = entity.IsActive;
            
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var course = _context.Courses.Include(c => c.Enrolled).FirstOrDefault(c => c.Id == id);
            if (course == null) return;
            if (course.Enrolled.Any()) 
                throw new InvalidOperationException($"Course {course.Code} has enrolled students and cannot be deleted.");
                
            _context.Courses.Remove(course);
            _context.SaveChanges();
        }

        public void EnrollStudent(Guid courseId, Guid studentId)
        {
            var course = _context.Courses.Include(c => c.Enrolled).FirstOrDefault(c => c.Id == courseId) 
                ?? throw new ArgumentException("Course not found.");
            if (!course.IsActive) throw new InvalidOperationException($"Course {course.Code} is inactive.");
            if (course.EnrollmentStatus == EnrollmentStatus.Full) throw new CourseFullException($"Course {course.Code} is full.");
            
            var student = _context.Students.Include(s => s.Enrollments).FirstOrDefault(s => s.Id == studentId) 
                ?? throw new ArgumentException("Student not found.");
                
            if (course.Enrolled.Any(s => s.Id == studentId) || student.Enrollments.Any(c => c.Id == courseId)) 
                throw new InvalidOperationException($"{student.FullName} is already enrolled in {course.Code}.");
                
            course.Enrolled.Add(student);
            _context.SaveChanges();
            
            _notifications.AddNotification($"You enrolled in {course.Code} - {course.Title}.", NotificationType.Enrollment, student.Id);
        }

        public void DropCourse(Guid courseId, Guid studentId)
        {
            var course = _context.Courses.Include(c => c.Enrolled).FirstOrDefault(c => c.Id == courseId) 
                ?? throw new ArgumentException("Course not found.");
            if (!course.IsActive) throw new InvalidOperationException($"Course {course.Code} is inactive and cannot be dropped.");
            
            var student = _context.Students.Include(s => s.Enrollments).FirstOrDefault(s => s.Id == studentId) 
                ?? throw new ArgumentException("Student not found.");
                
            var enrolledStudent = course.Enrolled.FirstOrDefault(s => s.Id == studentId);
            if (enrolledStudent == null) throw new InvalidOperationException($"{student.FullName} is not enrolled in {course.Code}.");
            
            course.Enrolled.Remove(enrolledStudent);
            _context.SaveChanges();
        }

        public List<Course> GetForFaculty(Guid facultyId) => _context.Courses
            .Include(c => c.Enrolled)
            .Where(c => c.AssignedFacultyId == facultyId)
            .ToList();
            
        public List<Course> GetAvailable() => _context.Courses
            .Include(c => c.Enrolled)
            .Where(c => c.IsActive && c.Enrolled.Count < c.MaxCapacity)
            .ToList();

        private static void Validate(Course entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Code)) throw new ArgumentException("Course code is required.");
            if (string.IsNullOrWhiteSpace(entity.Title)) throw new ArgumentException("Course title is required.");
            if (entity.CreditHours <= 0) throw new ArgumentException("Credit hours must be positive.");
            if (entity.MaxCapacity <= 0) throw new ArgumentException("Max capacity must be positive.");
        }
    }
}
