using System;

namespace EduConnect.Models
{
    public abstract class Person
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public abstract string GetRole();
    }
}