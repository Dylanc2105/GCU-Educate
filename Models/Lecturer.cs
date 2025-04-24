using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Lecturer:User
    {
        // Navigation properties

        public virtual ICollection<Unit> Units { get; set; }
        public virtual ICollection<Issue> Issues { get; set; }

        public virtual ICollection<SimpleFeedback> Feedbacks { get; set; }
    }
}