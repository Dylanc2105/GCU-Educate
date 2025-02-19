using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Module
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }

        // Foreign keys
        public int CourseId { get; set; }
        public string LecturerId { get; set; }

        // Navigation properties
        public virtual Course Course { get; set; }
        public virtual Lecturer Lecturer { get; set; }
        public virtual ICollection<Student> Students { get; set; } 

    }
}