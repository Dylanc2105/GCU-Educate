using System.ComponentModel.DataAnnotations;
using System.Web; // Required for HttpPostedFileBase

namespace GuidanceTracker.Models.ViewModels
{
    public class UploadLecturersViewModel
    {
        [Required(ErrorMessage = "Please select an Excel file.")]
        [Display(Name = "Excel File (.xlsx)")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase ExcelFile { get; set; }
    }
}