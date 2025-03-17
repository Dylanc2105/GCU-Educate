using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
	public class ArchivedComment
    {
        [Key]
        public int ArchivedCommentId { get; set; }

        // Store the original comment ID for reference
        public int OriginalCommentId { get; set; }

        [Required]
        public string CommentText { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ArchivedAt { get; set; }

        // Foreign Keys
        [Required]
        public int ArchivedTicketId { get; set; }

        // We store the User ID as string to preserve the data even if the original user is deleted
        [Required]
        public string UserId { get; set; }

        // Navigation property to ArchivedTicket
        [ForeignKey("ArchivedTicketId")]
        public virtual ArchivedTicket ArchivedTicket { get; set; }
    }
}