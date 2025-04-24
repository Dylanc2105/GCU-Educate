
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Models
{
    public class SimpleStudentFeedbackViewModel
    {
        [Required]
        [Display(Name = "Feedback Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Feedback Content")]
        public string content { get; set; }

        // Checkboxes for recipients
        [Display(Name = "Send to Guidance Teacher")]
        public bool SendToGuidanceTeacher { get; set; }

        [Display(Name = "Send to Curriculum Head")]
        public bool SendToCurriculumHead { get; set; }

        // For Unit selection (optional)
        [Display(Name = "Specific Unit (Optional)")]
        public int? UnitId { get; set; }
        public IEnumerable<SelectListItem> Units { get; set; }
        public string GuidanceTeacherId { get; set; }
        public string GuidanceTeacherName { get; set; }

        public string CurriculumHeadId { get; set; }
        public string CurriculumHeadName { get; set; }

        public SimpleStudentFeedbackViewModel()
        {
            Units = new List<SelectListItem>();
        }
    }
}