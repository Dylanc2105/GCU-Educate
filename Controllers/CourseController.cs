using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml; // For EPPlus library
using System.IO; // For file operations

namespace GuidanceTracker.Controllers
{

    [Authorize(Roles = "CurriculumHead")] 
    public class CourseController : Controller
    {
        private GuidanceTrackerDbContext db;

        public CourseController()
        {
            db = new GuidanceTrackerDbContext(); // Initialize  DbContext
        }

        // Action to check if CourseReference is unique for client-side validation
        [AllowAnonymous] // Allow access to this action for validation without login if needed
        public JsonResult IsCourseReferenceUnique(string courseReference)
        {
            // Check if a course with this reference already exists
            var isUnique = !db.Courses.Any(c => c.CourseReference == courseReference);
            return Json(isUnique, JsonRequestBehavior.AllowGet);
        }


        // GET: Course/Create
        public ActionResult Create()
        {
            // Populate Departments dropdown
            var departments = db.Departments.ToList();
            var viewModel = new CreateCourseViewModel
            {
                DepartmentsList = new SelectList(departments, "DepartmentId", "DepartmentName")
            };
            return View(viewModel);
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateCourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Basic check for CourseReference uniqueness before saving
                if (db.Courses.Any(c => c.CourseReference == model.CourseReference))
                {
                    ModelState.AddModelError("CourseReference", "This Course Reference already exists.");
                    // Re-populate dropdown before returning view
                    model.DepartmentsList = new SelectList(db.Departments.ToList(), "DepartmentId", "DepartmentName", model.DepartmentId);
                    return View(model);
                }

                var course = new Course
                {
                    CourseName = model.CourseName,
                    CourseReference = model.CourseReference,
                    ModeOfStudy = model.ModeOfStudy,
                    DurationInWeeks = model.DurationInWeeks,
                    SCQFLevel = model.SCQFLevel,
                    Site = model.Site,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    DepartmentId = model.DepartmentId
                };

                db.Courses.Add(course);
                db.SaveChanges();
                TempData["Message"] = "Course created successfully!";
                return RedirectToAction("EnrollmentAcademicOperationsCenter", "CurriculumHead"); // Redirect to appropriate dashboard
            }

            // If ModelState is not valid, re-populate dropdown and return view
            model.DepartmentsList = new SelectList(db.Departments.ToList(), "DepartmentId", "DepartmentName", model.DepartmentId);
            return View(model);
        }


        // GET: Course/UploadExcel
        public ActionResult UploadExcel()
        {
            var departments = db.Departments.ToList(); // Fetch all departments
            var viewModel = new UploadCoursesViewModel
            {
                Departments = departments // Assign the list to the ViewModel property
            };
            return View(viewModel);
        }

        // POST: Course/UploadExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadExcel(UploadCoursesViewModel model)
        {
            if (model.ExcelFile == null || model.ExcelFile.ContentLength == 0)
            {
                ModelState.AddModelError("ExcelFile", "Please select an Excel file to upload.");
                return View(model);
            }

            if (!model.ExcelFile.FileName.EndsWith(".xlsx"))
            {
                ModelState.AddModelError("ExcelFile", "Please upload a valid .xlsx Excel file.");
                return View(model);
            }

            int successfulImports = 0;
            List<string> errors = new List<string>();

            try
            {
                using (var package = new ExcelPackage(model.ExcelFile.InputStream))
                {
                    // Ensure EPPlus license is set in Global.asax.cs
                    // #pragma warning disable CS0618
                    // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    // #pragma warning restore CS0618

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ModelState.AddModelError("ExcelFile", "The Excel file does not contain any worksheets.");
                        return View(model);
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    // Ensure minimum columns are present (e.g., CourseName, CourseReference, ModeOfStudy, DurationInWeeks, SCQFLevel, StartDate, EndDate, DepartmentId)
                    if (colCount < 8) // Based on required fields
                    {
                        ModelState.AddModelError("ExcelFile", "The Excel file must contain at least 8 columns: CourseName, CourseReference, ModeOfStudy, DurationInWeeks, SCQFLevel, StartDate, EndDate, DepartmentId.");
                        return View(model);
                    }

                    // Map column headers to their respective indexes
                    Dictionary<string, int> headerMap = new Dictionary<string, int>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        string header = worksheet.Cells[1, col].Text?.Trim();
                        if (!string.IsNullOrEmpty(header))
                        {
                            headerMap[header] = col;
                        }
                    }

                    // Check for required headers
                    string[] requiredHeaders = { "CourseName", "CourseReference", "ModeOfStudy", "DurationInWeeks", "SCQFLevel", "StartDate", "EndDate", "DepartmentId" };
                    foreach (var header in requiredHeaders)
                    {
                        if (!headerMap.ContainsKey(header))
                        {
                            ModelState.AddModelError("ExcelFile", $"Missing required column header: {header}");
                            return View(model);
                        }
                    }

                    for (int row = 2; row <= rowCount; row++) // Start from row 2 to skip headers
                    {
                        try
                        {
                            string courseName = worksheet.Cells[row, headerMap["CourseName"]].Text?.Trim();
                            string courseReference = worksheet.Cells[row, headerMap["CourseReference"]].Text?.Trim();
                            string modeOfStudy = worksheet.Cells[row, headerMap["ModeOfStudy"]].Text?.Trim();
                            string durationStr = worksheet.Cells[row, headerMap["DurationInWeeks"]].Text?.Trim();
                            string scqfLevelStr = worksheet.Cells[row, headerMap["SCQFLevel"]].Text?.Trim();
                            string site = null; // Initialize to null
                            int siteColIndex;
                            if (headerMap.TryGetValue("Site", out siteColIndex))
                            {
                                // Only try to read if the header exists
                                site = worksheet.Cells[row, siteColIndex].Text?.Trim();
                            }
                            string startDateStr = worksheet.Cells[row, headerMap["StartDate"]].Text?.Trim();
                            string endDateStr = worksheet.Cells[row, headerMap["EndDate"]].Text?.Trim();
                            string departmentId = worksheet.Cells[row, headerMap["DepartmentId"]].Text?.Trim();

                            // Basic validation for required fields from Excel
                            if (string.IsNullOrEmpty(courseName) ||
                                string.IsNullOrEmpty(courseReference) ||
                                string.IsNullOrEmpty(modeOfStudy) ||
                                string.IsNullOrEmpty(durationStr) ||
                                string.IsNullOrEmpty(scqfLevelStr) ||
                                string.IsNullOrEmpty(startDateStr) ||
                                string.IsNullOrEmpty(endDateStr) ||
                                string.IsNullOrEmpty(departmentId))
                            {
                                errors.Add($"Row {row}: Missing required data. Skipping row.");
                                continue;
                            }

                            // Data Type Parsing and Validation
                            int durationInWeeks;
                            if (!int.TryParse(durationStr, out durationInWeeks))
                            {
                                errors.Add($"Row {row}: Invalid Duration in Weeks '{durationStr}'. Skipping row.");
                                continue;
                            }
                            if (durationInWeeks < 1 || durationInWeeks > 520)
                            {
                                errors.Add($"Row {row}: Duration in Weeks must be between 1 and 520. Skipping row.");
                                continue;
                            }

                            int scqfLevel;
                            if (!int.TryParse(scqfLevelStr, out scqfLevel))
                            {
                                errors.Add($"Row {row}: Invalid SCQF Level '{scqfLevelStr}'. Skipping row.");
                                continue;
                            }
                            if (scqfLevel < 1 || scqfLevel > 12)
                            {
                                errors.Add($"Row {row}: SCQF Level must be between 1 and 12. Skipping row.");
                                continue;
                            }

                            DateTime startDate;
                            if (!DateTime.TryParse(startDateStr, out startDate))
                            {
                                errors.Add($"Row {row}: Invalid Start Date format '{startDateStr}'. Skipping row.");
                                continue;
                            }

                            DateTime endDate;
                            if (!DateTime.TryParse(endDateStr, out endDate))
                            {
                                errors.Add($"Row {row}: Invalid End Date format '{endDateStr}'. Skipping row.");
                                continue;
                            }

                            // Check for Department existence
                            var existingDepartment = db.Departments.Find(departmentId);
                            if (existingDepartment == null)
                            {
                                errors.Add($"Row {row}: Department with ID '{departmentId}' not found. Skipping row.");
                                continue;
                            }

                            // Check for CourseReference uniqueness
                            if (db.Courses.Any(c => c.CourseReference == courseReference))
                            {
                                errors.Add($"Row {row}: Course with reference '{courseReference}' already exists. Skipping row.");
                                continue;
                            }

                            var course = new Course
                            {
                                CourseName = courseName,
                                CourseReference = courseReference,
                                ModeOfStudy = modeOfStudy,
                                DurationInWeeks = durationInWeeks,
                                SCQFLevel = scqfLevel,
                                Site = site,
                                StartDate = startDate,
                                EndDate = endDate,
                                DepartmentId = departmentId
                            };

                            db.Courses.Add(course);
                            successfulImports++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row {row}: An unexpected error occurred - {ex.Message}. Skipping row.");
                        }
                    }

                    if (successfulImports > 0)
                    {
                        db.SaveChanges(); // Save all valid courses
                        TempData["Message"] = $"{successfulImports} courses imported successfully!";
                    }
                    else
                    {
                        TempData["Message"] = "No courses were imported. Please check for errors.";
                    }

                    if (errors.Any())
                    {
                        // Store errors in TempData to display on the next page
                        TempData["ExcelErrors"] = errors;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"An error occurred while processing the Excel file: {ex.Message}";
                // For detailed errors, log 'ex'
            }

            return RedirectToAction("UploadExcel"); // Redirect back to the upload page to show messages
        }


        // GET: Course/DownloadTemplate
        public FileResult DownloadTemplate()
        {
            using (var package = new ExcelPackage())
            {
                // Ensure EPPlus license is set in Global.asax.cs
                // #pragma warning disable CS0618
                // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // #pragma warning restore CS0618

                var worksheet = package.Workbook.Worksheets.Add("Courses");

                // Set headers
                worksheet.Cells[1, 1].Value = "CourseName";
                worksheet.Cells[1, 2].Value = "CourseReference";
                worksheet.Cells[1, 3].Value = "ModeOfStudy";
                worksheet.Cells[1, 4].Value = "DurationInWeeks";
                worksheet.Cells[1, 5].Value = "SCQFLevel";
                worksheet.Cells[1, 6].Value = "Site";
                worksheet.Cells[1, 7].Value = "StartDate";
                worksheet.Cells[1, 8].Value = "EndDate";
                worksheet.Cells[1, 9].Value = "DepartmentId"; // The actual Department ID (e.g., GUID or string)

                // Optional: Apply some basic styling to headers
                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Auto-fit columns for readability
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"CourseUploadTemplate_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}