using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuidanceTracker.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        public string TicketTitle { get; set; }

        [Required]
        public string TicketDescription { get; set; }

        public string TicketStatus { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        // Foreign keys
        public string LecturerId { get; set; }
        public string StudentId { get; set; }
        public string GuidanceTeacherId { get; set; }

        // Navigation properties
        public virtual Lecturer Lecturer { get; set; }
        public virtual Student Student { get; set; }
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        // 🔹 Adding a list of comments & appointments
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();


    }
}
