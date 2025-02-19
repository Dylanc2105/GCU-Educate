using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace GuidanceTracker.Models
{
    public class Student:User
    {
        // Foreign key to GuidanceTeacher
        public string GuidanceTeacherId { get; set; }
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        // Foreign key to Course
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        // Navigation properties
        public virtual ICollection<Session> Sessions { get; set; } 
        public virtual ICollection<Ticket> Tickets { get; set; } 
    }
}