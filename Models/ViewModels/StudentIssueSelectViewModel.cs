using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuidanceTracker.Models.ViewModels
{
    public class StudentIssueSelectViewModel
    {
        public int SelectedClassId { get; set; }
        public string SelectedStudentId { get; set; }

        // For dropdowns
        public IEnumerable<Class> Classes { get; set; }
        public IEnumerable<Student> Students { get; set; }

        // Issues for the selected student
        public IEnumerable<Issue> StudentIssues { get; set; }

        // Student name for display
        public string StudentName { get; set; }
    }
}