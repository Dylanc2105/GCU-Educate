using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Models.ViewModels
{
    public class CreatePostViewModel
    {
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Visibility")]
        public VisibilityType Visibility { get; set; }

        // Dropdown options for visibility
        public IEnumerable<SelectListItem> VisibilityOptions { get; set; }

        // For Department visibility
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }

        // For Course visibility
        [Display(Name = "Course")]
        public int? CourseId { get; set; }
        public IEnumerable<SelectListItem> Courses { get; set; }

        // For Class visibility
        [Display(Name = "Class")]
        public int? ClassId { get; set; }
        public IEnumerable<SelectListItem> Classes { get; set; }

        // For Unit visibility
        [Display(Name = "Unit")]
        public int? UnitId { get; set; }
        public IEnumerable<SelectListItem> Units { get; set; }

        public CreatePostViewModel()
        {
            VisibilityOptions = new List<SelectListItem>();
            Departments = new List<SelectListItem>();
            Courses = new List<SelectListItem>();
            Classes = new List<SelectListItem>();
            Units = new List<SelectListItem>();
        }
    }
}