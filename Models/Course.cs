using System;
using System.Collections.Generic;
using System.Linq;

namespace EduConnect.Models
{
    public class Course
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public int CreditHours { get; set; } = 3;
        public int MaxCapacity { get; set; } = 30;
        public bool IsActive { get; set; } = true;
        public Guid AssignedFacultyId { get; set; }

        public List<Student> Enrolled { get; set; } = new();

        public EnrollmentStatus EnrollmentStatus
        {
            get
            {
                if (Enrolled.Count >= MaxCapacity) return EnrollmentStatus.Full;
                double percentage = (double)Enrolled.Count / MaxCapacity;
                return percentage >= 0.7 ? EnrollmentStatus.AlmostFull : EnrollmentStatus.Open;
            }
        }
    }
}