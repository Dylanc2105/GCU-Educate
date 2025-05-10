using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuidanceTracker.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int IssueId { get; set; }

        [ForeignKey("IssueId")]
        public virtual Issue Issue { get; set; }

        // Optional reference to an archived comment if this was restored
        public int? ArchivedCommentId { get; set; }
    }
}
