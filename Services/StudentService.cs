using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Exceptions;
using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Manages student records only. ISP: Depends on IStudentService without grade operations. DIP: Pages consume the interface abstraction.
    /// </summary>
    public class StudentService : IStudentService
    {
        public StudentService() { }

        public List<Student> GetAll() => SeedData.Students.Select(s => s).ToList();
        public Student? GetById(Guid id) => SeedData.Students.FirstOrDefault(s => s.Id == id);

        public void Add(Student entity)
        {
            if (!entity.Validate(out var errorMessage)) throw new ArgumentException(errorMessage);
            if (string.IsNullOrWhiteSpace(entity.PasswordHash)) throw new ArgumentException("Password is required.");
            if (SeedData.Students.Any(s => s.Email.Equals(entity.Email, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("A student with this email already exists.");
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            SeedData.Students.Add(entity);
        }

        public void Update(Student entity)
        {
            if (!entity.Validate(out var errorMessage)) throw new ArgumentException(errorMessage);
            if (string.IsNullOrWhiteSpace(entity.PasswordHash)) throw new ArgumentException("Password is required.");
            var existing = GetById(entity.Id) ?? throw new ArgumentException("Student not found.");
            if (SeedData.Students.Any(s => s.Id != entity.Id && s.Email.Equals(entity.Email, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("A student with this email already exists.");
            existing.FullName = entity.FullName;
            existing.Email = entity.Email;
            existing.PasswordHash = entity.PasswordHash;
            existing.Semester = entity.Semester;
            existing.CGPA = entity.CGPA;
        }

        public void Delete(Guid id)
        {
            var student = GetById(id);
            if (student == null) return;
            if (student.Enrollments.Any()) throw new StudentHasActiveEnrollmentsException($"Student {student.FullName} has active enrollments and cannot be deleted.");
            SeedData.Students.Remove(student);
        }

        public List<Student> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return GetAll();
            term = term.Trim().ToLowerInvariant();
            return SeedData.Students
                .Where(s => s.FullName.ToLower().Contains(term) || s.Email.ToLower().Contains(term) || s.Id.ToString().Contains(term))
                .ToList();
        }
    }
}
