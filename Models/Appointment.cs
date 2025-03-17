using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentStatus { get; set; }/* = Requested;*/
        public string AppointmentNotes { get; set; }

        [Required]
        public string GuidanceTeacherId { get; set; }
        public DateTime Time { get; set; }
        public string Room { get; set; }
        [Display(Name = "Appointment Comment")]
        public string AppointmentComment { get; set; }

        [ForeignKey("GuidanceTeacherId")]
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        [Required]
        public string StudentId { get; set; }
        //nav props
        public int TicketId { get; set; }

        // Navigation properties
        public virtual Ticket Ticket { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }

    public enum AppointmentStatus
    {
        Requested,
        Scheduled,
        Completed,
        Cancelled,
        Rescheduled
    }
}