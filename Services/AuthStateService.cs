using System;
using System.Linq;
using EduConnect.Models;

namespace EduConnect.Services
{
    public class AuthStateService
    {
        private readonly ApplicationDbContext _context;
        public AuthState State { get; private set; } = new();
        public event Action? OnAuthChanged;

        public AuthStateService(ApplicationDbContext context) 
        { 
            _context = context;
        }

        public bool Login(string email, string password)
        {
            var em = email.Trim().ToLower();
            var admin = _context.Admins.FirstOrDefault(a => a.Email.ToLower() == em && a.PasswordHash == password);
            if (admin != null)
            {
                State.CurrentUser = admin;
                OnAuthChanged?.Invoke();
                return true;
            }
            var faculty = _context.Faculty.FirstOrDefault(f => f.Email.ToLower() == em && f.PasswordHash == password);
            if (faculty != null)
            {
                State.CurrentUser = faculty;
                OnAuthChanged?.Invoke();
                return true;
            }
            var student = _context.Students.FirstOrDefault(s => s.Email.ToLower() == em && s.PasswordHash == password);
            if (student != null)
            {
                State.CurrentUser = student;
                OnAuthChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void Logout()
        {
            State.CurrentUser = null;
            OnAuthChanged?.Invoke();
        }
    }
}
