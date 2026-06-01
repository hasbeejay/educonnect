using System.Collections.Generic;

namespace EduConnect.Models
{
    public class Faculty : Person
    {
        public List<Course> AssignedCourses { get; set; } = new();

        public override string GetRole() => "Faculty";
    }
}