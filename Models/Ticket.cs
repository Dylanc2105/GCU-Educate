using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string TicketTitle { get; set; }
        public string TicketDescription { get; set; }
        public string TicketStatus { get; set; } // Open, In Progress, Resolved
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Foreign keys
        public string LecturerId { get; set; }
        public string StudentId { get; set; }
        public string GuidanceTeacherId { get; set; }

        // Navigation properties
        public virtual Lecturer Lecturer { get; set; }
        public virtual Student Student { get; set; }
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        // Comments list
        public virtual List<Comment> Comments { get; set; } = new List<Comment>();
    }

}