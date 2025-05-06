using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class AppointmentFeedbackForm
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Student's Opinion")]
        public string StudentOpinion { get; set; }
        [Display(Name = "Actions To Be Taken By Student")]
        public string StudentActions { get; set; }
        [Display(Name = "Actions To Be Taken By Others")]
        public string OtherActions { get; set; }
        [Display(Name = "Guidance Teacher Notes")]
        public string GuidanceTeacherNotes { get; set; }


        //nav props
        public string GuidanceTeacherId { get; set; }
        [ForeignKey("GuidanceTeacherId")]
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }

        [Required]
        public int IssueId { get; set; }
        [ForeignKey("IssueId")]
        public virtual Issue Issue { get; set; }

        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        [Required]
        public int AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }
    }
}