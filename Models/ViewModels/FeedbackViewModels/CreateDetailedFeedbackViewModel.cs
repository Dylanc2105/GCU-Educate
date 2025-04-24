using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels.FeedbackViewModels
{
    public class CreateDetailedFeedbackViewModel
    {
        public int RequestId { get; set; }
        public int FeedbackId { get; set; } 

        // Basic information
        [Display(Name = "Course")]
        public string Course { get; set; }

        [Display(Name = "Class")]
        public string Class { get; set; }

        [Display(Name = "Feedback Date")]
        [DisplayFormat(DataFormatString = "{0:MMMM d, yyyy}")]
        public DateTime FeedbackDate { get; set; }

        // Overall comments section
        [Required]
        [Display(Name = "What are the best features of this course/unit?")]
        [DataType(DataType.MultilineText)]
        public string BestFeatures { get; set; }

        [Required]
        [Display(Name = "What areas could be improved?")]
        [DataType(DataType.MultilineText)]
        public string AreasForImprovement { get; set; }

        // Learning & Teaching Approaches section
        [Required]
        [Display(Name = "Key Issues with Learning & Teaching")]
        [DataType(DataType.MultilineText)]
        public string LearningTeachingKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Learning & Teaching")]
        [DataType(DataType.MultilineText)]
        public string LearningTeachingStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Learning & Teaching")]
        [DataType(DataType.MultilineText)]
        public string LearningTeachingImprovements { get; set; }

        [Display(Name = "Additional Comments on Learning & Teaching")]
        [DataType(DataType.MultilineText)]
        public string LearningTeachingComments { get; set; }

        // Student Assessment & Progress section
        [Required]
        [Display(Name = "Key Issues with Assessment & Progress")]
        [DataType(DataType.MultilineText)]
        public string AssessmentKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Assessment & Progress")]
        [DataType(DataType.MultilineText)]
        public string AssessmentStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Assessment & Progress")]
        [DataType(DataType.MultilineText)]
        public string AssessmentImprovements { get; set; }

        [Display(Name = "Additional Comments on Assessment & Progress")]
        [DataType(DataType.MultilineText)]
        public string AssessmentComments { get; set; }

        // Resources & Learning Environment section
        [Required]
        [Display(Name = "Key Issues with Resources & Learning Environment")]
        [DataType(DataType.MultilineText)]
        public string ResourcesKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Resources & Learning Environment")]
        [DataType(DataType.MultilineText)]
        public string ResourcesStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Resources & Learning Environment")]
        [DataType(DataType.MultilineText)]
        public string ResourcesImprovements { get; set; }

        [Display(Name = "Additional Comments on Resources & Learning Environment")]
        [DataType(DataType.MultilineText)]
        public string ResourcesComments { get; set; }

        public bool IsSubmitted { get; set; }
        public int ClassId { get; set; }
        public string StudentId { get; set; }
        public string CreatorId { get; set; }
        //public string GuidanceTeacherId { get; set; }
        //public string CurriculumHeadId { get; set; }
    }
}