using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuidanceTracker.Models
{
	public class CurriculumHead:User
	{
        public string DepartmentId { get; set; }

        // Navigation property
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        // Navigation properties
        public virtual ICollection<SimpleFeedback> Feedbacks { get; set; }
    }
}