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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static GuidanceTracker.Controllers.PostController; // Assuming PostController exists and PostVisibilityHelper is public/static

namespace GuidanceTracker.Controllers
{
    [Authorize(Roles = "Student, CurriculumHead, GuidanceTeacher")]
    public class StudentController : Controller
    {
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
        private readonly UserManager<Student> _userManager;

        public StudentController()
        {
            _userManager = new UserManager<Student>(new UserStore<Student>(db));
            // IMPORTANT: Configure PasswordValidator to allow 8-digit numbers if StudentNumber is used as password.
            _userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8, // Student Number must be 8 digits
                RequireNonLetterOrDigit = false, // Allow only digits
                RequireDigit = true,
                RequireLowercase = false,
                RequireUppercase = false,
            };
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

        // GET: Student
        public ActionResult StudentDash()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Students.Find(userId);

            if (user == null)
            {
                // Handle case where student user is not found (e.g., redirect to logout or error)
                return RedirectToAction("Login", "Account");
            }

            // Count unread announcements
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User); // Assuming PostVisibilityHelper is accessible
            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            // Count unread messages for student
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
                NewMessagesCount = unreadMessagesCount
            };

            return View(model);
        }

        // GET: Student/UpcomingAppoinments
        public ActionResult UpcomingAppoinments()
        {
            var userId = User.Identity.GetUserId();

            var appointments = db.Appointments
                .Where(a => a.StudentId == userId && a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToList();
            return View(appointments);
        }

        // GET: Student/RequestAppointment
        public ActionResult RequestAppointment()
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Include(s => s.Class).FirstOrDefault(s => s.Id == studentId);

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
                // You might want a more user-friendly message for the student
                TempData["ErrorMessage"] = "No guidance session found for your class. Please contact your teacher.";
                return RedirectToAction("StudentDash"); // Redirect to dashboard or another appropriate page
            }

            // Create view model with only available time slots
            var viewModel = new AppointmentViewModel
            {
                StudentId = studentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                AppointmentDate = guidanceSession.Day,
                GuidanceSessionId = guidanceSession.GuidanceSessionId,
                Room = guidanceSession.Room,
                AppointmentStatus = AppointmentStatus.Requested, // Set default status
                // Only include available time slots
                AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestAppointment(AppointmentViewModel model)
        {
            // Reload available time slots if ModelState is invalid to ensure dropdown is populated
            if (!ModelState.IsValid)
            {
                var session = db.GuidanceSessions
                    .Include(g => g.Appointments)
                    .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(session);
                return View(model);
            }

            var studentId = User.Identity.GetUserId();
            var student = db.Students.Include(s => s.Class).FirstOrDefault(s => s.Id == studentId);


            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            var guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

            if (guidanceSession == null)
            {
                ModelState.AddModelError("", "Guidance session not found.");
                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(null); // Pass null to clear slots
                return View(model);
            }

            // Double-check that the time slot is still available (for concurrent submissions)
            if (!guidanceSession.IsTimeSlotAvailable(model.StartTime))
            {
                ModelState.AddModelError("StartTime", "This time slot is no longer available. Please choose another.");
                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession);
                return View(model);
            }

            // Check if the student already has an appointment for this session/day to prevent multiple bookings
            if (db.Appointments.Any(a => a.StudentId == studentId && a.GuidanceSessionId == model.GuidanceSessionId && a.AppointmentDate == model.AppointmentDate && a.AppointmentStatus != AppointmentStatus.Cancelled))
            {
                ModelState.AddModelError("", "You already have an appointment booked for this guidance session.");
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
                AppointmentStatus = AppointmentStatus.Requested, // Default status for student requests
                GuidanceTeacherId = student.Class.GuidanceTeacherId, // Assign to the teacher of the student's class
                Room = model.Room // Take room from the guidance session model
            };

            db.Appointments.Add(appointment);
            db.SaveChanges();

            TempData["Success"] = "Appointment requested successfully. Your guidance teacher will review it.";
            return RedirectToAction("StudentDash");
        }

        // Helper method to get ONLY available time slots for the view
        private List<TimeSlotViewModel> GetOnlyAvailableTimeSlots(GuidanceSession session)
        {
            if (session == null)
                return new List<TimeSlotViewModel>();

            var timeSlots = new List<TimeSlotViewModel>();

            // Assuming GuidanceSession has a property like AllTimeSlots which returns a list of TimeSpans
            // representing all possible slots for that session.
            // If AllTimeSlots doesn't exist, you'll need to generate them based on StartTime, EndTime, and SlotDuration.
            foreach (var slot in session.AllTimeSlots)
            {
                // Check if slot is available (i.e., no existing appointment for this slot in this session)
                bool isAvailable = !session.Appointments.Any(a => a.StartTime == slot);

                // Only add available time slots to the list
                if (isAvailable)
                {
                    timeSlots.Add(new TimeSlotViewModel
                    {
                        StartTime = slot,
                        EndTime = slot.Add(TimeSpan.FromMinutes(10)), // Assuming 10-minute slots
                        IsAvailable = true
                    });
                }
            }

            return timeSlots;
        }


        // GET: Student/Create (Manual Creation)
        [Authorize(Roles = "CurriculumHead, GuidanceTeacher")]
        [HttpGet]
        public ActionResult Create()
        {
            var classes = db.Classes.Select(c => new SelectListItem
            {
                Value = c.ClassId.ToString(),
                Text = c.ClassName
            }).ToList();

            var viewModel = new CreateStudentViewModel
            {
                Classes = classes
            };
            return View(viewModel);
        }

        // POST: Student/Create (Manual Creation)
        [Authorize(Roles = "CurriculumHead, GuidanceTeacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateStudentViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if Student Number is already taken
                if (await db.Students.AnyAsync(s => s.StudentNumber == viewModel.StudentNumber))
                {
                    ModelState.AddModelError("StudentNumber", "This Student Number is already taken. Please use a unique one.");
                }
                // Check if Email is already in use
                if (await _userManager.FindByEmailAsync(viewModel.Email) != null)
                {
                    ModelState.AddModelError("Email", "A user with this email address already exists.");
                }

                if (ModelState.IsValid) // Re-check ModelState after custom validations
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
                        var newStudent = new Student
                        {
                            UserName = viewModel.Email,
                            Email = viewModel.Email,
                            EmailConfirmed = true,
                            PhoneNumber = viewModel.PhoneNumber,
                            FirstName = viewModel.FirstName,
                            LastName = viewModel.LastName,
                            StudentNumber = viewModel.StudentNumber,
                            IsClassRep = false, // Default to false
                            IsDeputyClassRep = false, // Default to false
                            GuidanceTeacherId = assignedClass.GuidanceTeacherId,
                            ClassId = viewModel.ClassId,
                            RegistredAt = DateTime.UtcNow,
                            Street = viewModel.Address, // Map ViewModel.Address to Student.AddressLine1
                            MustChangePassword = true
                        };

                        // Use UserManager to create the user and hash the password
                        var createResult = await _userManager.CreateAsync(newStudent, viewModel.StudentNumber);

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
                                await _userManager.DeleteAsync(newStudent);
                                foreach (var error in roleResult.Errors)
                                {
                                    ModelState.AddModelError("", $"Role assignment failed: {error}");
                                }
                            }
                        }
                        else
                        {
                            foreach (var error in createResult.Errors)
                            {
                                ModelState.AddModelError("", error);
                            }
                        }
                    }
                }
            }

            // If ModelState is not valid or there were Identity errors, repopulate classes and return to view
            var classes = db.Classes.Select(c => new SelectListItem
            {
                Value = c.ClassId.ToString(),
                Text = c.ClassName
            }).ToList();
            viewModel.Classes = classes; // Assign repopulated classes
            return View(viewModel);
        }

        // GET: Student/UploadStudents
        [Authorize(Roles = "CurriculumHead, GuidanceTeacher")]
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
        [Authorize(Roles = "CurriculumHead, GuidanceTeacher")]
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

                    // Define required headers for Students
                    var requiredHeaders = new List<string> { "FirstName", "LastName", "Email", "StudentNumber", "Address", "PhoneNumber", "ClassId" };
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

                    // Check for existing StudentNumbers and Emails in the database
                    var existingStudentNumbers = new HashSet<string>(await db.Students.Select(s => s.StudentNumber).ToListAsync(), StringComparer.OrdinalIgnoreCase);
                    var existingEmails = new HashSet<string>(await db.Students.Select(s => s.Email).ToListAsync(), StringComparer.OrdinalIgnoreCase);

                    // Use temporary hashsets to check for duplicates within the same uploaded batch
                    var studentNumbersInCurrentBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var emailsInCurrentBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    var studentsToProcess = new List<Student>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var firstName = worksheet.Cells[row, headerMap["FirstName"]]?.Text?.Trim();
                        var lastName = worksheet.Cells[row, headerMap["LastName"]]?.Text?.Trim();
                        var email = worksheet.Cells[row, headerMap["Email"]]?.Text?.Trim();
                        var studentNumber = worksheet.Cells[row, headerMap["StudentNumber"]]?.Text?.Trim();
                        var addressLine1 = worksheet.Cells[row, headerMap["Address"]]?.Text?.Trim();
                        var phoneNumber = worksheet.Cells[row, headerMap["PhoneNumber"]]?.Text?.Trim();

                        var classIdString = worksheet.Cells[row, headerMap["ClassId"]]?.Text?.Trim();

                        // Skip completely empty rows
                        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) &&
                            string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(studentNumber) &&
                            string.IsNullOrWhiteSpace(addressLine1) && string.IsNullOrWhiteSpace(phoneNumber) &&
                            string.IsNullOrWhiteSpace(classIdString))
                        {
                            continue;
                        }

                        var rowErrors = new List<string>();

                        if (string.IsNullOrWhiteSpace(firstName)) rowErrors.Add("First Name is required.");
                        if (string.IsNullOrWhiteSpace(lastName)) rowErrors.Add("Last Name is required.");

                        if (string.IsNullOrWhiteSpace(email)) rowErrors.Add("Email Address is required.");
                        else if (!IsValidEmail(email)) rowErrors.Add($"Invalid Email Address format for '{email}'.");
                        else if (existingEmails.Contains(email)) rowErrors.Add($"Email '{email}' already exists in the system.");
                        else if (emailsInCurrentBatch.Contains(email)) rowErrors.Add($"Duplicate Email '{email}' found in current upload batch.");

                        if (string.IsNullOrWhiteSpace(studentNumber)) rowErrors.Add("Student Number is required.");
                        // Specific validation for 8-digit student number
                        else if (!Regex.IsMatch(studentNumber, @"^\d{8}$")) rowErrors.Add($"Student Number '{studentNumber}' must be exactly an 8-digit number.");
                        else if (existingStudentNumbers.Contains(studentNumber)) rowErrors.Add($"Student with number '{studentNumber}' already exists in the system.");
                        else if (studentNumbersInCurrentBatch.Contains(studentNumber)) rowErrors.Add($"Duplicate Student Number '{studentNumber}' found in current upload batch.");

                        if (string.IsNullOrWhiteSpace(addressLine1)) rowErrors.Add("Address is required.");
                        if (string.IsNullOrWhiteSpace(phoneNumber)) rowErrors.Add("Phone Number is required.");

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

                        if (rowErrors.Any())
                        {
                            foreach (var error in rowErrors)
                            {
                                excelErrors.Add($"Row {row}: {error}");
                            }
                            continue;
                        }

                        // Add to batch sets only after all checks pass for the row
                        studentNumbersInCurrentBatch.Add(studentNumber);
                        emailsInCurrentBatch.Add(email);

                        var newStudent = new Student
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true,
                            PhoneNumber = phoneNumber,
                            FirstName = firstName,
                            LastName = lastName,
                            StudentNumber = studentNumber,
                            IsClassRep = false, // Default to false
                            IsDeputyClassRep = false, // Default to false
                            GuidanceTeacherId = assignedGuidanceTeacherId,
                            ClassId = classId,
                            RegistredAt = DateTime.UtcNow,
                            Street = addressLine1
                        };

                        studentsToProcess.Add(newStudent);
                    }

                    // Process students using UserManager
                    foreach (var student in studentsToProcess)
                    {
                        var createResult = await _userManager.CreateAsync(student, student.StudentNumber);
                        if (createResult.Succeeded)
                        {
                            var roleResult = await _userManager.AddToRoleAsync(student.Id, "Student");

                            if (roleResult.Succeeded)
                            {
                                uploadedStudentsCount++;
                                existingStudentNumbers.Add(student.StudentNumber); // Update master set
                                existingEmails.Add(student.Email); // Update master set
                            }
                            else
                            {
                                await _userManager.DeleteAsync(student);
                                foreach (var error in roleResult.Errors)
                                {
                                    excelErrors.Add($"Failed to assign role for '{student.Email}': {error}");
                                }
                            }
                        }
                        else
                        {
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
        [Authorize(Roles = "CurriculumHead, GuidanceTeacher")]
        [HttpGet]
        public ActionResult DownloadStudentTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Student Data");

                // Set headers for the template
                worksheet.Cells[1, 1].Value = "FirstName";
                worksheet.Cells[1, 2].Value = "LastName";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "StudentNumber";
                worksheet.Cells[1, 5].Value = "Address";
                worksheet.Cells[1, 6].Value = "PhoneNumber";
                worksheet.Cells[1, 7].Value = "ClassId";

                // Apply some basic styling to the header row
                using (var range = worksheet.Cells[1, 1, 1, 7])
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