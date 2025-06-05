using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace GuidanceTracker.Models.ViewModels
{
    /// <summary>
    /// ViewModel for manually creating a single student.
    /// </summary>
    public class CreateStudentViewModel
    {
        [Display(Name = "Class")]
        [Required(ErrorMessage = "Class is required.")]
        public int ClassId { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Student Number is required.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Student Number must be exactly 8 digits.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Student Number must be an 8-digit number.")]
        [Display(Name = "Student Number (8 digits, will be initial password)")]
        public string StudentNumber { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\d{6,12}$", ErrorMessage = "Phone Number must consist of 6 to 12 digits only.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [Display(Name = "Address")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        public string Address { get; set; }

        // Dropdown list for Classes
        public IEnumerable<SelectListItem> Classes { get; set; }
    }
}