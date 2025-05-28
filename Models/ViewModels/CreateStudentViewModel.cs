using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace GuidanceTracker.Models.ViewModels
{
    /// <summary>
    /// ViewModel for manually creating a single student. StudentNumber is auto-generated.
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

        // Password field - ConfirmPassword and its [Compare] attribute are removed
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        // New Address Fields
        [Display(Name = "Street")]
        [StringLength(200, ErrorMessage = "Street cannot exceed 200 characters.")]
        public string Street { get; set; }

        [Display(Name = "City")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; }

       
        [Display(Name = "Postcode")]
        [StringLength(10, ErrorMessage = "Postcode cannot exceed 10 characters.")]
        public string Postcode { get; set; }


        [Display(Name = "Is Class Representative")]
        public bool IsClassRep { get; set; }

        [Display(Name = "Is Deputy Class Representative")]
        public bool IsDeputyClassRep { get; set; }

        // Dropdown list for Classes
        public IEnumerable<SelectListItem> Classes { get; set; }
    }
}