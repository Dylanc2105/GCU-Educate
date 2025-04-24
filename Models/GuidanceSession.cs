using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class GuidanceSession
    {
        public int GuidanceSessionId { get; set; }
        public string Room { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime Day { get; set; }

        //nav props
        [Required]
        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }



        public virtual ICollection<Appointment> Appointments { get; set; }




    }
}