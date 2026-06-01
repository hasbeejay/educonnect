using System;
using System.Linq;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Maintains the current authenticated user state and broadcasts auth changes to the UI.
    /// </summary>
    public class AuthStateService
    {
        public AuthState State { get; private set; } = new();
        public event Action? OnAuthChanged;

        public AuthStateService() { }

        public bool Login(string email, string password)
        {
            var em = email.Trim();
            var admin = SeedData.Users.OfType<Admin>().FirstOrDefault(a => a.Email == em && a.PasswordHash == password);
            if (admin != null)
            {
                State.CurrentUser = admin;
                OnAuthChanged?.Invoke();
                return true;
            }
            var faculty = SeedData.Users.OfType<Faculty>().FirstOrDefault(f => f.Email == em && f.PasswordHash == password);
            if (faculty != null)
            {
                State.CurrentUser = faculty;
                OnAuthChanged?.Invoke();
                return true;
            }
            var student = SeedData.Users.OfType<Student>().FirstOrDefault(s => s.Email == em && s.PasswordHash == password);
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
