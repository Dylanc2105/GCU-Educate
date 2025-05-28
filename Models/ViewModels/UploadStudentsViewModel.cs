using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web; // Required for HttpPostedFileBase

namespace GuidanceTracker.Models.ViewModels
{
    /// <summary>
    /// ViewModel for uploading student data from an Excel file.
    /// </summary>
    public class UploadStudentsViewModel
    {
        [Required(ErrorMessage = "Please select an Excel file.")]
        [Display(Name = "Excel File (.xlsx)")]
        public HttpPostedFileBase ExcelFile { get; set; }

        // Optional: List of guidance teachers and classes to display for reference in the view
        public List<GuidanceTracker.Models.GuidanceTeacher> GuidanceTeachers { get; set; }
        public List<GuidanceTracker.Models.Class> Classes { get; set; }
    }
}