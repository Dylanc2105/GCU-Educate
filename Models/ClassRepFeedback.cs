using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class ClassRepFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        // Basic information
        public string Course { get; set; }
        public string Class { get; set; }
        // The creator (guidance teacher or curriculum head)
        public string CreatorId { get; set; }
        // The student submitting the feedback
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        public string ClassRepId { get; set; }

        public bool IsReadByGuidanceTeacher { get; set; }
        public bool IsReadByCurriculumHead { get; set; }

        //Current Learning Experience section
        public int MeetsExpectationsYesCount { get; set; }
        public int MeetsExpectationsNoCount { get; set; }
        public string MeetsExpectationsNotes { get; set; }
        public int WouldRecommendYesCount { get; set; }
        public int WouldRecommendNoCount { get; set; }
        public string WouldRecommendNotes { get; set; }
        public int WorkloadManageableNoCount { get; set; }
        public int WorkloadManageableYesCount { get; set; }
        public string WorkloadManageableNotes { get; set; }
        public string LearningExperienceKeyIssues { get; set; }
        public string LearningExperienceStrengths { get; set; }
        public string LearningExperienceImprovements { get; set; }
        public string LearningExperienceComments { get; set; }
        public int LearningExperienceRating { get; set; }
        public string LearningExperienceClassDiscussion { get; set; }
        //End of Current Learning Experience section

        // Teaching & Learning Methods section
        public int ConceptsPresentedYesCount { get; set; }
        public int ConceptsPresentedNoCount { get; set; }
        public string ConceptsPresentedNotes { get; set; }
        public int MaterialsAvailableYesCount { get; set; }
        public int MaterialsAvailableNoCount { get; set; }
        public string MaterialsAvailableNotes { get; set; }
        public int AccommodatesStylesYesCount { get; set; }
        public int AccommodatesStylesNoCount { get; set; }
        public string AccommodatesStylesNotes { get; set; }
        public int LecturerResponsiveYesCount { get; set; }
        public int LecturerResponsiveNoCount { get; set; }
        public string LecturerResponsiveNotes { get; set; }
        public string LearningTeachingKeyIssues { get; set; }
        public string LearningTeachingStrengths { get; set; }
        public string LearningTeachingImprovements { get; set; }
        public string LearningTeachingComments { get; set; }
        public int LearningTeachingRating { get; set; }
        public string LearningTeachingClassDiscussion { get; set; }
        //End of Teaching & Learning Methods section

        //Assessment & Progress Tracking
        public int AssessmentConfidenceYesCount { get; set; }
        public int AssessmentConfidenceNoCount { get; set; }
        public string AssessmentConfidenceNotes { get; set; }
        public int TimelyFeedbackYesCount { get; set; }
        public int TimelyFeedbackNoCount { get; set; }
        public string TimelyFeedbackNotes { get; set; }
        public int SpecificFeedbackYesCount { get; set; }
        public int SpecificFeedbackNoCount { get; set; }
        public string SpecificFeedbackNotes { get; set; }
        public int AssessmentsAlignedYesCount { get; set; }
        public int AssessmentsAlignedNoCount { get; set; }
        public string AssessmentsAlignedNotes { get; set; }
        public int SufficientTimeYesCount { get; set; }
        public int SufficientTimeNoCount { get; set; }
        public string SufficientTimeNotes { get; set; }
        public string AssessmentKeyIssues { get; set; }
        public string AssessmentStrengths { get; set; }
        public string AssessmentImprovements { get; set; }
        public string AssessmentComments { get; set; }
        public int AssessmentRating { get; set; }
        public string AssessmentClassDiscussion { get; set; }
        //End of Assessment & Progress Tracking section

        //Learning Resources & Environment
        public int MaterialsAccessibleYesCount { get; set; }
        public int MaterialsAccessibleNoCount { get; set; }
        public string MaterialsAccessibleNotes { get; set; }
        public int PlatformOrganisedYesCount { get; set; }
        public int PlatformOrganisedNoCount { get; set; }
        public string PlatformOrganisedNotes { get; set; }
        public int EquipmentWorkingYesCount { get; set; }
        public int EquipmentWorkingNoCount { get; set; }
        public string EquipmentWorkingNotes { get; set; }
        public int SupplementaryResourcesYesCount { get; set; }
        public int SupplementaryResourcesNoCount { get; set; }
        public string SupplementaryResourcesNotes { get; set; }
        public int SpecialisedEquipmentYesCount { get; set; }
        public int SpecialisedEquipmentNoCount { get; set; }
        public string SpecialisedEquipmentNotes { get; set; }
        public int LibraryResourcesYesCount { get; set; }
        public int LibraryResourcesNoCount { get; set; }
        public string LibraryResourcesNotes { get; set; }
        public string ResourcesKeyIssues { get; set; }
        public string ResourcesStrengths { get; set; }
        public string ResourcesImprovements { get; set; }
        public string ResourcesComments { get; set; }
        public int ResourcesRating { get; set; }
        public string ResourcesClassDiscussion { get; set; }
        //End of Learning Resources & Environment section

        //Communication & Support
        public int StaffResponsiveYesCount { get; set; }
        public int StaffResponsiveNoCount { get; set; }
        public string StaffResponsiveNotes { get; set; }
        public int AdditionalHelpAvailableYesCount { get; set; }
        public int AdditionalHelpAvailableNoCount { get; set; }
        public string AdditionalHelpAvailableNotes { get; set; }
        public int AccommodationsProvidedYesCount { get; set; }
        public int AccommodationsProvidedNoCount { get; set; }
        public string AccommodationsProvidedNotes { get; set; }
        public int ClearPointsOfContactYesCount { get; set; }
        public int ClearPointsOfContactNoCount { get; set; }
        public string ClearPointsOfContactNotes { get; set; }
        public string SupportEffectivenesssKeyIssues { get; set; }
        public string SupportEffectivenessStrengths { get; set; }
        public string SupportEffectivenessImprovements { get; set; }
        public string SupportEffectivenessComments { get; set; }
        public int SupportEffectivenessRating { get; set; }
        public string SupportEffectivenessClassDiscussion { get; set; }
        //End of Communication & Support section

        //Skills Development
        public int DevelopingCriticalThinkingYesCount { get; set; }
        public int DevelopingCriticalThinkingNoCount { get; set; }
        public string DevelopingCriticalThinkingNotes { get; set; }
        public int EnhancingProblemSolvingYesCount { get; set; }
        public int EnhancingProblemSolvingNoCount { get; set; }
        public string EnhancingProblemSolvingNotes { get; set; }
        public int GainingPracticalSkillsYesCount { get; set; }
        public int GainingPracticalSkillsNoCount { get; set; }
        public string GainingPracticalSkillsNotes { get; set; }
        public int ImprovingCommunicationYesCount { get; set; }
        public int ImprovingCommunicationNoCount { get; set; }
        public string ImprovingCommunicationNotes { get; set; }
        public int DevelopingResearchSkillsYesCount { get; set; }
        public int DevelopingResearchSkillsNoCount { get; set; }
        public string DevelopingResearchSkillsNotes { get; set; }
        public string SkillsDevelopmentKeyIssues { get; set; }
        public string SkillsDevelopmentStrengths { get; set; }
        public string SkillsDevelopmentImprovements { get; set; }
        public string SkillsDevelopmentComments { get; set; }
        public int SkillsDevelopmentRating { get; set; }
        public string SkillsDevelopmentClassDiscussion { get; set; }
        //End of Skills Development section

        // Overall comments section
        public string BestFeatures { get; set; }
        public string AreasForImprovement { get; set; }
        public int OverallRating { get; set; }

        // Status properties 
        public string OverallClassDiscussionNotes { get; set; }
        public string AdditionalClassComments { get; set; }
        public DateTime? DateSubmittedByClassRep { get; set; }
        public bool IsSubmittedByClassRep { get; set; }

        public bool IsRequested { get; set; } 

        // The class this feedback is for
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class TargetClass { get; set; }

        public List<DetailedFeedback> DetailedFeedbacks { get; set; } = new List<DetailedFeedback>();

        public ClassRepFeedback()
        {
            DateSubmittedByClassRep = null;
            IsSubmittedByClassRep = false;
            IsRequested = true;

            // Set default rating values
            OverallRating = 5;
            LearningTeachingRating = 5;
            AssessmentRating = 5;
            ResourcesRating = 5;
            SupportEffectivenessRating = 5;
            SkillsDevelopmentRating = 5;

            // Initialize class discussion notes as empty strings
            LearningExperienceClassDiscussion = string.Empty;
            LearningTeachingClassDiscussion = string.Empty;
            AssessmentClassDiscussion = string.Empty;
            ResourcesClassDiscussion = string.Empty;
            SupportEffectivenessClassDiscussion = string.Empty;
            SkillsDevelopmentClassDiscussion = string.Empty;
            OverallClassDiscussionNotes = string.Empty;
        }
    }
}



    
