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
        public AppointmentStatus AppointmentStatus { get; set; }/* = Requested;*/
        [Display(Name = "Appointment Notes")]
        public string AppointmentNotes { get; set; }

        [Required]
        public string GuidanceTeacherId { get; set; }
        public string Room { get; set; }
        

        [ForeignKey("GuidanceTeacherId")]
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        [Required]
        public string StudentId { get; set; }
        

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