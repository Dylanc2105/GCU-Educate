using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml; // Make sure this is installed via NuGet (EPPlus)
using System.IO; // Required for MemoryStream
using static GuidanceTracker.Controllers.PostController; // Keep this if still needed

namespace GuidanceTracker.Controllers
{
    public class LecturerController : AccountController
    {
        public LecturerController() : base()
        {
        }
        public LecturerController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) :
            base(userManager, signInManager)
        {
        }

        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        [Authorize(Roles = "Lecturer, CurriculumHead")]
        public ActionResult LecturerDash()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Lecturers.Find(userId);

            // Count unread announcements
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);
            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            // Count active issues
            var activeIssuesCount = db.Issues
                .Where(i => (i.IssueStatus == IssueStatus.New || i.IssueStatus == IssueStatus.InProgress)
                && i.LecturerId == userId)
                .Count();

            // Count unread messages
            var unreadMessagesCount = db.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .Count();

            var model = new LecturerDashViewModel
            {
                FirstName = user.FirstName,
                ActiveIssuesCount = activeIssuesCount,
                NewAnnouncementsCount = newAnnouncementsCount,
                NewMessagesCount = unreadMessagesCount
            };

            return View(model);
        }


        public ActionResult ViewAllStudents()
        {
            var students = db.Students.OrderBy(s => s.RegistredAt).ToList();

            return View(students);
        }


        [Authorize(Roles = "CurriculumHead")]
        // GET: Lecturer/Create
        // Displays the form to create a new lecturer
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "CurriculumHead")]
        // POST: Lecturer/Create
        // Handles the submission of the lecturer creation form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery attacks
        public async Task<ActionResult> Create(CreateLecturerViewModel model)
        {
            if (ModelState.IsValid)
            {
              
                string generatedPassword = GenerateUniqueNumber(); 

                // Create a new Lecturer user based on the ViewModel data
                var lecturerUser = new Lecturer
                {
                    UserName = model.Email, // Email is used as the UserName 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    EmailConfirmed = true, // Assuming email is confirmed upon creation
                    LastName = model.LastName,
                    RegistredAt = DateTime.UtcNow, // Set the registration date to the current UTC time
                    MustChangePassword = true
                };

                // Attempt to create the user in the ASP.NET Identity system with the generated password
                var result = await UserManager.CreateAsync(lecturerUser, generatedPassword);

                if (result.Succeeded)
                {
                    // If user creation is successful, assign the "Lecturer" role
                    var roleResult = await UserManager.AddToRoleAsync(lecturerUser.Id, "Lecturer");

                    if (roleResult.Succeeded)
                    {
                        // Set a success message to display on the next view
                        TempData["Message"] = $"Lecturer '{lecturerUser.FirstName} {lecturerUser.LastName}' created successfully with generated password: {generatedPassword}";
                        // Redirect to the dashboard
                        return RedirectToAction("EnrollmentAcademicOperationsCenter", "CurriculumHead");
                    }
                    else
                    {
                        // If role assignment fails, add errors to ModelState
                        AddErrors(roleResult);
                        // Optionally, you might want to delete the user if role assignment fails
                        await UserManager.DeleteAsync(lecturerUser); // Delete user if role assignment fails
                    }
                }
                else
                {
                    // If user creation fails (e.g., duplicate email, password policy), add errors to ModelState
                    AddErrors(result);
                }
            }

            // If ModelState is not valid or user/role creation failed, re-display the form with errors
            return View(model);
        }



        // GET: Lecturer/UploadExcel
        // Displays the form to upload an Excel file for lecturer creation
        [Authorize(Roles = "CurriculumHead")] // Ensure only authorized roles can access
        public ActionResult UploadExcel()
        {
            return View();
        }

        // POST: Lecturer/UploadExcel
        // Handles the uploaded Excel file, reads data, and creates lecturers
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CurriculumHead")] // Ensure only authorized roles can access
        public async Task<ActionResult> UploadExcel(UploadLecturersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ExcelFile != null && model.ExcelFile.ContentLength > 0)
                {
                    // Validate file extension
                    var fileExtension = System.IO.Path.GetExtension(model.ExcelFile.FileName);
                    if (fileExtension != ".xlsx")
                    {
                        ModelState.AddModelError("ExcelFile", "Please upload an Excel file with .xlsx extension.");
                        return View(model);
                    }

                    List<string> successMessages = new List<string>();
                    List<string> errorMessages = new List<string>();

                    try
                    {
                        using (var package = new ExcelPackage(model.ExcelFile.InputStream))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // data is in the first worksheet
                            int rowCount = worksheet.Dimension.Rows;

                            // Map column headers to their indices dynamically
                            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                            int colCount = worksheet.Dimension.Columns;
                            for (int col = 1; col <= colCount; col++)
                            {
                                string headerText = worksheet.Cells[1, col].Text?.Trim();
                                if (!string.IsNullOrEmpty(headerText))
                                {
                                    headerMap[headerText] = col;
                                }
                            }

                            // Define expected headers for lecturers (only Email, FirstName, LastName)
                            var expectedHeaders = new List<string> { "Email", "FirstName", "LastName" };
                            foreach (var header in expectedHeaders)
                            {
                                if (!headerMap.ContainsKey(header))
                                {
                                    errorMessages.Add($"Missing required column: '{header}'. Please check your Excel file format.");
                                }
                            }

                            if (errorMessages.Any())
                            {
                                TempData["ExcelErrors"] = errorMessages; // Use TempData for displaying errors after redirect
                                TempData["Message"] = "Error: Missing required Excel columns.";
                                return RedirectToAction("UploadExcel"); // Redirect to show errors
                            }

                            // the first row is a header, start from row 2
                            for (int row = 2; row <= rowCount; row++)
                            {
                                // Skip entirely empty rows
                                if (worksheet.Cells[row, 1].Text?.Trim() == "" &&
                                        worksheet.Cells[row, 2].Text?.Trim() == "" &&
                                        worksheet.Cells[row, 3].Text?.Trim() == "") // Check only the required fields
                                {
                                    continue;
                                }

                                try
                                {
                                    // Read data from Excel columns using mapped headers
                                    var email = worksheet.Cells[row, headerMap["Email"]]?.Text?.Trim();
                                    var firstName = worksheet.Cells[row, headerMap["FirstName"]]?.Text?.Trim();
                                    var lastName = worksheet.Cells[row, headerMap["LastName"]]?.Text?.Trim();

                                    // Basic validation for required fields from Excel
                                    if (string.IsNullOrEmpty(email) ||
                                        string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                                    {
                                        errorMessages.Add($"Row {row}: Missing required data (Email, First Name, Last Name). Skipping.");
                                        continue;
                                    }

                                    // Check if email already exists
                                    if (UserManager.FindByEmail(email) != null)
                                    {
                                        errorMessages.Add($"Row {row}: Lecturer with email '{email}' already exists. Skipping.");
                                        continue;
                                    }

                                    // Generate password using the existing method
                                    string generatedPassword = GenerateUniqueNumber(); 

                                    // Create a new Lecturer user
                                    var lecturerUser = new Lecturer
                                    {
                                        UserName = email,
                                        Email = email,
                                        FirstName = firstName,
                                        LastName = lastName,
                                        // Address fields are no longer expected from Excel
                                        Street = null,
                                        City = null,
                                        Postcode = null,
                                        RegistredAt = DateTime.UtcNow,
                                        EmailConfirmed = true // Confirm email by default for uploaded users
                                    };

                                    // Attempt to create the user
                                    var createResult = await UserManager.CreateAsync(lecturerUser, generatedPassword);

                                    if (createResult.Succeeded)
                                    {
                                        // Assign the "Lecturer" role
                                        var roleResult = await UserManager.AddToRoleAsync(lecturerUser.Id, "Lecturer");
                                        if (roleResult.Succeeded)
                                        {
                                            successMessages.Add($"Successfully created lecturer: {firstName} {lastName} ({email}) with generated password.");
                                        }
                                        else
                                        {
                                            errorMessages.Add($"Row {row}: Failed to assign role for {email}. Errors: {string.Join(", ", roleResult.Errors)}");
                                            // Delete the user if role assignment fails
                                            await UserManager.DeleteAsync(lecturerUser);
                                        }
                                    }
                                    else
                                    {
                                        errorMessages.Add($"Row {row}: Failed to create user {email}. Errors: {string.Join(", ", createResult.Errors)}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errorMessages.Add($"Row {row}: An error occurred while processing: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"An error occurred while reading the Excel file: {ex.Message}");
                        TempData["ExcelErrors"] = errorMessages; // Pass existing errors
                        return View(model);
                    }

                    // Prepare a summary message for TempData
                    string summaryMessage = "";
                    if (successMessages.Count > 0)
                    {
                        summaryMessage += $"Successfully created {successMessages.Count} lecturer(s). ";
                    }
                    if (errorMessages.Count > 0)
                    {
                        summaryMessage += $"Encountered {errorMessages.Count} error(s). Please review the errors below.";
                        TempData["ExcelErrors"] = errorMessages; // Store detailed errors
                    }
                    else if (successMessages.Count == 0 && errorMessages.Count == 0)
                    {
                        summaryMessage = "No valid lecturer data found in the Excel file.";
                    }

                    TempData["Message"] = summaryMessage;
                    return RedirectToAction("UploadExcel"); // Redirect back to show messages/errors
                }
                else
                {
                    ModelState.AddModelError("ExcelFile", "No file uploaded or file is empty.");
                }
            }

            // If ModelState is not valid or file is not selected, re-display the form with errors
            return View(model);
        }


        // GET: Lecturer/DownloadLecturerTemplate
        /// <summary>
        /// Provides an Excel template file for lecturer data upload.
        /// </summary>
        /// <returns>An Excel file for download.</returns>
        [Authorize(Roles = "CurriculumHead")] // Only Curriculum Head can download this template
        public ActionResult DownloadLecturerTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Lecturer Data");

                // Set headers for the template (only Email, FirstName, LastName)
                worksheet.Cells[1, 1].Value = "Email";
                worksheet.Cells[1, 2].Value = "FirstName";
                worksheet.Cells[1, 3].Value = "LastName";

                // Apply some basic styling to the header row
                using (var range = worksheet.Cells[1, 1, 1, 3]) // Adjusted range to 3 columns
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

                string excelName = $"LecturerUploadTemplate-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }


        [Authorize(Roles = "CurriculumHead")]
        // Helper method to generate a unique student number
        private string GenerateUniqueNumber()
        {
            string newStudentNumber;
            // Loop until a unique number is found
            do
            {
                Random rand = new Random();
                string randomNumber = rand.Next(10000000, 99999999).ToString(); // 8-digit random number
                newStudentNumber = randomNumber;

            }
            while (db.Students.Any(s => s.StudentNumber == newStudentNumber));

            return newStudentNumber;
        }


        // Helper method to add IdentityResult errors to ModelState
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        // Dispose the UserManager when the controller is disposed to free resources
        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}