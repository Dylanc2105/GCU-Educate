using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class DetailedFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        // Basic information
        [Required]
        public string Course { get; set; }

        [Required]
        public string Class { get; set; }

        // The creator (guidance teacher or curriculum head)
        public string CreatorId { get; set; }

        // The student submitting the feedback
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        // Overall comments section
        public string BestFeatures { get; set; }
        public string AreasForImprovement { get; set; }

        // Learning & Teaching Approaches section
        public string LearningTeachingKeyIssues { get; set; }
        public string LearningTeachingStrengths { get; set; }
        public string LearningTeachingImprovements { get; set; }
        public string LearningTeachingComments { get; set; }

        // Student Assessment & Progress section
        public string AssessmentKeyIssues { get; set; }
        public string AssessmentStrengths { get; set; }
        public string AssessmentImprovements { get; set; }
        public string AssessmentComments { get; set; }

        // Resources & Learning Environment section
        public string ResourcesKeyIssues { get; set; }
        public string ResourcesStrengths { get; set; }
        public string ResourcesImprovements { get; set; }
        public string ResourcesComments { get; set; }

        // Status properties
        public DateTime DateCreated { get; set; }
        public bool IsSubmitted { get; set; }

        // The class this feedback is for
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class TargetClass { get; set; }

        public bool IsReadByGuidanceTeacher { get; set; }

        public bool IsReadByCurriculumHead { get; set; }

        //public string GuidanceTeacherId { get; set; }

        //[ForeignKey("GuidanceTeacherId")]
        //public GuidanceTeacher GuidanceTeacher { get; set; }

        //public string CurriculumHeadId { get; set; }

        //[ForeignKey("CurriculumHeadId")]
        //public CurriculumHead CurriculumHead { get; set; }

        public DetailedFeedback()
        {
            DateCreated = DateTime.Now;
            IsSubmitted = false;
        }
    }
}