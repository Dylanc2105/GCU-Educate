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
    
    public class TicketsController : Controller
    {
        private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        // GET: Ticket
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TicketsDetails()
        {
            return View();
        }

        public ActionResult CreateTicket()
        {
            using (var db = new GuidanceTrackerDbContext())
            {
                var classes = db.Courses
                    .Select(c => new CourseViewModel
                    {
                        CourseId = c.CourseId,
                        CourseName = c.CourseName
                    }).ToList();

                return View(classes); // Passing as a strongly typed model
            }
        }


        public JsonResult GetStudents(int courseId)
        {
            using (var db = new GuidanceTrackerDbContext())
            {
                var students = db.Users.OfType<Student>()
                    .Where(s => s.CourseId == courseId)
                    .Select(s => new
                    {
                        Id = s.Id,
                        Name = s.FirstName + " " + s.LastName,
                        StudentNumber = s.Id // Replace with actual student number field if available
                    })
                    .ToList();

                return Json(students, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetStudentTickets(string studentId)
        {
            using (var db = new GuidanceTrackerDbContext())
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    return Json(new { error = "Student ID is required" }, JsonRequestBehavior.AllowGet);
                }

                // Fetch tickets from database
                var tickets = db.Tickets
                    .Where(t => t.StudentId == studentId) // ✅ Comparing string to string
                    .ToList() // ✅ Convert to a list first (forces in-memory processing)
                    .Select(t => new
                    {
                        Id = t.TicketId,
                        Title = t.TicketTitle,
                        Description = t.TicketDescription,
                        Status = t.TicketStatus,
                        Date = t.CreatedAt.ToString("dd/MM/yyyy") // ✅ Now formatting in-memory
                    })
                    .ToList();

                return Json(tickets, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult NewTicket(string studentId, string studentName)
        {
            using (var db = new GuidanceTrackerDbContext())
            {
                // Find student
                var student = db.Students.FirstOrDefault(s => s.Id == studentId);
                if (student == null)
                {
                    return HttpNotFound("Student not found.");
                }

                // Find course
                var course = db.Courses.FirstOrDefault(c => c.CourseId == student.CourseId);

                // Fetch only modules related to the course
                var units = db.Modules
                              .Where(m => m.CourseId == student.CourseId)
                              .Select(m => new SelectListItem
                              {
                                  Value = m.ModuleId.ToString(),
                                  Text = m.ModuleName
                              })
                              .ToList();

                // Ensure ViewBag is never null
                ViewBag.StudentId = studentId;
                ViewBag.StudentName = studentName;
                ViewBag.CourseName = course?.CourseName ?? "Unknown Course";
                ViewBag.Units = units;

                // Pass an empty model to prevent null errors
                var newTicket = new Ticket
                {
                    TicketTitle = "",
                    CreatedAt = DateTime.Now,
                    TicketStatus = "Open",
                    Comments = new List<Comment>()
                };

                return View("NewTicket", newTicket);
            }
        }



        [HttpPost]
        public ActionResult AddComment(int ticketId, string content)
        {
            var userId = User.Identity.GetUserId(); // Get logged-in user ID
            var ticket = db.Tickets.Find(ticketId);

            if (ticket == null)
            {
                return HttpNotFound();
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

        [HttpPost]
        public ActionResult Create(string StudentId, string IssueType, string Description, int UnitId)
        {
            using (var db = new GuidanceTrackerDbContext())
            {
                // Check if student exists
                var student = db.Students.FirstOrDefault(s => s.Id == StudentId);
                if (student == null)
                {
                    return HttpNotFound("Student not found.");
                }

                // Create a new ticket
                var ticket = new Ticket
                {
                    TicketTitle = IssueType, // Use IssueType as the Title
                    TicketDescription = Description,
                    TicketStatus = "Open",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    StudentId = StudentId,
                    LecturerId = null, // You can modify this if needed
                    GuidanceTeacherId = null, // Modify if required
                };

                // Save to database
                db.Tickets.Add(ticket);
                db.SaveChanges();

                return Json(new { success = true, message = "Ticket successfully created!" });
            }
        }


    }
}