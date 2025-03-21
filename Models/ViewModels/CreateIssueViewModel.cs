using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuidanceTracker.Models.ViewModels
{
    public class CreateIssueViewModel
    {
        [Required]
        public int SelectedClassId { get; set; }

        [Required]
        public int SelectedUnitId { get; set; }

        [Required]
        public List<string> SelectedStudentIds { get; set; }

        [Required]
        public IssueTitle IssueTitle { get; set; }


        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        // For dropdowns
        public IEnumerable<Class> Classes { get; set; }
        public IEnumerable<Unit> Units { get; set; }
        public IEnumerable<Student> Students { get; set; }
    }
}
