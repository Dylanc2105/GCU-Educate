using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GuidanceTracker.Models
{
    public class Post
    {

        public Post()
        {
            Departments = new List<Department>();
            Courses = new List<Course>();
            Classes = new List<Class>();
        }

        [Key]
        public string PostId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public DateTime PostDate { get; set; }

        [Required]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; }

        public VisibilityType Visibility { get; set; }

        // Navigation properties for specific visibility types
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
    }




    public enum VisibilityType
    {
        Global,
        Staff,
        Student,
        Department,
        Course,
        Class,
    }
}