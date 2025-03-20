using GuidanceTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GuidanceTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;

namespace GuidanceTracker.Controllers
{
    [Authorize(Roles = "GuidanceTeacher, Lecturer")]
    public class IssueController : Controller
    {
        private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        // GET: Issues Dashboard
        public ActionResult Index()
        {
            var tickets = db.Issues.Include("Student").ToList();
            return View(tickets);
        }

        // ✅ Load the Student Issue Selection Page (For Adding Issues)
        public ActionResult StudentIssue()
        {
            var classes = db.Classes.Select(c => new ClassViewModel
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName
            }).ToList();

            return View(classes);
        }

        // ✅ View All Issues
        public ActionResult AllIssue(string sortOrder, string issueType, string searchString)
        {
            var issues = db.Issues
                           .Where(i => i.IssueStatus != IssueStatus.Archived) // Exclude archived issues
                           .AsQueryable();

            // Search dynamically by name or title
            if (!string.IsNullOrEmpty(searchString))
            {
                issues = issues.Where(i => i.Student.FirstName.Contains(searchString) ||
                                           i.Student.LastName.Contains(searchString)); 
            }

            // Filter by Issue Title (IssueType)
            if (!string.IsNullOrEmpty(issueType) && Enum.TryParse(issueType, out IssueTitle selectedIssueType))
            {
                issues = issues.Where(i => i.IssueTitle == selectedIssueType);
            }

            // Sorting (Newest first by default)
            if (sortOrder == "oldest")
            {
                issues = issues.OrderBy(i => i.CreatedAt);
            }
            else
            {
                issues = issues.OrderByDescending(i => i.CreatedAt);
            }

            return View(issues.ToList());
        }




        // ✅ View Archived Issues
        public ActionResult ArchivedIssues()
        {
            var archivedIssues = db.Issues
                                   .Where(t => t.IssueStatus == IssueStatus.Archived)
                                   .OrderByDescending(t => t.CreatedAt)
                                   .ToList();
            return View(archivedIssues);
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
                    CreatedAt = DateTime.Now,
                    UserId = userId,
                    IssueId = ticketId
                };

                db.Comments.Add(comment);
                db.SaveChanges();

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

        // GET: Load the Create Issue Page
        public ActionResult CreateIssue(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("StudentIssue");
            }

            var student = db.Users.OfType<Student>().FirstOrDefault(s => s.Id == studentId);
            if (student == null)
            {
                return RedirectToAction("StudentIssue");
            }

            var classes = db.Classes.FirstOrDefault(c => c.ClassId == student.ClassId);
            var units = db.Units.Where(m => m.UnitId == student.ClassId).ToList();

            var model = new CreateIssueViewModel
            {
                StudentId = student.Id,
                StudentName = student.FirstName + " " + student.LastName,
                ClassName = classes?.ClassName ?? "Unknown Course",
                Units = units
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateIssue(CreateIssueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Invalid form submission. Please check all fields." });
            }

            // Convert string issue type to enum
            if (!Enum.TryParse(model.IssueType, out IssueTitle issueTitle))
            {
                return Json(new { success = false, error = "Invalid Issue Type." });
            }

            // Check if the issue already exists
            var existingIssue = db.Issues.FirstOrDefault(t =>
                t.StudentId == model.StudentId &&
                t.IssueTitle == issueTitle);

            if (existingIssue != null)
            {
                return Json(new
                {
                    success = false,
                    issueId = existingIssue.IssueId,
                    error = "❌ This issue is already active for this student."
                });
            }

            // Find the selected unit and lecturer
            var selectedUnit = db.Units.FirstOrDefault(m => m.UnitId == model.SelectedUnitId);
            var lecturerId = selectedUnit?.LecturerId;

            // Create new issue
            var issue = new Issue
            {
                IssueTitle = issueTitle,
                IssueDescription = model.IssueDescription,
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                StudentId = model.StudentId,
                LecturerId = lecturerId,
                GuidanceTeacherId = User.Identity.GetUserId()
            };

            db.Issues.Add(issue);
            db.SaveChanges();

            return Json(new { success = true, message = "✅ Issue successfully created!" });
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
        }

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
                issue.UpdatedAt = DateTime.Now;

                // Save the changes
                db.SaveChanges();

                return Json(new { success = true, message = "✅ Issue reinstated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
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
                issue.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
