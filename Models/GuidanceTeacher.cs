using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace GuidanceTracker.Models
{
    public class GuidanceTeacher : User
    {
        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                // Concatenate FirstName and LastName, handling potential nulls
                return $"{FirstName} {LastName}".Trim();
            }
        }

        // Navigation properties
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Issue> Issues { get; set; }
        public virtual ICollection<Class> Classes { get; set; }

    }
    
}
