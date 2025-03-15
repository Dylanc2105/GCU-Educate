using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        [Required]
        public DateTime Time { get; set; }
        public string Room { get; set; }
        [Display(Name = "Appointment Comment")]
        public string AppointmentComment { get; set; }


        //nav props
        public int TicketId { get; set; }

        // Navigation properties
        public virtual Ticket Ticket { get; set; }


    }
}