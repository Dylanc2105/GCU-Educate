using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
	// this is a tracking table to see if a post has been read by a user. This is to help with notifications about new announcements(posts)
	public class PostRead
	{
        [Key]
        public int Id { get; set; }

        [Required]
        public string PostId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime ReadOn { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

    }
}