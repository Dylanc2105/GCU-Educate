using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Models.ViewModels
{
    public class CreateStudentViewModel
    {

        [Display(Name = "Class")]
        [Required]
        public int ClassId { get; set; }

        public string CourseName { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        [DataType(DataType.PostalCode)]
        [Display(Name = "Post Code")]
        public string Postcode { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email Confirm")]
        public bool EmailConfirm { get; set; }

        [Display(Name = "Phone Confirm")]
        public bool PhoneConfirm { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
        public IEnumerable<SelectListItem> Classes { get; set; }
        public IEnumerable<SelectListItem> Courses { get; set; }

    }
}