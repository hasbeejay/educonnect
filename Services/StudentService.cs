using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Exceptions;
using EduConnect.Interfaces;
using EduConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Student> GetAll() => _context.Students.Include(s => s.Enrollments).ToList();
        
        public Student? GetById(Guid id) => _context.Students.Include(s => s.Enrollments).FirstOrDefault(s => s.Id == id);

        public void Add(Student entity)
        {
            if (!entity.Validate(out var errorMessage)) throw new ArgumentException(errorMessage);
            if (string.IsNullOrWhiteSpace(entity.PasswordHash)) throw new ArgumentException("Password is required.");
            if (_context.Students.Any(s => s.Email.ToLower() == entity.Email.ToLower())) throw new ArgumentException("A student with this email already exists.");
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            
            _context.Students.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Student entity)
        {
            if (!entity.Validate(out var errorMessage)) throw new ArgumentException(errorMessage);
            if (string.IsNullOrWhiteSpace(entity.PasswordHash)) throw new ArgumentException("Password is required.");
            var existing = _context.Students.FirstOrDefault(s => s.Id == entity.Id) ?? throw new ArgumentException("Student not found.");
            if (_context.Students.Any(s => s.Id != entity.Id && s.Email.ToLower() == entity.Email.ToLower())) throw new ArgumentException("A student with this email already exists.");
            
            existing.FullName = entity.FullName;
            existing.Email = entity.Email;
            existing.PasswordHash = entity.PasswordHash;
            existing.Semester = entity.Semester;
            existing.CGPA = entity.CGPA;
            
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var student = _context.Students.Include(s => s.Enrollments).FirstOrDefault(s => s.Id == id);
            if (student == null) return;
            if (student.Enrollments.Any()) throw new StudentHasActiveEnrollmentsException($"Student {student.FullName} has active enrollments and cannot be deleted.");
            
            _context.Students.Remove(student);
            _context.SaveChanges();
        }

        public List<Student> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return GetAll();
            term = term.Trim().ToLowerInvariant();
            return _context.Students
                .Include(s => s.Enrollments)
                .Where(s => s.FullName.ToLower().Contains(term) || s.Email.ToLower().Contains(term) || s.Id.ToString().Contains(term))
                .ToList();
        }
    }
}
