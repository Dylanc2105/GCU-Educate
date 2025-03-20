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
            var tickets = db.Tickets.Include("Student").ToList();
            return View(tickets);
        }

        // View All Issues
        public ActionResult AllIssue(string sortOrder, string issueType, string searchString)
        {
            var issues = db.Tickets
                           .Where(t => t.TicketStatus != "Archived") // Exclude archived issues
                           .AsQueryable();

            // Search dynamically by name or title
            if (!string.IsNullOrEmpty(searchString))
            {
                issues = issues.Where(t => t.TicketTitle.Contains(searchString) ||
                                           t.Student.FirstName.Contains(searchString) ||
                                           t.Student.LastName.Contains(searchString));
            }

            // Filter by Issue Type (e.g., Late Attendance, Behaviour, etc.)
            if (!string.IsNullOrEmpty(issueType))
            {
                issues = issues.Where(t => t.IssueType == issueType);
            }

            // Sorting (Newest first by default)
            if (sortOrder == "oldest")
            {
                issues = issues.OrderBy(t => t.CreatedAt);
            }
            else
            {
                issues = issues.OrderByDescending(t => t.CreatedAt);
            }

            return View(issues.ToList());
        }

        // View Archived Issues
        public ActionResult ArchivedIssues()
        {
            var archivedIssues = db.Tickets.Where(t => t.TicketStatus == "Archived").OrderByDescending(t => t.CreatedAt).ToList();
            return View(archivedIssues);
        }
        public ActionResult ViewIssue(int id)
        {
            var ticket = db.Tickets
                .Include("Student")
                .Include("Comments") // Make sure it includes comments
                .FirstOrDefault(t => t.TicketId == id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            return View(ticket); // This should return the ViewIssue page
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

                var ticket = db.Tickets.Find(ticketId);
                if (ticket == null)
                {
                    return Json(new { success = false, error = "Ticket not found." });
                }

                var comment = new Comment
                {
                    Content = content,
                    CreatedAt = DateTime.Now,
                    UserId = userId,
                    TicketId = ticketId
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

        // View for Selecting Student Issue
        public ActionResult StudentIssue()
        {
            var classes = db.Classes.Select(c => new ClassViewModel
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName
            }).ToList();

            return View(classes);
        }

        // ✅ Get Students for a Selected Course
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
                        Name = s.FirstName + " " + s.LastName,
                        StudentNumber = s.Id // Change to actual StudentNumber field if applicable
                    })
                    .ToList();

                if (!students.Any())
                {
                    return Json(new { error = "No students found for this course." }, JsonRequestBehavior.AllowGet);
                }

                return Json(students, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ✅ Get Issues for a Selected Student
        public JsonResult GetStudentIssues(string studentId)
        {
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    return Json(new { error = "Student ID is required" }, JsonRequestBehavior.AllowGet);
                }

                // Retrieve issues from the database
                var issues = db.Tickets
                    .Where(t => t.StudentId == studentId)
                    .ToList() // ✅ Fetch the data first (prevents LINQ to Entities error)
                    .Select(t => new
                    {
                        TicketId = t.TicketId,
                        TicketTitle = t.TicketTitle ?? "Untitled Issue",
                        TicketDescription = t.TicketDescription ?? "No description available",
                        TicketStatus = t.TicketStatus ?? "Unknown",
                        CreatedAt = t.CreatedAt.ToString("dd/MM/yyyy") // ✅ Format AFTER data is retrieved
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

        // ✅ GET: Load the Create Issue Page
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

            // Check if the issue already exists for the selected student
            var existingIssue = db.Tickets.FirstOrDefault(t =>
                t.StudentId == model.StudentId &&
                t.TicketTitle == model.IssueType);

            if (existingIssue != null)
            {
                if (existingIssue.TicketStatus == "Archived")
                {
                    return Json(new
                    {
                        success = false,
                        archived = true,
                        issueId = existingIssue.TicketId,
                        error = "❌ This issue already exists for this student but is archived. Please reinstate and add comments."
                    });
                }

                return Json(new
                {
                    success = false,
                    archived = false,
                    issueId = existingIssue.TicketId,
                    error = "❌ This issue is already active for this student. Please add comments to the existing issue."
                });
            }

            // Find the selected unit and its lecturer
            var selectedUnit = db.Units.FirstOrDefault(m => m.UnitId == model.SelectedUnitId);
            var lecturerId = selectedUnit?.LecturerId;

            // Create new issue
            var issueTitle = model.IssueType == "Custom" ? model.CustomIssue : model.IssueType;
            var issue = new Issue
            {
                TicketTitle = issueTitle,
                TicketDescription = model.IssueDescription,
                TicketStatus = "Open",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                StudentId = model.StudentId,
                LecturerId = lecturerId,
                GuidanceTeacherId = User.Identity.GetUserId()
            };

            db.Tickets.Add(issue);
            db.SaveChanges();

            return Json(new { success = true, message = "✅ Issue successfully created!" });
        }



        [HttpPost]
        public JsonResult UpdateIssueStatus(int issueId, string status)
        {
            try
            {
                if (issueId == 0 || string.IsNullOrEmpty(status))
                {
                    return Json(new { success = false, error = "Invalid Issue ID or Status" });
                }

                var issue = db.Tickets.Find(issueId);
                if (issue == null)
                {
                    return Json(new { success = false, error = "Issue not found" });
                }

                issue.TicketStatus = status;
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
