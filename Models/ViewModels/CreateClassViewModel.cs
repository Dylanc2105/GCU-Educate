using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // Required for SelectList

namespace GuidanceTracker.Models.ViewModels
{
    /// <summary>
    /// ViewModel for manually creating a single class.
    /// </summary>
    public class CreateClassViewModel
    {
        /// <summary>
        /// Gets or sets the name of the class.
        /// </summary>
        [Required(ErrorMessage = "Class Name is required.")]
        [StringLength(100, ErrorMessage = "Class Name cannot exceed 100 characters.")]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the maximum capacity of the class.
        /// </summary>
        [Required(ErrorMessage = "Max Capacity is required.")]
        [Range(1, 500, ErrorMessage = "Max Capacity must be between 1 and 500.")] // Example range
        [Display(Name = "Maximum Capacity")]
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Gets or sets the ID of the assigned Guidance Teacher.
        /// </summary>
        [Required(ErrorMessage = "Guidance Teacher is required.")]
        [Display(Name = "Guidance Teacher")]
        public string GuidanceTeacherId { get; set; }

        /// <summary>
        /// Gets or sets the SelectList for populating the Guidance Teacher dropdown.
        /// </summary>
        public SelectList GuidanceTeachers { get; set; }
    }
}