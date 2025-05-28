using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels.FeedbackViewModels
{
    public class CreateDetailedFeedbackViewModel
    {
        public string RequestId { get; set; }
        public string FeedbackId { get; set; }

        public string ClassRepId { get; set; }

        // Basic information
        [Display(Name = "Course")]
        public string Course { get; set; }

        [Display(Name = "Class")]
        public string Class { get; set; }

        [Display(Name = "Feedback Date")]
        [DisplayFormat(DataFormatString = "{0:MMMM d, yyyy}")]
        public DateTime FeedbackDate { get; set; }

        #region Current Learning Experience
        [Required]
        [Display(Name = "Is the course currently meeting your expectations?")]
        public bool? MeetsExpectations { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string MeetsExpectationsNotes { get; set; }

        [Required]
        [Display(Name = "Would you recommend this course to others at this stage?")]
        public bool? WouldRecommend { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string WouldRecommendNotes { get; set; }

        [Required]
        [Display(Name = "Is the current workload manageable?")]
        public bool? WorkloadManageable { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string WorkloadManageableNotes { get; set; }

        [Required]
        [Display(Name = "Key Issues with Learning Experience:")]
        [DataType(DataType.MultilineText)]
        public string LearningExperienceKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Learning Experience:")]
        [DataType(DataType.MultilineText)]
        public string LearningExperienceStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Learning Experience:")]
        [DataType(DataType.MultilineText)]
        public string LearningExperienceImprovements { get; set; }

        [Display(Name = "Additional Comments on Learning Experience:")]
        [DataType(DataType.MultilineText)]
        public string LearningExperienceComments { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Current Learning Experience Rating:")]
        public int LearningExperienceRating { get; set; }
        #endregion

        #region Teaching & Learning Methods
        [Required]
        [Display(Name = "Are concepts being presented in a clear and organised manner?")]
        public bool? ConceptsPresented { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string ConceptsPresentedNotes { get; set; }

        [Required]
        [Display(Name = "Are learning materials being made available?")]
        public bool? MaterialsAvailable { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string MaterialsAvailableNotes { get; set; }

        [Required]
        [Display(Name = "Is the teaching approach accommodating different learning styles in the class?")]
        public bool? AccommodatesStyles { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string AccommodatesStylesNotes { get; set; }

        [Required]
        [Display(Name = "Is the lecturer responsive to questions and clarification requests?")]
        public bool? LecturerResponsive { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string LecturerResponsiveNotes { get; set; }

        [Required]
        [Display(Name = "Key Issues with Learning & Teaching:")]
        [DataType(DataType.MultilineText)]
        public string LearningTeachingKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Learning & Teaching:")]
        [DataType(DataType.MultilineText)]
        public string LearningTeachingStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Learning & Teaching:")]
        [DataType(DataType.MultilineText)]
        public string LearningTeachingImprovements { get; set; }

        [Display(Name = "Additional Comments on Learning & Teaching:")]
        [DataType(DataType.MultilineText)]
        public string LearningTeachingComments { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Learning & Teaching Rating:")]
        public int LearningTeachingRating { get; set; }
        #endregion

        #region Assessment & Progress Tracking
        [Required]
        [Display(Name = "Are you confident about upcoming assessments?")]
        public bool? AssessmentConfidence { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string AssessmentConfidenceNotes { get; set; }

        [Required]
        [Display(Name = "Is feedback being returned promptly?")]
        public bool? TimelyFeedback { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string TimelyFeedbackNotes { get; set; }

        [Required]
        [Display(Name = "Is the feedback specific enough to help you improve?")]
        public bool? SpecificFeedback { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string SpecificFeedbackNotes { get; set; }

        [Required]
        [Display(Name = "Do the assessments align with what's being taught in class?")]
        public bool? AssessmentsAligned { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string AssessmentsAlignedNotes { get; set; }

        [Required]
        [Display(Name = "Is sufficient time being provided to complete assigned work?")]
        public bool? SufficientTime { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string SufficientTimeNotes { get; set; }

        [Required]
        [Display(Name = "Key Issues with Assessment & Progress:")]
        [DataType(DataType.MultilineText)]
        public string AssessmentKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Assessment & Progress:")]
        [DataType(DataType.MultilineText)]
        public string AssessmentStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Assessment & Progress:")]
        [DataType(DataType.MultilineText)]
        public string AssessmentImprovements { get; set; }

        [Display(Name = "Additional Comments on Assessment & Progress:")]
        [DataType(DataType.MultilineText)]
        public string AssessmentComments { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Assessment & Progress Rating:")]
        public int AssessmentRating { get; set; }
        #endregion

        #region Learning Resources & Environment
        [Required]
        [Display(Name = "Are the required materials easily accessible?")]
        public bool? MaterialsAccessible { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string MaterialsAccessibleNotes { get; set; }

        [Required]
        [Display(Name = "Is the Canvas organised in a helpful way?")]
        public bool? PlatformOrganised { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string PlatformOrganisedNotes { get; set; }

        [Required]
        [Display(Name = "Is the necessary equipment available and working properly?")]
        public bool? EquipmentWorking { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string EquipmentWorkingNotes { get; set; }

        [Required]
        [Display(Name = "Are additional resources being provided for topics you find challenging?")]
        public bool? SupplementaryResources { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string SupplementaryResourcesNotes { get; set; }

        [Required]
        [Display(Name = "Do you have sufficient access to specialised equipment needed for current units?")]
        public bool? SpecializedEquipment { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string SpecializedEquipmentNotes { get; set; }

        [Required]
        [Display(Name = "Are library resources adequate?")]
        public bool? LibraryResources { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string LibraryResourcesNotes { get; set; }

        [Required]
        [Display(Name = "Key Issues with Resources & Learning Environment:")]
        [DataType(DataType.MultilineText)]
        public string ResourcesKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Resources & Learning Environment:")]
        [DataType(DataType.MultilineText)]
        public string ResourcesStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Resources & Learning Environment:")]
        [DataType(DataType.MultilineText)]
        public string ResourcesImprovements { get; set; }

        [Display(Name = "Additional Comments on Resources & Learning Environment:")]
        [DataType(DataType.MultilineText)]
        public string ResourcesComments { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Resources & Learning Environment Rating:")]
        public int ResourcesRating { get; set; }
        #endregion

        #region Communication & Support
        [Required]
        [Display(Name = "Are staff responding to queries in a timely manner?")]
        public bool? StaffResponsive { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string StaffResponsiveNotes { get; set; }

        [Required]
        [Display(Name = "Is additional help available when needed?")]
        public bool? AdditionalHelpAvailable { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string AdditionalHelpAvailableNotes { get; set; }

        [Required]
        [Display(Name = "Are accommodations being provided for students who require them?")]
        public bool? AccommodationsProvided { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string AccommodationsProvidedNotes { get; set; }

        [Required]
        [Display(Name = "Is it clear who to approach with specific questions or concerns?")]
        public bool? ClearPointsOfContact { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string ClearPointsOfContactNotes { get; set; }

        [Required]
        [Display(Name = "Key Issues with Support Effectiveness:")]
        [DataType(DataType.MultilineText)]
        public string SupportEffectivenesssKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Support Effectiveness:")]
        [DataType(DataType.MultilineText)]
        public string SupportEffectivenessStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Support Effectiveness:")]
        [DataType(DataType.MultilineText)]
        public string SupportEffectivenessImprovements { get; set; }

        [Display(Name = "Additional Comments on Support Effectiveness:")]
        [DataType(DataType.MultilineText)]
        public string SupportEffectivenessComments { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Current support effectiveness:")]
        public int SupportEffectivenessRating { get; set; }
        #endregion

        #region Skills Development
        [Required]
        [Display(Name = "Are you developing critical thinking skills in this course?")]
        public bool? DevelopingCriticalThinking { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string DevelopingCriticalThinkingNotes { get; set; }

        [Required]
        [Display(Name = "Are you enhancing your problem-solving abilities through the coursework?")]
        public bool? EnhancingProblemSolving { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string EnhancingProblemSolvingNotes { get; set; }

        [Required]
        [Display(Name = "Are you gaining practical skills relevant to your intended career?")]
        public bool? GainingPracticalSkills { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string GainingPracticalSkillsNotes { get; set; }

        [Required]
        [Display(Name = "Is the course helping improve your communication abilities?")]
        public bool? ImprovingCommunication { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string ImprovingCommunicationNotes { get; set; }

        [Required]
        [Display(Name = "Are your research skills developing through the current assignments?")]
        public bool? DevelopingResearchSkills { get; set; }

        [Display(Name = "If no, please explain why:")]
        [DataType(DataType.MultilineText)]
        public string DevelopingResearchSkillsNotes { get; set; }

        [Required]
        [Display(Name = "Key Issues with Skills Development:")]
        [DataType(DataType.MultilineText)]
        public string SkillsDevelopmentKeyIssues { get; set; }

        [Required]
        [Display(Name = "Strengths in Skills Development:")]
        [DataType(DataType.MultilineText)]
        public string SkillsDevelopmentStrengths { get; set; }

        [Required]
        [Display(Name = "Suggested Improvements for Skills Development:")]
        [DataType(DataType.MultilineText)]
        public string SkillsDevelopmentImprovements { get; set; }

        [Display(Name = "Additional Comments on Skills Development:")]
        [DataType(DataType.MultilineText)]
        public string SkillsDevelopmentComments { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Current skills development progress:")]
        public int SkillsDevelopmentRating { get; set; }
        #endregion

        #region Overall Comments
        [Required]
        [Display(Name = "What are the best features of this course/unit?")]
        [DataType(DataType.MultilineText)]
        public string BestFeatures { get; set; }

        [Required]
        [Display(Name = "What areas could be improved?")]
        [DataType(DataType.MultilineText)]
        public string AreasForImprovement { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Overall Rating:")]
        public int OverallRating { get; set; }
        #endregion

        public bool IsSubmitted { get; set; }
        public int ClassId { get; set; }
        public string StudentId { get; set; }
        public string CreatorId { get; set; }
    }
}