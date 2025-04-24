using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
	public class Department
	{
        [Key]
        public string DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        public string CurriculumHeadId { get; set; }
        public virtual CurriculumHead CurriculumHead { get; set; }

        // Navigation properties
        public virtual ICollection<Course> Courses { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }
}