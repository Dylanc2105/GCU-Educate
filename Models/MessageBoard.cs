using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
	public class MessageBoard
	{
        [Key]
        public int MessageId { get; set; }

        public DateTime PostDate { get; set; }

        [Required]
        public string Content { get; set; }

        // Navigation properties

        public ICollection<Student> Students { get; set; }


    }
}