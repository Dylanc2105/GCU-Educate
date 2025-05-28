using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static GuidanceTracker.Controllers.PostController; // Assuming this is needed

namespace GuidanceTracker.Controllers
{
    public class StudentController : Controller
    {
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
        private readonly UserManager<Student> _userManager;

        // Constructor for dependency injection of UserManager
        public StudentController()
        {
            _userManager = new UserManager<Student>(new UserStore<Student>(db));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                if (_userManager != null)
                {
                    _userManager.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        [Authorize(Roles = "Student, CurriculumHead, GuidanceTeacher")]
        // GET: Student
        public ActionResult StudentDash()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Students.Find(userId);

            // Count unread announcements
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);
            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            // ✅ Count unread messages for student
            var unreadMessagesCount = db.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .Count();

            var model = new StudentDashViewModel
            {
                FirstName = user.FirstName,
                UpcomingAppointmentsCount = db.Appointments
                .Where(a => a.StudentId == userId && a.AppointmentDate >= DateTime.Today)
                .Count(),
                NewAnnouncementsCount = newAnnouncementsCount,
                NewMessagesCount = unreadMessagesCount // ✅ Add this to the view model
            };

            return View(model);
        }

        public ActionResult UpcomingAppoinments()
        {
            var userId = User.Identity.GetUserId();

            var appointments = db.Appointments
                .Where(a => a.StudentId == userId && a.AppointmentDate >= DateTime.Today)
                .ToList();
            return View(appointments);
        }

        public ActionResult RequestAppointment()
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Find(studentId);

            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            // Get the guidance session with appointments to check availability
            GuidanceSession guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .Where(g => g.ClassId == student.ClassId)
                .FirstOrDefault();

            if (guidanceSession == null)
            {
                return HttpNotFound("No guidance session found for this student's class");
            }

            // Create view model with only available time slots
            var viewModel = new AppointmentViewModel
            {
                StudentId = studentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                AppointmentDate = guidanceSession.Day,
                GuidanceSessionId = guidanceSession.GuidanceSessionId,
                Room = guidanceSession.Room,
                AppointmentStatus = AppointmentStatus.Requested,
                // Only include available time slots
                AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestAppointment(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload available time slots
                var session = db.GuidanceSessions
                    .Include(g => g.Appointments)
                    .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(session);
                return View(model);
            }

            var studentId = User.Identity.GetUserId();
            var student = db.Students.Find(studentId);

            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            var guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

            if (guidanceSession == null)
            {
                return HttpNotFound("Guidance session not found");
            }

            // Double-check that the time slot is still available (for concurrent submissions)
            if (!guidanceSession.IsTimeSlotAvailable(model.StartTime))
            {
                ModelState.AddModelError("StartTime", "This time slot is no longer available");
                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession);
                return View(model);
            }

            // Create the appointment
            var appointment = new Appointment
            {
                StudentId = studentId,
                GuidanceSessionId = model.GuidanceSessionId,
                AppointmentDate = model.AppointmentDate,
                StartTime = model.StartTime,
                AppointmentNotes = model.AppointmentNotes,
                AppointmentStatus = AppointmentStatus.Requested,
                GuidanceTeacherId = student.Class.GuidanceTeacherId,
                Room = model.Room
            };

            db.Appointments.Add(appointment);
            db.SaveChanges();

            TempData["Success"] = "Appointment requested successfully.";
            return RedirectToAction("StudentDash");
        }

        // Helper method to get ONLY available time slots for the view
        private List<TimeSlotViewModel> GetOnlyAvailableTimeSlots(GuidanceSession session)
        {
            if (session == null)
                return new List<TimeSlotViewModel>();

            var timeSlots = new List<TimeSlotViewModel>();

            foreach (var slot in session.AllTimeSlots)
            {
                // Check if slot is available
                bool isAvailable = session.IsTimeSlotAvailable(slot);

                // Only add available time slots to the list
                if (isAvailable)
                {
                    timeSlots.Add(new TimeSlotViewModel
                    {
                        StartTime = slot,
                        EndTime = slot.Add(TimeSpan.FromMinutes(10)),
                        IsAvailable = true
                    });
                }
            }

            return timeSlots;
        }


        // Helper method to generate a unique student number
        private string GenerateUniqueStudentNumber()
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


        // GET: Student/Create (Manual Creation)
        [HttpGet]
        public ActionResult Create()
        {
            var classes = db.Classes.ToList();
            var viewModel = new CreateStudentViewModel
            {
                Classes = new SelectList(classes, "ClassId", "ClassName")
            };
            return View(viewModel);
        }

        // POST: Student/Create (Manual Creation)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateStudentViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Find the class to get its assigned GuidanceTeacher
                var assignedClass = await db.Classes
                                          .Include(c => c.GuidanceTeacher)
                                          .FirstOrDefaultAsync(c => c.ClassId == viewModel.ClassId);

                if (assignedClass == null)
                {
                    ModelState.AddModelError("ClassId", "Selected class not found.");
                }
                else if (assignedClass.GuidanceTeacherId == null)
                {
                    ModelState.AddModelError("ClassId", "The selected class does not have an assigned guidance teacher.");
                }
                else
                {
                    // Generate a unique student number (now only system-generated)
                    string generatedStudentNumber = GenerateUniqueStudentNumber();

                    // Create the Student object
                    var newStudent = new Student
                    {
                        // IdentityUser properties
                        UserName = viewModel.Email,
                        Email = viewModel.Email,
                        EmailConfirmed = true,

                        // Student-specific properties
                        FirstName = viewModel.FirstName,
                        LastName = viewModel.LastName,
                        StudentNumber = generatedStudentNumber, // System-generated
                        IsClassRep = viewModel.IsClassRep,
                        IsDeputyClassRep = viewModel.IsDeputyClassRep,
                        GuidanceTeacherId = assignedClass.GuidanceTeacherId,
                        ClassId = viewModel.ClassId,
                        RegistredAt = DateTime.UtcNow, // Set creation timestamp (use UtcNow for consistency)

                        // NEW: Address fields
                        Street = viewModel.Street,
                        City = viewModel.City,
                        Postcode = viewModel.Postcode
                    };

                    // Use UserManager to create the user and hash the password
                    var createResult = await _userManager.CreateAsync(newStudent, viewModel.Password);

                    if (createResult.Succeeded)
                    {
                        // Assign student to "Student" role
                        var roleResult = await _userManager.AddToRoleAsync(newStudent.Id, "Student");

                        if (roleResult.Succeeded)
                        {
                            TempData["Message"] = $"Student '{newStudent.FullName}' (Student No: {newStudent.StudentNumber}) created successfully!";
                            return RedirectToAction("EnrollmentAcademicOperationsCenter", "CurriculumHead");
                        }
                        else
                        {
                            // If role assignment fails, optionally delete the user that was just created
                            // to prevent an account existing without the correct role.
                            await _userManager.DeleteAsync(newStudent); // Clean up
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError("", $"Role assignment failed: {error}");
                            }
                        }
                    }
                    else
                    {
                        // Handle errors from user creation (e.g., duplicate email, weak password)
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                }
            }

            // If ModelState is not valid or there were Identity errors, repopulate classes and return to view
            var classes = db.Classes.ToList();
            viewModel.Classes = new SelectList(classes, "ClassId", "ClassName", viewModel.ClassId);
            return View(viewModel);
        }

        // GET: Student/UploadStudents
        [HttpGet]
        public ActionResult UploadStudents()
        {
            var teachers = db.GuidanceTeachers.ToList();
            var classes = db.Classes.ToList();
            var viewModel = new UploadStudentsViewModel
            {
                GuidanceTeachers = teachers,
                Classes = classes
            };
            return View(viewModel);
        }

        // POST: Student/UploadStudents
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadStudents(UploadStudentsViewModel viewModel)
        {
            if (viewModel.ExcelFile == null || viewModel.ExcelFile.ContentLength == 0)
            {
                TempData["Message"] = "Error: Please select an Excel file to upload.";
                viewModel.GuidanceTeachers = db.GuidanceTeachers.ToList();
                viewModel.Classes = db.Classes.ToList();
                return View(viewModel);
            }

            if (!Path.GetExtension(viewModel.ExcelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Message"] = "Error: Please upload a valid .xlsx Excel file.";
                viewModel.GuidanceTeachers = db.GuidanceTeachers.ToList();
                viewModel.Classes = db.Classes.ToList();
                return View(viewModel);
            }

            var excelErrors = new List<string>();
            var uploadedStudentsCount = 0;

            try
            {
                using (var package = new ExcelPackage(viewModel.ExcelFile.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["Message"] = "Error: Excel file contains no worksheets.";
                        viewModel.GuidanceTeachers = db.GuidanceTeachers.ToList();
                        viewModel.Classes = db.Classes.ToList();
                        return View(viewModel);
                    }

                    var rowCount = worksheet.Dimension.Rows;
                    var colCount = worksheet.Dimension.Columns;

                    var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int col = 1; col <= colCount; col++)
                    {
                        string headerText = worksheet.Cells[1, col].Text?.Trim();
                        if (!string.IsNullOrEmpty(headerText))
                        {
                            headerMap[headerText] = col;
                        }
                    }

                    // Define required headers for Students, now including Password
                    var requiredHeaders = new List<string> { "FirstName", "LastName", "Email", "Password", "Street", "City", "Postcode", "ClassId" };
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
                        viewModel.GuidanceTeachers = db.GuidanceTeachers.ToList();
                        viewModel.Classes = db.Classes.ToList();
                        return View(viewModel);
                    }

                    var allClasses = await db.Classes.Include(c => c.GuidanceTeacher).ToListAsync();
                    var allClassesDict = allClasses.ToDictionary(c => c.ClassId, c => c);

                    var existingStudentNumbers = await db.Students.Select(s => s.StudentNumber).ToListAsync();

                    var studentsToProcess = new List<(Student student, string password)>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var firstName = worksheet.Cells[row, headerMap["FirstName"]]?.Text?.Trim();
                        var lastName = worksheet.Cells[row, headerMap["LastName"]]?.Text?.Trim();
                        var email = worksheet.Cells[row, headerMap["Email"]]?.Text?.Trim();
                        var password = worksheet.Cells[row, headerMap["Password"]]?.Text?.Trim();
                        // StudentNumber is no longer read from Excel during upload, it's always generated if not present.
                        // However, to avoid issues if a file still *contains* the column,
                        // we'll read it and validate it if present, but always generate if empty.
                        string studentNumber = headerMap.ContainsKey("StudentNumber") ? worksheet.Cells[row, headerMap["StudentNumber"]]?.Text?.Trim() : null;

                        // Read address fields from Excel (if available)
                        var street = headerMap.ContainsKey("Street") ? worksheet.Cells[row, headerMap["Street"]]?.Text?.Trim() : null;
                        var city = headerMap.ContainsKey("City") ? worksheet.Cells[row, headerMap["City"]]?.Text?.Trim() : null;
                        var postcode = headerMap.ContainsKey("Postcode") ? worksheet.Cells[row, headerMap["Postcode"]]?.Text?.Trim() : null;


                        var classIdString = worksheet.Cells[row, headerMap["ClassId"]]?.Text?.Trim();
                        var isClassRepString = headerMap.ContainsKey("IsClassRep") ? worksheet.Cells[row, headerMap["IsClassRep"]]?.Text?.Trim() : "FALSE";
                        var isDeputyClassRepString = headerMap.ContainsKey("IsDeputyClassRep") ? worksheet.Cells[row, headerMap["IsDeputyClassRep"]]?.Text?.Trim() : "FALSE";

                        // Skip completely empty rows
                        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) &&
                            string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password) &&
                            string.IsNullOrWhiteSpace(studentNumber) && string.IsNullOrWhiteSpace(classIdString) &&
                            string.IsNullOrWhiteSpace(street) && string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(postcode))
                        {
                            continue;
                        }

                        var rowErrors = new List<string>();

                        if (string.IsNullOrWhiteSpace(firstName)) rowErrors.Add("First Name is required.");
                        if (string.IsNullOrWhiteSpace(lastName)) rowErrors.Add("Last Name is required.");
                        if (string.IsNullOrWhiteSpace(email)) rowErrors.Add("Email Address is required.");
                        else if (!IsValidEmail(email)) rowErrors.Add($"Invalid Email Address format for '{email}'.");

                        if (string.IsNullOrWhiteSpace(password)) rowErrors.Add("Password is required.");
                        else if (password.Length < 6) rowErrors.Add("Password must be at least 6 characters long."); // Basic check, UserManager will do more

                        // If studentNumber is empty or not provided, generate a new one
                        if (string.IsNullOrWhiteSpace(studentNumber))
                        {
                            studentNumber = GenerateUniqueStudentNumber();
                        }
                        // If a student number *was* provided in the file, ensure it's unique
                        else if (existingStudentNumbers.Contains(studentNumber, StringComparer.OrdinalIgnoreCase))
                        {
                            rowErrors.Add($"Student with number '{studentNumber}' already exists. Cannot upload duplicate.");
                        }

                        int classId = 0;
                        Class assignedClass = null;
                        string assignedGuidanceTeacherId = null;

                        if (string.IsNullOrWhiteSpace(classIdString))
                        {
                            rowErrors.Add("Class ID is required.");
                        }
                        else if (!int.TryParse(classIdString, out classId))
                        {
                            rowErrors.Add($"Invalid Class ID format '{classIdString}'. It must be a whole number.");
                        }
                        else if (!allClassesDict.TryGetValue(classId, out assignedClass))
                        {
                            rowErrors.Add($"Class with ID '{classIdString}' not found.");
                        }
                        else if (assignedClass.GuidanceTeacherId == null)
                        {
                            rowErrors.Add($"Class '{assignedClass.ClassName}' (ID: {classIdString}) does not have an assigned guidance teacher.");
                        }
                        else
                        {
                            assignedGuidanceTeacherId = assignedClass.GuidanceTeacherId;
                        }

                        bool isClassRep = !string.IsNullOrWhiteSpace(isClassRepString) && bool.TryParse(isClassRepString, out isClassRep) && isClassRep;
                        bool isDeputyClassRep = !string.IsNullOrWhiteSpace(isDeputyClassRepString) && bool.TryParse(isDeputyClassRepString, out isDeputyClassRep) && isDeputyClassRep;

                        if (rowErrors.Any())
                        {
                            foreach (var error in rowErrors)
                            {
                                excelErrors.Add($"Row {row}: {error}");
                            }
                            continue;
                        }

                        var newStudent = new Student
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true,
                            FirstName = firstName,
                            LastName = lastName,
                            StudentNumber = studentNumber, // Will be generated if not provided in Excel
                            IsClassRep = isClassRep,
                            IsDeputyClassRep = isDeputyClassRep,
                            GuidanceTeacherId = assignedGuidanceTeacherId,
                            ClassId = classId,
                            RegistredAt = DateTime.UtcNow, // Set creation timestamp
                            Street = street,
                            City = city,
                            Postcode = postcode
                        };

                        studentsToProcess.Add((newStudent, password));
                    }

                    // Process students using UserManager
                    foreach (var (student, pwd) in studentsToProcess)
                    {
                        var createResult = await _userManager.CreateAsync(student, pwd);
                        if (createResult.Succeeded)
                        {
                            // Assign student to "Student" role
                            var roleResult = await _userManager.AddToRoleAsync(student.Id, "Student");

                            if (roleResult.Succeeded)
                            {
                                uploadedStudentsCount++;
                                existingStudentNumbers.Add(student.StudentNumber);
                            }
                            else
                            {
                                // If role assignment fails, optionally delete the user that was just created
                                await _userManager.DeleteAsync(student); // Clean up
                                foreach (var error in roleResult.Errors)
                                {
                                    excelErrors.Add($"Failed to assign role for '{student.Email}': {error}");
                                }
                            }
                        }
                        else
                        {
                            // Handle errors from user creation (e.g., duplicate email, weak password)
                            foreach (var error in createResult.Errors)
                            {
                                excelErrors.Add($"Failed to create student '{student.Email}': {error}");
                            }
                        }
                    }

                    if (excelErrors.Any())
                    {
                        TempData["ExcelErrors"] = excelErrors;
                        TempData["Message"] = $"Successfully uploaded {uploadedStudentsCount} students. However, some errors were found (see below).";
                    }
                    else
                    {
                        TempData["Message"] = $"Successfully uploaded {uploadedStudentsCount} students.";
                    }
                }
            }
            catch (Exception ex)
            {
                excelErrors.Add($"An unexpected error occurred during upload: {ex.Message}. StackTrace: {ex.StackTrace}");
                TempData["ExcelErrors"] = excelErrors;
                TempData["Message"] = "An unexpected error occurred during Excel processing. Please check the file format.";
            }

            viewModel.GuidanceTeachers = db.GuidanceTeachers.ToList();
            viewModel.Classes = db.Classes.ToList();
            return View(viewModel);
        }

        // GET: Student/DownloadStudentTemplate
        [HttpGet]
        public ActionResult DownloadStudentTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Student Data");

                // Set headers for the template. StudentNumber column is removed.
                worksheet.Cells[1, 1].Value = "FirstName";
                worksheet.Cells[1, 2].Value = "LastName";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Password";
                worksheet.Cells[1, 5].Value = "Street";
                worksheet.Cells[1, 6].Value = "City";
                worksheet.Cells[1, 7].Value = "Postcode";
                worksheet.Cells[1, 8].Value = "IsClassRep";
                worksheet.Cells[1, 9].Value = "IsDeputyClassRep";
                worksheet.Cells[1, 10].Value = "ClassId";

                // Apply some basic styling to the header row
                using (var range = worksheet.Cells[1, 1, 1, 10]) // Adjusted column count to 10
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"StudentUploadTemplate-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}