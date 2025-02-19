using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
	public class Session
	{
        public int SessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionStatus { get; set; } // Pending, Approved, Rejected
        public string SessionNotes { get; set; }

        // Foreign keys
        public string StudentId { get; set; }
        public string GuidanceTeacherId { get; set; }

        // Navigation properties
        public virtual Student Student { get; set; }
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }
    }
}