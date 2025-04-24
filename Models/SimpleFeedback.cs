using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuidanceTracker.Models
{
	public class SimpleFeedback
	{
        [Key]
		public int FeedbackId { get; set; }

		public string FeedbackTitle { get; set; }

        public string FeedbackContent { get; set; }

        public DateTime DateOfCreation { get; set; }

        public Reciepient Reciepient { get; set; }

        public bool IsRead { get; set; }
        public bool IsReadByGuidanceTeacher { get; set; }

        public bool IsReadByCurriculumHead { get; set; }

        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public string GuidanceTeacherId { get; set; }

        [ForeignKey("GuidanceTeacherId")]
        public GuidanceTeacher GuidanceTeacher { get; set; }

		public string CurriculumHeadId { get; set; }

        [ForeignKey("CurriculumHeadId")]
        public CurriculumHead CurriculumHead { get; set; }

        public int? UnitId { get; set; }

        [ForeignKey("UnitId")]
        public Unit Unit { get; set; }
    }

    public enum  Reciepient
    {
        CurriculumHead,
        GuidanceTeacher
    }

}