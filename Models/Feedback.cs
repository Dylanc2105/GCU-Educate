using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuidanceTracker.Models
{
	public class Feedback
	{
		[Key]
        public string FeedbackId { get; set; }

        public DateTime DateOfPosting { get; set; }
        public DateTime SubmissionDate { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

    }
}