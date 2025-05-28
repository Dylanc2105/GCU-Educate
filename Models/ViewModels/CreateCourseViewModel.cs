using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // Required for SelectList

namespace GuidanceTracker.Models.ViewModels
{
    public class CreateCourseViewModel
    {
        [Required(ErrorMessage = "Course Name is required.")]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }

        [Required(ErrorMessage = "Course Reference is required.")]
        [Display(Name = "Course Reference")]
        [Remote("IsCourseReferenceUnique", "Course", ErrorMessage = "Course Reference already exists.")]
        public string CourseReference { get; set; }

        [Required(ErrorMessage = "Mode of Study is required.")]
        [Display(Name = "Mode of Study")]
        public string ModeOfStudy { get; set; }

        [Required(ErrorMessage = "Duration in Weeks is required.")]
        [Display(Name = "Duration in Weeks")]
        [Range(1, 520, ErrorMessage = "Duration must be between 1 and 520 weeks.")]
        public int DurationInWeeks { get; set; }

        [Required(ErrorMessage = "SCQF Level is required.")]
        [Display(Name = "SCQF Level")]
        [Range(1, 12, ErrorMessage = "SCQF Level must be between 1 and 12.")]
        public int SCQFLevel { get; set; }

        [Display(Name = "Site")]
        public string Site { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public string DepartmentId { get; set; } // This will hold the selected Department's ID

        // Property to populate the dropdown list for Departments
        public IEnumerable<SelectListItem> DepartmentsList { get; set; }
    }
}