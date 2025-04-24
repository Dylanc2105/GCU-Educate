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

        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Appointment Status")]
        public AppointmentStatus AppointmentStatus { get; set; }
        [Display(Name = "Appointment Notes")]
        public string AppointmentNotes { get; set; }

        public string Room { get; set; }



        // Navigation properties
        [Required]
        public string GuidanceTeacherId { get; set; }
        [ForeignKey("GuidanceTeacherId")]
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [Required]
        public int GuidanceSessionId { get; set; }
        [ForeignKey("GuidanceSessionId")]
        public virtual GuidanceSession GuidanceSession { get; set; }


        public int? IssueId { get; set; }
        [ForeignKey("IssueId")]
        public virtual Issue Issue { get; set; }



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