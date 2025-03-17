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

        public string TicketDescription { get; set; }
        public string TicketStatus { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        // Foreign keys

        [Required]
        public string LecturerId { get; set; }

        [ForeignKey("LecturerId")]
        public virtual Lecturer Lecturer { get; set; }

        [Required]
        public string GuidanceTeacherId { get; set; }

        [ForeignKey("GuidanceTeacherId")]
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        // Optional reference to an archived ticket if this was restored
        public int? ArchivedTicketId { get; set; }

        // 🔹 Adding a list of comments
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();


    }
}
