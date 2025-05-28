using System.Collections.Generic; // Required for List<T>
using System.ComponentModel.DataAnnotations;
using System.Web; // Required for HttpPostedFileBase
using GuidanceTracker.Models; // Ensure this is present to reference GuidanceTeacher model

namespace GuidanceTracker.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the class Excel upload page.
    /// </summary>
    public class UploadClassesViewModel
    {
        /// <summary>
        /// Gets or sets the Excel file uploaded by the user.
        /// </summary>
        [Display(Name = "Excel File")]
        [Required(ErrorMessage = "Please select an Excel file.")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase ExcelFile { get; set; }

        /// <summary>
        /// Gets or sets the list of guidance teachers to display in the view
        /// for user reference when populating the spreadsheet.
        /// </summary>
        public List<GuidanceTeacher> GuidanceTeachers { get; set; }
    }
}