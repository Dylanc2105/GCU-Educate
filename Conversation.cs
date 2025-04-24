using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace GuidanceTracker.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserOneId { get; set; }

        [Required]
        public string UserTwoId { get; set; }

        public bool IsPinnedByUserOne { get; set; }
        public bool IsPinnedByUserTwo { get; set; }

        public DateTime LastUpdated { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

        [ForeignKey("UserOneId")]
        public virtual User UserOne { get; set; }

        [ForeignKey("UserTwoId")]
        public virtual User UserTwo { get; set; }
    }


}