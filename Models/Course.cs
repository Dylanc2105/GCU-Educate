using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }

        // Navigation properties
        public virtual ICollection<Module> Modules { get; set; } 
        public virtual ICollection<Student> Students { get; set; } 
    }
}