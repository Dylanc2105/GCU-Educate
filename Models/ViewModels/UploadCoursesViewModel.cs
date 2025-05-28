using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations;
using System.Web;
using GuidanceTracker.Models; 

namespace GuidanceTracker.Models.ViewModels
{
    public class UploadCoursesViewModel
    {
        [Display(Name = "Excel File")]
        [Required(ErrorMessage = "Please select an Excel file.")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase ExcelFile { get; set; }

        //property to hold the list of departments for display
        public List<Department> Departments { get; set; }
    }
}