using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }

        [Required]
        [Display(Name = "Course Reference")]
        public string CourseReference { get; set; }

        [Required]
        [Display(Name = "Mode of Study")]
        public string ModeOfStudy { get; set; }

        [Display(Name = "Duration in Weeks")]
        public int DurationInWeeks { get; set; }

        [Display(Name = "SCQF Level")]
        public int SCQFLevel { get; set; }
        public string Site { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Navigation properties

        [Required]
        public string DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }
}