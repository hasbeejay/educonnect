using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Exceptions;
using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Manages course catalog and enrollment rules. DIP: Receives NotificationService through DI instead of constructing dependencies.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly NotificationService _notifications;
        public CourseService(NotificationService notifications)
        {
            _notifications = notifications;
        }

        public List<Course> GetAll() => SeedData.Courses.Select(c => c).ToList();
        public Course? GetById(Guid id) => SeedData.Courses.FirstOrDefault(c => c.Id == id);

        public void Add(Course entity)
        {
            Validate(entity);
            if (SeedData.Courses.Any(c => c.Code.Equals(entity.Code, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("A course with this code already exists.");
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            SeedData.Courses.Add(entity);
            SyncFacultyAssignment(entity);
        }

        public void Update(Course entity)
        {
            Validate(entity);
            var existing = GetById(entity.Id) ?? throw new ArgumentException("Course not found.");
            if (SeedData.Courses.Any(c => c.Id != entity.Id && c.Code.Equals(entity.Code, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("A course with this code already exists.");
            if (entity.MaxCapacity < existing.Enrolled.Count) throw new ArgumentException($"Cannot reduce capacity below current enrollment count ({existing.Enrolled.Count}).");
            foreach (var faculty in SeedData.Faculty) faculty.AssignedCourses.RemoveAll(c => c.Id == existing.Id);
            existing.Code = entity.Code;
            existing.Title = entity.Title;
            existing.CreditHours = entity.CreditHours;
            existing.MaxCapacity = entity.MaxCapacity;
            existing.AssignedFacultyId = entity.AssignedFacultyId;
            existing.IsActive = entity.IsActive;
            SyncFacultyAssignment(existing);
        }

        public void Delete(Guid id)
        {
            var course = GetById(id);
            if (course == null) return;
            if (course.Enrolled.Any()) throw new InvalidOperationException($"Course {course.Code} has enrolled students and cannot be deleted.");
            SeedData.Courses.Remove(course);
            foreach (var faculty in SeedData.Faculty) faculty.AssignedCourses.RemoveAll(c => c.Id == id);
        }

        public void EnrollStudent(Guid courseId, Guid studentId)
        {
            var course = GetById(courseId) ?? throw new ArgumentException("Course not found.");
            if (!course.IsActive) throw new InvalidOperationException($"Course {course.Code} is inactive.");
            if (course.EnrollmentStatus == EnrollmentStatus.Full) throw new CourseFullException($"Course {course.Code} is full.");
            var student = SeedData.Students.FirstOrDefault(s => s.Id == studentId) ?? throw new ArgumentException("Student not found.");
            if (course.Enrolled.Any(s => s.Id == studentId) || student.Enrollments.Any(c => c.Id == courseId)) throw new InvalidOperationException($"{student.FullName} is already enrolled in {course.Code}.");
            course.Enrolled.Add(student);
            student.Enrollments.Add(course);
            _notifications.AddNotification($"You enrolled in {course.Code} - {course.Title}.", NotificationType.Enrollment, student.Id);
        }

        public void DropCourse(Guid courseId, Guid studentId)
        {
            var course = GetById(courseId) ?? throw new ArgumentException("Course not found.");
            if (!course.IsActive) throw new InvalidOperationException($"Course {course.Code} is inactive and cannot be dropped.");
            var student = SeedData.Students.FirstOrDefault(s => s.Id == studentId) ?? throw new ArgumentException("Student not found.");
            var enrolledStudent = course.Enrolled.FirstOrDefault(s => s.Id == studentId);
            if (enrolledStudent == null) throw new InvalidOperationException($"{student.FullName} is not enrolled in {course.Code}.");
            course.Enrolled.Remove(enrolledStudent);
            student.Enrollments.RemoveAll(c => c.Id == courseId);
        }

        public List<Course> GetForFaculty(Guid facultyId) => SeedData.Courses.Where(c => c.AssignedFacultyId == facultyId).ToList();
        public List<Course> GetAvailable() => SeedData.Courses.Where(c => c.IsActive && c.EnrollmentStatus != EnrollmentStatus.Full).ToList();

        private static void Validate(Course entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Code)) throw new ArgumentException("Course code is required.");
            if (string.IsNullOrWhiteSpace(entity.Title)) throw new ArgumentException("Course title is required.");
            if (entity.CreditHours <= 0) throw new ArgumentException("Credit hours must be positive.");
            if (entity.MaxCapacity <= 0) throw new ArgumentException("Max capacity must be positive.");
        }

        private void SyncFacultyAssignment(Course course)
        {
            var faculty = SeedData.Faculty.FirstOrDefault(f => f.Id == course.AssignedFacultyId);
            if (faculty != null && !faculty.AssignedCourses.Any(c => c.Id == course.Id)) faculty.AssignedCourses.Add(course);
        }
    }
}
