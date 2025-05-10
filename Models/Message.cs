using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        public bool IsArchived { get; set; } = false;


        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }
    }


}