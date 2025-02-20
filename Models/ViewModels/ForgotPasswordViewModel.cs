using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress, Display(Name = "Registered Email Address") ]
        public string Email { get; set; }
        public bool EmailSent { get; set; }
    }
}