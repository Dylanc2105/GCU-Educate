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
        public virtual ICollection<Module> Modules { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; } 
    }
}