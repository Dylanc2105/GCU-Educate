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

        //Current Learning Experience secyion
        public bool? MeetsExpectations { get; set; }
        public string MeetsExpectationsNotes { get; set; }
        public bool? WouldRecommend { get; set; }
        public string WouldRecommendNotes { get; set; }
        public bool? WorkloadManageable { get; set; }
        public string WorkloadManageableNotes { get; set; }
        public string LearningExperienceKeyIssues { get; set; }
        public string LearningExperienceStrengths { get; set; }
        public string LearningExperienceImprovements { get; set; }
        public string LearningExperienceComments { get; set; }
        public int LearningExperienceRating { get; set; }

        //End of Current Learning Experience section



        // Teaching & Learning Methods section
        public bool? ConceptsPresented { get; set; }
        public string ConceptsPresentedNotes { get; set; }
        public bool? MaterialsAvailable { get; set; }
        public string MaterialsAvailableNotes { get; set; }
        public bool? AccommodatesStyles { get; set; }
        public string AccommodatesStylesNotes { get; set; }
        public bool? LecturerResponsive { get; set; }
        public string LecturerResponsiveNotes { get; set; }
        public string LearningTeachingKeyIssues { get; set; }
        public string LearningTeachingStrengths { get; set; }
        public string LearningTeachingImprovements { get; set; }
        public string LearningTeachingComments { get; set; }
        public int LearningTeachingRating { get; set; }

        //End of Teaching & Learning Methods section


        //Assessment & Progress Tracking
        public bool? AssessmentConfidence { get; set; }
        public string AssessmentConfidenceNotes { get; set; }
        public bool? TimelyFeedback { get; set; }
        public string TimelyFeedbackNotes { get; set; }
        public bool? SpecificFeedback { get; set; }
        public string SpecificFeedbackNotes { get; set; }
        public bool? AssessmentsAligned { get; set; }
        public string AssessmentsAlignedNotes { get; set; }
        public bool? SufficientTime { get; set; }
        public string SufficientTimeNotes { get; set; }
        public string AssessmentKeyIssues { get; set; }
        public string AssessmentStrengths { get; set; }
        public string AssessmentImprovements { get; set; }
        public string AssessmentComments { get; set; }
        public int AssessmentRating { get; set; }

        //End of Assessment & Progress Tracking section



        //Learning Resources & Environment
        public bool? MaterialsAccessible { get; set; }
        public string MaterialsAccessibleNotes { get; set; }
        public bool? PlatformOrganised { get; set; }
        public string PlatformOrganisedNotes { get; set; }
        public bool? EquipmentWorking { get; set; }
        public string EquipmentWorkingNotes { get; set; }
        public bool? SupplementaryResources { get; set; }
        public string SupplementaryResourcesNotes { get; set; }
        public bool? SpecializedEquipment { get; set; }
        public string SpecializedEquipmentNotes { get; set; }
        public bool? LibraryResources { get; set; }
        public string LibraryResourcesNotes { get; set; }
        public string ResourcesKeyIssues { get; set; }
        public string ResourcesStrengths { get; set; }
        public string ResourcesImprovements { get; set; }
        public string ResourcesComments { get; set; }
        public int ResourcesRating { get; set; }

        //End of Learning Resources & Environment section



        //Communication & Support
        public bool? StaffResponsive { get; set; }
        public string StaffResponsiveNotes { get; set; }
        public bool? AdditionalHelpAvailable { get; set; }
        public string AdditionalHelpAvailableNotes { get; set; }
        public bool? AccommodationsProvided { get; set; }
        public string AccommodationsProvidedNotes { get; set; }
        public bool? ClearPointsOfContact { get; set; }
        public string ClearPointsOfContactNotes { get; set; }

        public string SupportEffectivenesssKeyIssues { get; set; }
        public string SupportEffectivenessStrengths { get; set; }
        public string SupportEffectivenessImprovements { get; set; }
        public string SupportEffectivenessComments { get; set; }
        public int SupportEffectivenessRating { get; set; }

        //End of Communication & Support section



        //Skills Development
        public bool? DevelopingCriticalThinking { get; set; }
        public string DevelopingCriticalThinkingNotes { get; set; }
        public bool? EnhancingProblemSolving { get; set; }
        public string EnhancingProblemSolvingNotes { get; set; }
        public bool? GainingPracticalSkills { get; set; }
        public string GainingPracticalSkillsNotes { get; set; }
        public bool? ImprovingCommunication { get; set; }
        public string ImprovingCommunicationNotes { get; set; }
        public bool? DevelopingResearchSkills { get; set; }
        public string DevelopingResearchSkillsNotes { get; set; }
        public string SkillsDevelopmentKeyIssues { get; set; }
        public string SkillsDevelopmentStrengths { get; set; }
        public string SkillsDevelopmentImprovements { get; set; }
        public string SkillsDevelopmentComments { get; set; }
        public int SkillsDevelopmentRating { get; set; }

        //End of Skills Development section



        // Overall comments section
        public string BestFeatures { get; set; }
        public string AreasForImprovement { get; set; }
        public int OverallRating { get; set; }

        // Status properties
        public DateTime DateCreated { get; set; }
        public bool IsSubmitted { get; set; }

        // The class this feedback is for
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class TargetClass { get; set; }

        public bool IsReadByGuidanceTeacher { get; set; }

        public bool IsReadByCurriculumHead { get; set; }

        public DetailedFeedback()
        {
            DateCreated = DateTime.Now;
            IsSubmitted = false;

            // Set default rating values
            OverallRating = 5;
            LearningTeachingRating = 5;
            AssessmentRating = 5;
            ResourcesRating = 5;
            SupportEffectivenessRating = 5;
            SkillsDevelopmentRating = 5;
        }
    }
}