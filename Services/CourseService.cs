using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Exceptions;
using EduConnect.Interfaces;
using EduConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Manages course catalog and enrollment rules. DIP: Receives NotificationService through DI instead of constructing dependencies.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly NotificationService _notifications;
        private readonly Data.EduConnectContext _db;

        public CourseService(NotificationService notifications, Data.EduConnectContext db)
        {
            _notifications = notifications;
            _db = db;
        }

        public List<Course> GetAll() => _db.Courses.Include(c => c.Enrolled).ToList();
        public Course? GetById(Guid id) => _db.Courses.Include(c => c.Enrolled).FirstOrDefault(c => c.Id == id);

        public void Add(Course entity)
        {
            Validate(entity);
            if (_db.Courses.Any(c => c.Code.Equals(entity.Code, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("A course with this code already exists.");
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            _db.Courses.Add(entity);
            SyncFacultyAssignment(entity);
            _db.SaveChanges();
        }

        public void Update(Course entity)
        {
            Validate(entity);
            var existing = GetById(entity.Id) ?? throw new ArgumentException("Course not found.");
            if (_db.Courses.Any(c => c.Id != entity.Id && c.Code.Equals(entity.Code, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("A course with this code already exists.");
            if (entity.MaxCapacity < existing.Enrolled.Count) throw new ArgumentException($"Cannot reduce capacity below current enrollment count ({existing.Enrolled.Count}).");
            foreach (var faculty in _db.Faculty.Include(f => f.AssignedCourses)) faculty.AssignedCourses.RemoveAll(c => c.Id == existing.Id);
            existing.Code = entity.Code;
            existing.Title = entity.Title;
            existing.CreditHours = entity.CreditHours;
            existing.MaxCapacity = entity.MaxCapacity;
            existing.AssignedFacultyId = entity.AssignedFacultyId;
            existing.IsActive = entity.IsActive;
            SyncFacultyAssignment(existing);
            _db.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var course = GetById(id);
            if (course == null) return;
            if (course.Enrolled.Any()) throw new InvalidOperationException($"Course {course.Code} has enrolled students and cannot be deleted.");
            _db.Courses.Remove(course);
            foreach (var faculty in _db.Faculty.Include(f => f.AssignedCourses)) faculty.AssignedCourses.RemoveAll(c => c.Id == id);
            _db.SaveChanges();
        }

        public void EnrollStudent(Guid courseId, Guid studentId)
        {
            var course = GetById(courseId) ?? throw new ArgumentException("Course not found.");
            if (!course.IsActive) throw new InvalidOperationException($"Course {course.Code} is inactive.");
            if (course.EnrollmentStatus == EnrollmentStatus.Full) throw new CourseFullException($"Course {course.Code} is full.");
            var student = _db.Students.Include(s => s.Enrollments).FirstOrDefault(s => s.Id == studentId) ?? throw new ArgumentException("Student not found.");
            if (course.Enrolled.Any(s => s.Id == studentId) || student.Enrollments.Any(c => c.Id == courseId)) throw new InvalidOperationException($"{student.FullName} is already enrolled in {course.Code}.");
            course.Enrolled.Add(student);
            student.Enrollments.Add(course);
            _db.SaveChanges();
            _notifications.AddNotification($"You enrolled in {course.Code} - {course.Title}.", NotificationType.Enrollment, student.Id);
        }

        public void DropCourse(Guid courseId, Guid studentId)
        {
            var course = GetById(courseId) ?? throw new ArgumentException("Course not found.");
            if (!course.IsActive) throw new InvalidOperationException($"Course {course.Code} is inactive and cannot be dropped.");
            var student = _db.Students.Include(s => s.Enrollments).FirstOrDefault(s => s.Id == studentId) ?? throw new ArgumentException("Student not found.");
            var enrolledStudent = course.Enrolled.FirstOrDefault(s => s.Id == studentId);
            if (enrolledStudent == null) throw new InvalidOperationException($"{student.FullName} is not enrolled in {course.Code}.");
            course.Enrolled.Remove(enrolledStudent);
            student.Enrollments.RemoveAll(c => c.Id == courseId);
            _db.SaveChanges();
        }

        public List<Course> GetForFaculty(Guid facultyId) => _db.Courses.Where(c => c.AssignedFacultyId == facultyId).Include(c => c.Enrolled).ToList();
        public List<Course> GetAvailable() => _db.Courses.Where(c => c.IsActive && c.EnrollmentStatus != EnrollmentStatus.Full).Include(c => c.Enrolled).ToList();

        private static void Validate(Course entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Code)) throw new ArgumentException("Course code is required.");
            if (string.IsNullOrWhiteSpace(entity.Title)) throw new ArgumentException("Course title is required.");
            if (entity.CreditHours <= 0) throw new ArgumentException("Credit hours must be positive.");
            if (entity.MaxCapacity <= 0) throw new ArgumentException("Max capacity must be positive.");
        }

        private void SyncFacultyAssignment(Course course)
        {
            var faculty = _db.Faculty.Include(f => f.AssignedCourses).FirstOrDefault(f => f.Id == course.AssignedFacultyId);
            if (faculty != null && !faculty.AssignedCourses.Any(c => c.Id == course.Id)) faculty.AssignedCourses.Add(course);
        }
    }
}
