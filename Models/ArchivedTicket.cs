using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
	public class ArchivedTicket
	{
        [Key]
        [ForeignKey("Issue")]
        public int IssueId { get; set; }
        public DateTime ArchivedAt { get; set; } = DateTime.Now;
        public string ArchivedBy { get; set; }

        public virtual Issue Issue { get; set; }
        public ICollection<ArchivedComment> ArchivedComments { get; set; }
    }
}