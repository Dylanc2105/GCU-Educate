using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class ProgressMeetingFeedbackForm
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "General course rate")]
        public CourseRating CourseFeedback { get; set; }
        [Display(Name = "General feedback comments about course")]
        public string CourseFeedbackComments { get; set; }
        [Display(Name = "Areas of strength")]
        public string Strengths { get; set; }
        [Display(Name = "Areas needing improvement")]
        public string ToBeImproved { get; set; }
        [Display(Name = "Student goals")]
        public string Goals { get; set; }
        [Display(Name = "Guidance Teacher Notes")]
        public string GuidanceTeacherNotes { get; set; }


        //nav props
        public string GuidanceTeacherId { get; set; }
        [ForeignKey("GuidanceTeacherId")]
        public virtual GuidanceTeacher GuidanceTeacher { get; set; }
        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        [Required]
        public int AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }


        public enum CourseRating
        {
            Excellent,
            Good,
            Satisfactory,
            Bad
        }
    }
}