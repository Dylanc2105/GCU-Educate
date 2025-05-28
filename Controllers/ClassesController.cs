using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels; 
using OfficeOpenXml; // Required for EPPlus
using System.IO; // Required for FileStream, MemoryStream

namespace GuidanceTracker.Controllers
{
    /// <summary>
    /// Controller responsible for managing Class entities, including creation and bulk upload.
    /// </summary>
    [Authorize(Roles = "CurriculumHead")]
    public class ClassesController : Controller
    {
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        /// <summary>
        /// Disposes the database context when the controller is disposed.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Classes/CreateClass (Manual Creation)
        /// <summary>
        /// Displays the form for manually creating a new class.
        /// Populates a dropdown list with available guidance teachers.
        /// </summary>
        /// <returns>The CreateClass view.</returns>
        [HttpGet]
        public ActionResult Create()
        {
            var teachers = db.GuidanceTeachers.ToList();
            var viewModel = new CreateClassViewModel
            {
                // Create a SelectList for the dropdown, using Teacher's Id as value and FullName as text
                GuidanceTeachers = new SelectList(teachers, "Id", "FullName")
            };
            return View(viewModel);
        }

        // POST: Classes/CreateClass (Manual Creation)
        /// <summary>
        /// Handles the submission of the form for manually creating a new class.
        /// Validates input and saves the new class to the database.
        /// </summary>
        /// <param name="viewModel">The ViewModel containing the class data from the form.</param>
        /// <returns>Redirects on success or returns to the form with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateClassViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate class name before saving
                if (db.Classes.Any(c => c.ClassName.Equals(viewModel.ClassName, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("ClassName", "A class with this name already exists.");
                }
                else
                {
                    var newClass = new Class
                    {
                        ClassName = viewModel.ClassName,
                        MaxCapacity = viewModel.MaxCapacity,
                        GuidanceTeacherId = viewModel.GuidanceTeacherId
                    };

                    db.Classes.Add(newClass);
                    db.SaveChanges();

                    TempData["Message"] = $"Class '{newClass.ClassName}' created successfully!";
                    // Redirect to a suitable dashboard or list of classes
                    // Assuming EnrollmentAcademicOperationsCenter in CurriculumHead is a general admin dashboard
                    return RedirectToAction("EnrollmentAcademicOperationsCenter", "CurriculumHead");
                }
            }

            // If ModelState is not valid or duplicate name, repopulate teachers and return to view
            var teachers = db.GuidanceTeachers.ToList();
            viewModel.GuidanceTeachers = new SelectList(teachers, "Id", "FullName", viewModel.GuidanceTeacherId);
            return View(viewModel);
        }


        // GET: Classes/UploadClasses
        /// <summary>
        /// Displays the page for uploading class data from an Excel file.
        /// Populates the view with a list of available guidance teachers.
        /// </summary>
        /// <returns>The UploadClasses view.</returns>
        [HttpGet]
        public ActionResult UploadClasses()
        {
            var teachers = db.GuidanceTeachers.ToList(); // Fetch all guidance teachers
            var viewModel = new UploadClassesViewModel
            {
                GuidanceTeachers = teachers // Assign the list to the ViewModel
            };
            return View(viewModel);
        }

        // POST: Classes/UploadClasses
        /// <summary>
        /// Handles the upload of an Excel file containing class data.
        /// Parses the file, validates data, creates new Class records, and reports errors.
        /// </summary>
        /// <param name="viewModel">The ViewModel containing the uploaded Excel file.</param>
        /// <returns>Redirects to the same page with messages or errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadClasses(UploadClassesViewModel viewModel)
        {
            if (viewModel.ExcelFile == null || viewModel.ExcelFile.ContentLength == 0)
            {
                TempData["Message"] = "Error: Please select an Excel file to upload.";
                return RedirectToAction("UploadClasses");
            }

            if (!Path.GetExtension(viewModel.ExcelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Message"] = "Error: Please upload a valid .xlsx Excel file.";
                return RedirectToAction("UploadClasses");
            }

            var excelErrors = new List<string>();
            var uploadedClassesCount = 0;

            try
            {
                using (var package = new ExcelPackage(viewModel.ExcelFile.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["Message"] = "Error: Excel file contains no worksheets.";
                        return RedirectToAction("UploadClasses");
                    }

                    var rowCount = worksheet.Dimension.Rows;
                    var colCount = worksheet.Dimension.Columns;

                    // Map column headers to their indices
                    var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int col = 1; col <= colCount; col++)
                    {
                        string headerText = worksheet.Cells[1, col].Text?.Trim();
                        if (!string.IsNullOrEmpty(headerText))
                        {
                            headerMap[headerText] = col;
                        }
                    }

                    // Define required headers for Classes
                    var requiredHeaders = new List<string> { "ClassName", "MaxCapacity", "GuidanceTeacherId" };
                    foreach (var header in requiredHeaders)
                    {
                        if (!headerMap.ContainsKey(header))
                        {
                            excelErrors.Add($"Missing required column: '{header}'. Please check your Excel file format.");
                        }
                    }

                    if (excelErrors.Any())
                    {
                        TempData["ExcelErrors"] = excelErrors;
                        return RedirectToAction("UploadClasses");
                    }

                    // Process data rows
                    for (int row = 2; row <= rowCount; row++) // Start from row 2 to skip headers
                    {
                        var className = worksheet.Cells[row, headerMap["ClassName"]]?.Text?.Trim();
                        var maxCapacityString = worksheet.Cells[row, headerMap["MaxCapacity"]]?.Text?.Trim();
                        var guidanceTeacherIdString = worksheet.Cells[row, headerMap["GuidanceTeacherId"]]?.Text?.Trim();

                        // Basic validation for the row to skip entirely empty rows
                        if (string.IsNullOrWhiteSpace(className) && string.IsNullOrWhiteSpace(maxCapacityString) && string.IsNullOrWhiteSpace(guidanceTeacherIdString))
                        {
                            continue;
                        }

                        var rowErrors = new List<string>();

                        // Validate ClassName
                        if (string.IsNullOrWhiteSpace(className))
                        {
                            rowErrors.Add($"Row {row}: Class Name is required.");
                        }
                        else if (db.Classes.Any(c => c.ClassName.Equals(className, StringComparison.OrdinalIgnoreCase)))
                        {
                            rowErrors.Add($"Row {row}: Class with name '{className}' already exists.");
                        }

                        // Validate MaxCapacity
                        int maxCapacity = 0; // FIX: Initialize maxCapacity to prevent 'unassigned' error
                        if (string.IsNullOrWhiteSpace(maxCapacityString))
                        {
                            rowErrors.Add($"Row {row}: Max Capacity is required.");
                        }
                        else if (!int.TryParse(maxCapacityString, out maxCapacity))
                        {
                            rowErrors.Add($"Row {row}: Invalid Max Capacity format '{maxCapacityString}'. It must be a whole number.");
                        }
                        else if (maxCapacity <= 0) // This check runs after successful parse
                        {
                            rowErrors.Add($"Row {row}: Max Capacity must be a positive whole number.");
                        }

                        // Validate GuidanceTeacherId
                        Guid guidanceTeacherGuid;
                        GuidanceTeacher guidanceTeacher = null;
                        if (string.IsNullOrWhiteSpace(guidanceTeacherIdString))
                        {
                            rowErrors.Add($"Row {row}: Guidance Teacher ID is required.");
                        }
                        else if (!Guid.TryParse(guidanceTeacherIdString, out guidanceTeacherGuid))
                        {
                            rowErrors.Add($"Row {row}: Invalid Guidance Teacher ID format '{guidanceTeacherIdString}'. It must be a valid GUID.");
                        }
                        else
                        {
                            guidanceTeacher = db.GuidanceTeachers.Find(guidanceTeacherIdString);
                            if (guidanceTeacher == null)
                            {
                                rowErrors.Add($"Row {row}: Guidance Teacher with ID '{guidanceTeacherIdString}' not found.");
                            }
                        }

                        // If any errors for the current row, add to overall errors and skip to next row
                        if (rowErrors.Any())
                        {
                            excelErrors.AddRange(rowErrors);
                            continue;
                        }

                        // Create new Class object
                        var newClass = new Class
                        {
                            ClassName = className,
                            MaxCapacity = maxCapacity, // maxCapacity is now guaranteed to be assigned
                            GuidanceTeacherId = guidanceTeacherIdString // Already validated as existing GUID
                        };

                        db.Classes.Add(newClass);
                        uploadedClassesCount++;
                    }

                    db.SaveChanges(); // Save all valid classes after processing all rows

                    if (excelErrors.Any())
                    {
                        TempData["ExcelErrors"] = excelErrors;
                        TempData["Message"] = $"Successfully uploaded {uploadedClassesCount} classes. However, some errors were found (see below).";
                    }
                    else
                    {
                        TempData["Message"] = $"Successfully uploaded {uploadedClassesCount} classes.";
                    }
                }
            }
            catch (Exception ex)
            {
                excelErrors.Add($"An unexpected error occurred during upload: {ex.Message}");
                TempData["ExcelErrors"] = excelErrors;
                TempData["Message"] = "An unexpected error occurred during Excel processing. Please check the file format.";
            }

            return RedirectToAction("UploadClasses");
        }


        // GET: Classes/DownloadClassTemplate
        /// <summary>
        /// Provides an Excel template file for class data upload.
        /// </summary>
        /// <returns>An Excel file for download.</returns>
        public ActionResult DownloadClassTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Class Data");

                // Set headers for the template
                worksheet.Cells[1, 1].Value = "ClassName";
                worksheet.Cells[1, 2].Value = "MaxCapacity";
                worksheet.Cells[1, 3].Value = "GuidanceTeacherId";

                // Apply some basic styling to the header row
                using (var range = worksheet.Cells[1, 1, 1, 3])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // AutoFit columns for better readability
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"ClassUploadTemplate-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
    }
}