using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Unit
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string UnitDescription { get; set; }

        // Foreign keys
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<RequestedDetailedForm> RequestedDetailedForms { get; set; }
        public virtual ICollection<SimpleFeedback> Feedbacks { get; set; }

        [Required]

        public string LecturerId { get; set; }

        [ForeignKey("LecturerId")]

        public virtual Lecturer Lecturer { get; set; }

        public Unit()
        {
            Classes = new HashSet<Class>();
        }



    }
}