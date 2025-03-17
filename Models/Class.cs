using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;

namespace GuidanceTracker.Models
{
	public class Class
	{
        [Key]
        public int ClassId { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; }

        public int MaxCapacity { get; set; }

        // Navigation properties
        public string GuidanceTeacherId { get; set; }

        [ForeignKey("GuidanceTeacherId")]
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }
        public virtual ICollection<Timetable> Timetables { get; set; }

        public virtual ICollection<Unit> Units { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}