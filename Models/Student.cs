using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace GuidanceTracker.Models
{
    public class Student : User
    {
        public string StudentNumber { get; set; }
        // Foreign key to GuidanceTeacher
        public string GuidanceTeacherId { get; set; }
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }

        public string FeedbackId { get; set; }

        [ForeignKey("FeedbackId")]

        public virtual Feedback Feedback { get; set; }

        public ICollection<MessageBoard> MessageBoards { get; set; }
        public ICollection<Appointment> Appointments { get; set; }

    }
}