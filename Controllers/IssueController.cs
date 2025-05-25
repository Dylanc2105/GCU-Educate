using GuidanceTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GuidanceTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System.Data.Entity.SqlServer;
using System.Data.Entity;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using GuidanceTracker.Services;




namespace GuidanceTracker.Controllers
{
    [Authorize(Roles = "GuidanceTeacher, Lecturer")]
    public class IssueController : Controller
    {
        private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        // GET: Issues Dashboard
        public ActionResult Index()
        {
            // karina: updated the issue index action
            // index action shows only issues that the logged in user has raised(lecturer) or has been issued to(guidance teachers)
            // it also adds the issues that the lecturer leaves comments for, so they can track those too
            var currentUserId = User.Identity.GetUserId();

            // get all issue IDs the user has commented on
            var commentedIssueIds = db.Comments
                .Where(c => c.UserId == currentUserId)
                .Select(c => c.IssueId);

            // get all issues where the user is the lecturer, guidance teacher, or commented
            var issues = db.Issues
                .Include("Student")
                .Include("Lecturer")
                .Include("GuidanceTeacher")
                .Include("Comments")
                .Where(i =>
                    i.LecturerId == currentUserId ||
                    i.GuidanceTeacherId == currentUserId ||
                    commentedIssueIds.Contains(i.IssueId)
                )
                .ToList();

            return View(issues);
        }

        // GET: Issue/CreateIssue
        public ActionResult CreateIssue()
        {
            /// <summary> karina: updating the create issue action to load only the classes, units and students associated with the logged in user </summary>
            var userId = User.Identity.GetUserId();

            var model = new CreateIssueViewModel
            {
                Classes = db.Classes.ToList(),
                Units = new List<Unit>(),      // default empty so dropdown isn’t populated
                Students = new List<Student>(), // same here
                SelectedStudentIds = new List<string>() 
            };

            // check if the user is a lecturer or guidance teacher
            var lecturer = db.Lecturers
                .Include(l => l.Units.Select(u => u.Classes))
                .FirstOrDefault(l => l.Id == userId);

            if (lecturer != null)
            {
                // get unique classes from lecturer's units
                var classIds = lecturer.Units
                    .SelectMany(u => u.Classes)
                    .Select(c => c.ClassId)
                    .Distinct()
                    .ToList();

                model.Classes = db.Classes
                    .Where(c => classIds.Contains(c.ClassId))
                    .ToList();
            }
            else
            {
                var guidanceTeacher = db.GuidanceTeachers
                    .Include(gt => gt.Classes)
                    .FirstOrDefault(gt => gt.Id == userId);

                model.Classes = guidanceTeacher?.Classes.ToList() ?? new List<Class>();
            }

            return View(model);

        }

        [HttpGet]
        public JsonResult GetLinkableIssues()
        {
            var userId = User.Identity.GetUserId();

            var commentedIssueIds = db.Comments
                .Where(c => c.UserId == userId)
                .Select(c => c.IssueId);

            // Load the issues into memory first so we can use .ToString() safely
            var issues = db.Issues
                .Include(i => i.Student)
                .Where(i =>
                    i.LecturerId == userId ||
                    i.GuidanceTeacherId == userId ||
                    commentedIssueIds.Contains(i.IssueId)
                )
                .ToList() // Materialize the query here
               .Select(i => new
               {
                   IssueId = i.IssueId,
                   StudentId = i.StudentId, // ADD THIS
                   StudentName = i.Student.FirstName + " " + i.Student.LastName,
                   Title = i.IssueTitle.ToString(),
                   Status = i.IssueStatus.ToString(),
                   CreatedAt = i.CreatedAt.ToString("yyyy-MM-dd")
               })
                .ToList();
            return Json(issues, JsonRequestBehavior.AllowGet);
        }


        // POST: Issue/CreateIssue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateIssue(CreateIssueViewModel model)
        {
            if (ModelState.IsValid)
            {
                var teacherId = User.Identity.GetUserId();

                // Get full names of all selected students (for group comment)
                var studentNames = db.Students
                    .Where(s => model.SelectedStudentIds.Contains(s.Id))
                    .Select(s => s.FirstName + " " + s.LastName)
                    .ToList();

                var groupComment = "";
                if (studentNames.Count > 1)
                {
                    groupComment = "This issue was raised as part of a group involving: " + string.Join(", ", studentNames) + ".";
                }

                foreach (var studentId in model.SelectedStudentIds)
                {
                    var existingIssue = db.Issues.FirstOrDefault(i =>
                        i.StudentId == studentId &&
                        i.IssueStatus != IssueStatus.Archived &&
                        i.IssueTitle == model.IssueTitle);

                    if (existingIssue != null)
                    {
                        existingIssue.Comments.Add(new Comment
                        {
                            Content = model.Description,
                            CreatedAt = DateTime.UtcNow,
                            UserId = teacherId
                        });

                        continue; // Skip creating a new issue
                    }

                    var student = db.Students.Find(studentId);

                    var issue = new Issue
                    {
                        StudentId = student.Id,
                        IssueTitle = model.IssueTitle,
                        IssueDescription = model.Description,
                        IssueStatus = IssueStatus.New,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LecturerId = student.Class.Units.FirstOrDefault()?.LecturerId,
                        GuidanceTeacherId = student.GuidanceTeacherId, // karina: issues are correctly assigned to the guidance teacher
                        Comments = new List<Comment>()
                    };

                    // Add the group comment if multiple students are selected
                    if (!string.IsNullOrEmpty(groupComment))
                    {
                        issue.Comments.Add(new Comment
                        {
                            Content = groupComment,
                            CreatedAt = DateTime.UtcNow,
                            UserId = teacherId
                        });
                    }

                    // Add the main description as a comment
                    issue.Comments.Add(new Comment
                    {
                        Content = model.Description,
                        CreatedAt = DateTime.UtcNow,
                        UserId = teacherId
                    });

                    db.Issues.Add(issue);
                }

                db.SaveChanges();

                // send notification
                new NotificationService().NotifyGuidanceForClassIssues
                (
                    model.SelectedStudentIds,
                    User.Identity.GetUserId()
                );

                return RedirectToAction("StudentIssue");
            }

            // Repopulate dropdowns if model is invalid
            model.Classes = db.Classes.ToList();
            model.Units = db.Units.ToList();
            model.Students = db.Students.ToList();
            return View(model);
        }



        // AJAX: Get Units by Class
        public ActionResult GetUnits(int classId)
        {
            // Get the UnitIds manually from the join table (UnitClasses)
            var unitIds = db.Database.SqlQuery<int>(
                "SELECT UnitId FROM UnitClasses WHERE ClassId = @p0", classId).ToList();

            var units = db.Units
                .Where(u => unitIds.Contains(u.UnitId))
                .Select(u => new { u.UnitId, u.UnitName })
                .ToList();

            return Json(units, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DebugUnits(int classId)
        {
            var classEntity = db.Classes
                .Include(c => c.Units)
                .FirstOrDefault(c => c.ClassId == classId);

            if (classEntity == null)
            {
                return Content("Class not found.");
            }

            var output = $"Class: {classEntity.ClassName}<br/>Units:<ul>";

            foreach (var unit in classEntity.Units)
            {
                output += $"<li>{unit.UnitName} (ID: {unit.UnitId})</li>";
            }

            output += "</ul>";

            return Content(output);
        }


        // AJAX: Get Students by Class
        public ActionResult GetStudentsForClass(int classId)

        {
            var students = db.Students
                             .Where(s => s.ClassId == classId)
                             .Select(s => new { s.Id, FullName = s.FirstName + " " + s.LastName });

            return Json(students, JsonRequestBehavior.AllowGet);
        }


        // Load the Student Issue Selection Page (For Adding Issues)
        public ActionResult StudentIssue()
        {
            /// <summary>   karina: updating the action to load only the classes associated with the logged in user
            /// get classes from the database
            /// </summary>
            
            var userId = User.Identity.GetUserId();
            List<ClassViewModel> classList = new List<ClassViewModel>();
            /// <summary> karina: get the lecturer and include the units tehy teach </summary>
            var lecturer = db.Lecturers
                .Include(l => l.Units.Select(u => u.Classes))
                .FirstOrDefault(l => l.Id == userId);

            /// <summary> karina: if the lecturer is not null, get the classes from their units </summary>
            if (lecturer != null)
            {
                var classIds = lecturer.Units
                    .SelectMany(u => u.Classes)
                    .Select(c => c.ClassId)
                    .Distinct();

                classList = db.Classes
                    .Where(c => classIds.Contains(c.ClassId))
                    .Select(c => new ClassViewModel
                    {
                        ClassId = c.ClassId,
                        ClassName = c.ClassName
                    })
                    .ToList();
            }
            else
            {
                /// <summary> karina: if the user is a guidance teacher, get their classes </summary>
                var guidanceTeacher = db.GuidanceTeachers
                    .Include(gt => gt.Classes)
                    .FirstOrDefault(gt => gt.Id == userId);

                if (guidanceTeacher != null)
                {
                    classList = guidanceTeacher.Classes
                        .Select(c => new ClassViewModel
                        {
                            ClassId = c.ClassId,
                            ClassName = c.ClassName
                        })
                        .ToList();
                }
            }

            return View(classList);
        }

        public ActionResult AllIssues(string sortOrder, string issueType, string searchString)
        {
            var issuesQuery = db.Issues
                .Include("Lecturer")  // Use string-based Include to load Lecturer
                .Include("Student")   // Use string-based Include to load Student
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // SQL LIKE pattern with '%' for partial matching
                var searchPattern = "%" + searchString.ToLower() + "%";

                issuesQuery = issuesQuery.Where(i =>
                    SqlFunctions.PatIndex(searchPattern, i.Student.FirstName.ToLower()) > 0 ||
                    SqlFunctions.PatIndex(searchPattern, i.Student.LastName.ToLower()) > 0 ||
                    SqlFunctions.PatIndex(searchPattern, i.Lecturer.FirstName.ToLower()) > 0 ||
                    SqlFunctions.PatIndex(searchPattern, i.Lecturer.LastName.ToLower()) > 0);
            }

            // Apply filter for issue type
            if (!string.IsNullOrEmpty(issueType))
            {
                issuesQuery = issuesQuery.Where(i => i.IssueTitle.ToString() == issueType);
            }

            // Sorting
            if (sortOrder == "newest")
            {
                issuesQuery = issuesQuery.OrderByDescending(i => i.CreatedAt);
            }
            else if (sortOrder == "oldest")
            {
                issuesQuery = issuesQuery.OrderBy(i => i.CreatedAt);
            }

            // Execute the query and return the results
            var issues = issuesQuery.ToList();
            return View(issues);
        }



        //View Archived Issues
        public ActionResult ArchivedIssues(string sortOrder, string issueType, string searchString)
        {
            var issuesQuery = db.Issues
                .Include("Lecturer")
                .Include("Student")
                .Where(i => i.IssueStatus == IssueStatus.Archived)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var searchPattern = "%" + searchString.ToLower() + "%";

                issuesQuery = issuesQuery.Where(i =>
                    SqlFunctions.PatIndex(searchPattern, i.Student.FirstName.ToLower()) > 0 ||
                    SqlFunctions.PatIndex(searchPattern, i.Student.LastName.ToLower()) > 0 ||
                    SqlFunctions.PatIndex(searchPattern, i.Lecturer.FirstName.ToLower()) > 0 ||
                    SqlFunctions.PatIndex(searchPattern, i.Lecturer.LastName.ToLower()) > 0);
            }

            if (!string.IsNullOrEmpty(issueType))
            {
                issuesQuery = issuesQuery.Where(i => i.IssueTitle.ToString() == issueType);
            }

            if (sortOrder == "newest")
            {
                issuesQuery = issuesQuery.OrderByDescending(i => i.CreatedAt);
            }
            else if (sortOrder == "oldest")
            {
                issuesQuery = issuesQuery.OrderBy(i => i.CreatedAt);
            }

            var issues = issuesQuery.ToList();
            return View(issues);
        }

        public List<Appointment> GetAppointments(string studentId)
        {
            var appointments = db.Appointments.Where(s => s.StudentId == studentId).ToList();
            return appointments;
        }

        public ActionResult ViewIssue(int id)
        {
            var ticket = db.Issues
                .Include("Student")
                .Include("Comments") // Ensure comments are included
                .FirstOrDefault(t => t.IssueId == id);


            /// <summary> karina: gets the current user id and if its a lecturer gets the associated units that the they teach for the student. </summary>
            var currentUserId = User.Identity.GetUserId();
            if (User.IsInRole("Lecturer"))
            {
                ViewBag.LecturerUnits = db.Units
                    .Where(u => u.LecturerId == currentUserId &&
                     u.Classes.Any(c => c.ClassId == ticket.Student.ClassId))
                    .ToList();
            }

            var appointments = GetAppointments(ticket.StudentId);

            if (appointments != null)
            {
                ViewBag.Data = appointments;
            }

            if (ticket == null)
            {
                return HttpNotFound();
            }

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddComment(int ticketId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, error = "Comment cannot be empty." });
                }

                string userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);

                if (user == null)
                {
                    return Json(new { success = false, error = "User not found." });
                }

                var ticket = db.Issues.Find(ticketId);
                if (ticket == null)
                {
                    return Json(new { success = false, error = "Issue not found." });
                }

                var comment = new Comment
                {
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    IssueId = ticketId
                };

                db.Comments.Add(comment);
                db.SaveChanges();

                // notify
                var issue = db.Issues.Include("Student").FirstOrDefault(i => i.IssueId == ticketId);
                new NotificationService().NotifyNewComment(issue, userId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Get Students for a Selected Course
        public JsonResult GetStudents(int classId)
        {
            try
            {
                if (classId == 0)
                {
                    return Json(new { error = "Invalid Course ID" }, JsonRequestBehavior.AllowGet);
                }

                var students = db.Users.OfType<Student>()
                    .Where(s => s.ClassId == classId)
                    .Select(s => new
                    {
                        Id = s.Id,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();

                return Json(students, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

       

        [HttpGet]
        public JsonResult GetStudentIssues(string studentId)
        {
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    return Json(new { error = "Student ID is required" }, JsonRequestBehavior.AllowGet);
                }

                var issues = db.Issues
                    .Where(i => i.StudentId == studentId)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToList()
                    .Select(i => new
                    {
                        TicketId = i.IssueId,
                        TicketTitle = i.IssueTitle.ToString(), // Convert Enum to String
                        TicketDescription = i.IssueDescription ?? "No description available",
                        TicketStatus = i.IssueStatus.ToString(), // Convert Enum to String
                        CreatedAt = i.CreatedAt.ToString("dd/MM/yyyy")
                    })
                    .ToList();

                if (!issues.Any())
                {
                    return Json(new { error = "No issues found for this student." }, JsonRequestBehavior.AllowGet);
                }

                return Json(issues, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }// GET: Create New Issue Page (with Class, Unit, and Student Selection)

        [HttpPost]
        public JsonResult ReinstateIssue(int issueId)
        {
            try
            {
                var issue = db.Issues.Find(issueId);
                if (issue == null)
                {
                    return Json(new { success = false, error = "Issue not found." });
                }

                // Set the issue status to "New" or "InProgress"
                issue.IssueStatus = IssueStatus.New; // Or use InProgress if needed
                issue.UpdatedAt = DateTime.UtcNow;

                // Save the changes
                db.SaveChanges();

                return Json(new { success = true, message = "✅ Issue reinstated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetUnitsByClass(int classId)
        {
            try
            {
                var units = db.Units
                    .Where(u => u.Classes.Any(c => c.ClassId == classId))
                    .Select(u => new
                    {
                        UnitId = u.UnitId,
                        UnitName = u.UnitName
                    })
                    .ToList();

                return Json(units, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public JsonResult UpdateIssueStatus(int issueId, string status)
        {
            try
            {
                if (!Enum.TryParse(status, out IssueStatus newStatus))
                {
                    return Json(new { success = false, error = "Invalid Issue Status" });
                }

                var issue = db.Issues.Find(issueId);
                if (issue == null)
                {
                    return Json(new { success = false, error = "Issue not found" });
                
                }

                issue.IssueStatus = newStatus;
                issue.UpdatedAt = DateTime.UtcNow;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// karina: method to add to an existing issue for lecturers
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public JsonResult AddLecturerComment(int issueId, string content, int unitId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, error = "Comment cannot be empty." });
                }

                if (unitId == 0)
                {
                    return Json(new { success = false, error = "Unit must be selected." });
                }

                string userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);

                if (user == null)
                {
                    return Json(new { success = false, error = "User not found." });
                }

                var issue = db.Issues.Find(issueId);
                if (issue == null)
                {
                    return Json(new { success = false, error = "Issue not found." });
                }

                /// <summary> get the unit information </summary>
                var unit = db.Units.Find(unitId);
                if (unit == null)
                {
                    return Json(new { success = false, error = "Unit not found." });
                }


                /// <summary> create a comment for unit information </summary>
                var comment = new Comment
                {
                    Content = $"Related to {unit.UnitName}: {content}",
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    IssueId = issueId
                };

                db.Comments.Add(comment);
                db.SaveChanges();

                /// <summary> notifications </summary>
                new NotificationService().NotifyNewComment(issue, userId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }


    }
}
