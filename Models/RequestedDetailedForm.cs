using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class RequestedDetailedForm
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int ClassId { get; set; }

        public int? UnitId { get; set; }

        [Required]
        public string CreatorId { get; set; }

        [Required]
        public string StudentId { get; set; }

        [Required]
        public DateTime DateRequested { get; set; }

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class TargetClass { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

    }
}