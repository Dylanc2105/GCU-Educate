using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using GuidanceTracker.Models.ViewModels.FeedbackViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    public class SimpleFeedbackController : Controller
    {
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        public ActionResult StudentFeedbackDashboard()
        {
            var studentId = User.Identity.GetUserId();

            var currentStudent = db.Students.FirstOrDefault(s => s.Id == studentId);

            ViewBag.IsClassRep = currentStudent?.IsClassRep ?? false;

            if (currentStudent?.IsClassRep == true)
            {
                // Check if all students have submitted feedback (meaning class rep can review)
                var totalStudentsInClass = db.Students.Count(s => s.ClassId == currentStudent.ClassId);
                var submittedFeedbacks = db.DetailedFeedbacks
                    .Count(f => f.ClassId == currentStudent.ClassId && f.IsSubmitted == true);
                var pendingRequests = db.RequestedDetailedForms
                    .Count(r => r.ClassId == currentStudent.ClassId);

                // Check if class rep has already submitted their aggregated feedback
                var submittedClassRepFeedback = db.ClassRepFeedbacks
                    .Any(f => f.ClassId == currentStudent.ClassId && f.StudentId == currentStudent.Id && f.IsSubmittedByClassRep == true);

                ViewBag.HasPendingClassFeedback = (submittedFeedbacks >= totalStudentsInClass && pendingRequests == 0 && !submittedClassRepFeedback);
                ViewBag.HasSubmittedClassFeedback = submittedClassRepFeedback;
            }

            // Check if there are any requests for this specific student
            var hasRequest = db.RequestedDetailedForms
                .Any(r => r.StudentId == studentId);

            ViewBag.IsClassRep = currentStudent?.IsClassRep ?? false;

            if (hasRequest)
            {
                var firstRequest = db.RequestedDetailedForms
                    .FirstOrDefault(r => r.StudentId == studentId);

                ViewBag.HasDetailedFeedbackRequest = true;
                ViewBag.FirstRequestId = firstRequest?.RequestId;
            }
            else
            {
                ViewBag.HasDetailedFeedbackRequest = false;
            }

            return View();
        }


        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult CreateSimpleFeedback()
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Include(s => s.Class).FirstOrDefault(s => s.Id == studentId);

            // Load the student and their relationships
            student = db.Students
                .Include("GuidanceTeacher")
                .Include("Class.Enrollments.Course.Department.CurriculumHead")
                .FirstOrDefault(s => s.Id == studentId);

            var viewModel = new SimpleStudentFeedbackViewModel
            {
                SendToGuidanceTeacher = false,
                SendToCurriculumHead = false
            };

            if (student.GuidanceTeacher != null)
            {
                viewModel.GuidanceTeacherId = student.GuidanceTeacherId;
                viewModel.GuidanceTeacherName = $"{student.GuidanceTeacher.FirstName} {student.GuidanceTeacher.LastName}";
            }

            var ch = student.Class?.Enrollments.FirstOrDefault()?.Course?.Department?.CurriculumHead;
            if (ch != null)
            {
                viewModel.CurriculumHeadId = ch.Id;
                viewModel.CurriculumHeadName = $"{ch.FirstName} {ch.LastName}";
            }
            var studentUnits = db.Units
                    .Where(u => u.Classes.Any(c => c.Students.Any(s => s.Id == student.Id)))
                    .ToList();

            viewModel.Units = studentUnits.Select(u => new SelectListItem
            {


                Value = u.UnitId.ToString(),
                Text = u.UnitName
            }).ToList();
            return View(viewModel);

        }

        [HttpPost]
        public ActionResult CreateSimpleFeedback(SimpleStudentFeedbackViewModel viewModel)
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Include(s => s.Class).FirstOrDefault(s => s.Id == studentId);
            if (ModelState.IsValid)
            {

                // Create a single feedback record
                var feedback = new SimpleFeedback
                {
                    FeedbackTitle = viewModel.Title,
                    FeedbackContent = viewModel.content,
                    DateOfCreation = DateTime.UtcNow,
                    StudentId = studentId,
                    UnitId = viewModel.UnitId
                };

                // Set recipients based on checkboxes
                bool hasRecipient = false;

                if (viewModel.SendToGuidanceTeacher && !string.IsNullOrEmpty(viewModel.GuidanceTeacherId))
                {
                    feedback.GuidanceTeacherId = viewModel.GuidanceTeacherId;
                    feedback.Reciepient = Reciepient.GuidanceTeacher;
                    hasRecipient = true;
                }

                if (viewModel.SendToCurriculumHead && !string.IsNullOrEmpty(viewModel.CurriculumHeadId))
                {
                    feedback.CurriculumHeadId = viewModel.CurriculumHeadId;
                    feedback.Reciepient = Reciepient.CurriculumHead;
                    hasRecipient = true;
                }

                // Ensure at least one recipient is selected
                if (!hasRecipient)
                {
                    ModelState.AddModelError("", "Please select at least one recipient");
                    return View(viewModel);
                }

                // Add and save
                db.SimpleFeedbacks.Add(feedback);
                db.SaveChanges();

                return RedirectToAction("StudentDash", "Student");
            }

            // Need to repopulate dropdown if validation fails
            var studentUnits = db.Units
                .Where(u => u.Classes.Any(c => c.Students.Any(s => s.Id == studentId)))
                .ToList();

            viewModel.Units = studentUnits.Select(u => new SelectListItem
            {
                Value = u.UnitId.ToString(),
                Text = u.UnitName
            }).ToList();

            return View(viewModel);
        }

        public ActionResult ViewSimpleFeedbacks()
        {
            
            var studentId = User.Identity.GetUserId();
            var feedbacks = db.SimpleFeedbacks
                    .Include(f => f.GuidanceTeacher)
                    .Include(f => f.CurriculumHead)
                    .Include(f => f.Unit)
                    .Where(f => f.StudentId == studentId)
                    .ToList(); ;
            return View(feedbacks);
        }

        public ActionResult DeleteSimpleFeedback(int id)
        {
            var feedback = db.SimpleFeedbacks.Find(id);
            if (feedback != null)
            {
                db.SimpleFeedbacks.Remove(feedback);
                db.SaveChanges();
            }
            return RedirectToAction("ViewSimpleFeedbacks");
        }

        public ActionResult ViewSimpleFeedbackDetails(int id)
        {
            var currentUserId = User.Identity.GetUserId();

            var feedback = db.SimpleFeedbacks
                .Include(f => f.Student)
                .Include(f => f.GuidanceTeacher)
                .Include(f => f.CurriculumHead)
                .Include(f => f.Unit)
                .FirstOrDefault(f => f.FeedbackId == id);

            if (User.IsInRole("GuidanceTeacher") && feedback.GuidanceTeacherId == currentUserId)
            {
                if (!feedback.IsReadByGuidanceTeacher)
                {
                    feedback.IsReadByGuidanceTeacher = true;
                    db.SaveChanges();
                }
                return View(feedback);
            }
            else if (User.IsInRole("CurriculumHead") && feedback.CurriculumHeadId == currentUserId)
            {
                if (!feedback.IsReadByCurriculumHead)
                {
                    feedback.IsReadByCurriculumHead = true;
                    db.SaveChanges();
                }
                return View(feedback);
            }
            else if (User.IsInRole("Student") && feedback.StudentId == currentUserId)
            {
                return View(feedback);
            }
            throw new UnauthorizedAccessException("You do not have permission to view this feedback.");
        }


        public ActionResult EditSimpleFeedback(int id)
        {
            var feedback = db.SimpleFeedbacks.Find(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            var viewModel = new SimpleStudentFeedbackViewModel
            {
                Title = feedback.FeedbackTitle,
                content = feedback.FeedbackContent,
                UnitId = feedback.UnitId,
                SendToGuidanceTeacher = feedback.GuidanceTeacherId != null,
                SendToCurriculumHead = feedback.CurriculumHeadId != null
            };
            return View(viewModel);
        }

    }
    
}