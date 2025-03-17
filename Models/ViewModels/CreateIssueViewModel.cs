using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuidanceTracker.Models.ViewModels
{
    public class CreateIssueViewModel
    {
        [Required]
        public string StudentId { get; set; }

        public string StudentName { get; set; }
        public string ClassName { get; set; }

        [Required]
        public int SelectedUnitId { get; set; }

        public List<Unit> Units { get; set; }

        [Required]
        public string IssueType { get; set; }

        public string CustomIssue { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Description must be under 500 characters.")]
        public string IssueDescription { get; set; }
    }
}
