using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using GuidanceTracker.Models.ViewModels.FeedbackViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    public class DetailedFeedbackController : Controller
    {
        // GET: DetailedFeedback

        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ReceivedFeedbackDash()
        {
            var currentUserId = User.Identity.GetUserId();

            // Count unread simple feedback
            int unreadSimpleFeedbackCount = 0;
            if (User.IsInRole("GuidanceTeacher"))
            {
                unreadSimpleFeedbackCount = db.SimpleFeedbacks
                    .Count(f => f.GuidanceTeacherId == currentUserId && !f.IsReadByGuidanceTeacher);
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                unreadSimpleFeedbackCount = db.SimpleFeedbacks
                    .Count(f => f.CurriculumHeadId == currentUserId && !f.IsReadByCurriculumHead);
            }

            // Count read simple feedback
            int readSimpleFeedbackCount = 0;
            if (User.IsInRole("GuidanceTeacher"))
            {
                readSimpleFeedbackCount = db.SimpleFeedbacks
                    .Count(f => f.GuidanceTeacherId == currentUserId && f.IsReadByGuidanceTeacher);
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                readSimpleFeedbackCount = db.SimpleFeedbacks
                    .Count(f => f.CurriculumHeadId == currentUserId && f.IsReadByCurriculumHead);
            }

            // Count unread simple feedback
            int unreadDetailedFeedbackCount = 0;
            if (User.IsInRole("GuidanceTeacher"))
            {
                unreadDetailedFeedbackCount = db.DetailedFeedbacks
                    .Count(f => f.CreatorId == currentUserId && !f.IsReadByGuidanceTeacher);
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                unreadDetailedFeedbackCount = db.DetailedFeedbacks
                    .Count(f => f.CreatorId == currentUserId && !f.IsReadByCurriculumHead);
            }

            // Count read simple feedback
            int readDetailedFeedbackCount = 0;
            if (User.IsInRole("GuidanceTeacher"))
            {
                readDetailedFeedbackCount = db.DetailedFeedbacks
                    .Count(f => f.CreatorId == currentUserId && f.IsReadByGuidanceTeacher);
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                readDetailedFeedbackCount = db.DetailedFeedbacks
                    .Count(f => f.CreatorId == currentUserId && f.IsReadByCurriculumHead);
            }

            ViewBag.UnreadSimpleFeedbackCount = unreadSimpleFeedbackCount;
            ViewBag.ReadSimpleFeedbackCount = readSimpleFeedbackCount;
            ViewBag.UnreadDetailedFeedbackCount = unreadDetailedFeedbackCount;
            ViewBag.ReadDetailedFeedbackCount = readDetailedFeedbackCount;

            db.SaveChanges();
            return View();
        }

        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewUnreadReceivedSimpleFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();
            var simpleFeedbacks = new List<SimpleFeedback>();

            if (User.IsInRole("GuidanceTeacher"))
            {
                // For guidance teachers
                simpleFeedbacks = db.SimpleFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Unit)
                    .Where(f => f.GuidanceTeacherId == currentUserId)
                    .Where(f => f.IsReadByGuidanceTeacher == false)
                    .ToList();
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                // For curriculum heads
                simpleFeedbacks = db.SimpleFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Unit)
                    .Where(f => f.CurriculumHeadId == currentUserId)
                    .Where(f => f.IsReadByCurriculumHead == false)
                    .ToList();
            }

            return View(simpleFeedbacks);
        }

        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewreadReceivedSimpleFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();
            var simpleFeedbacks = new List<SimpleFeedback>();

            if (User.IsInRole("GuidanceTeacher"))
            {
                // For guidance teachers
                simpleFeedbacks = db.SimpleFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Unit)
                    .Where(f => f.GuidanceTeacherId == currentUserId)
                    .Where(f => f.IsReadByGuidanceTeacher == true)
                    .ToList();
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                // For curriculum heads
                simpleFeedbacks = db.SimpleFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Unit)
                    .Where(f => f.CurriculumHeadId == currentUserId)
                    .Where(f => f.IsReadByCurriculumHead == true)
                    .ToList();
            }

            return View(simpleFeedbacks);
        }

        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewUnreadReceivedDetailedFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();
            var detailedFeedbacks = new List<DetailedFeedback>();

            if (User.IsInRole("GuidanceTeacher"))
            {
                // For guidance teachers
                detailedFeedbacks = db.DetailedFeedbacks
                    .Include(f => f.Student)
                    .Where(f => f.CreatorId == currentUserId)
                    .Where(f => f.IsReadByGuidanceTeacher == false)
                    .ToList();
                return View(detailedFeedbacks);
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                // For curriculum heads
                detailedFeedbacks = db.DetailedFeedbacks
                    .Include(f => f.Student)
                    .Where(f => f.CreatorId == currentUserId)
                    .Where(f => f.IsReadByCurriculumHead == false)
                    .ToList();
                return View(detailedFeedbacks);
            }

            throw new Exception("Nope");
        }

        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewreadReceivedDetailedFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();
            var detailedFeedbacks = new List<DetailedFeedback>();

            if (User.IsInRole("GuidanceTeacher"))
            {
                // For guidance teachers
                detailedFeedbacks = db.DetailedFeedbacks
                    .Include(f => f.Student)
                    .Where(f => f.CreatorId == currentUserId)
                    .Where(f => f.IsReadByGuidanceTeacher == true)
                    .ToList();
                return View(detailedFeedbacks);
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                // For curriculum heads
                detailedFeedbacks = db.DetailedFeedbacks
                    .Include(f => f.Student)
                    .Where(f => f.CreatorId == currentUserId)
                    .Where(f => f.IsReadByCurriculumHead == true)
                    .ToList();
                return View(detailedFeedbacks);
            }

            throw new Exception("Nope");
        }

        [Authorize(Roles = "Student")]
        public ActionResult ViewSentDetailedFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();

            // Get all detailed feedback created by this student
            var detailedFeedbacks = db.DetailedFeedbacks
                .Where(f => f.StudentId == currentUserId)
                .ToList();

            // Get creator names for each feedback
            foreach (var feedback in detailedFeedbacks)
            {
                var guidanceTeacher = db.GuidanceTeachers
                    .FirstOrDefault(g => g.Id == feedback.CreatorId);

                if (guidanceTeacher != null)
                {
                    ViewData[$"CreatorName_{feedback.FeedbackId}"] =
                        $"{guidanceTeacher.FirstName} {guidanceTeacher.LastName}";
                    ViewData[$"CreatorRole_{feedback.FeedbackId}"] = "Guidance Teacher";
                }
                else
                {
                    var curriculumHead = db.CurriculumHeads
                        .FirstOrDefault(c => c.Id == feedback.CreatorId);

                    if (curriculumHead != null)
                    {
                        ViewData[$"CreatorName_{feedback.FeedbackId}"] =
                            $"{curriculumHead.FirstName} {curriculumHead.LastName}";
                        ViewData[$"CreatorRole_{feedback.FeedbackId}"] = "Curriculum Head";
                    }
                    else
                    {
                        ViewData[$"CreatorName_{feedback.FeedbackId}"] = "Unknown";
                        ViewData[$"CreatorRole_{feedback.FeedbackId}"] = "";
                    }
                }
            }

            return View(detailedFeedbacks);
        }

        [Authorize(Roles = "GuidanceTeacher,CurriculumHead,Student")]
        public ActionResult ViewDetailedFeedbackDetails(int id)
        {
            var currentUserId = User.Identity.GetUserId();
            var feedback = db.DetailedFeedbacks
                .Include(f => f.Student)
                .Include(f => f.TargetClass)
                .FirstOrDefault(f => f.FeedbackId == id);

            // Get the creator's name
            string creatorName = "Unknown";
            var guidanceTeacher = db.GuidanceTeachers
                .FirstOrDefault(g => g.Id == feedback.CreatorId);

            if (guidanceTeacher != null)
            {
                creatorName = $"{guidanceTeacher.FirstName} {guidanceTeacher.LastName}";
                ViewBag.CreatorRole = "Guidance Teacher";
            }
            else
            {
                var curriculumHead = db.CurriculumHeads
                    .FirstOrDefault(c => c.Id == feedback.CreatorId);

                if (curriculumHead != null)
                {

                    creatorName = $"{curriculumHead.FirstName} {curriculumHead.LastName}";
                    ViewBag.CreatorRole = "Curriculum Head";
                }
            }

            ViewBag.CreatorName = creatorName;

            // Check if the current user has access to this feedback
            if (User.IsInRole("GuidanceTeacher") && (feedback.CreatorId == currentUserId))
            {
                feedback.IsReadByGuidanceTeacher = true;
                db.SaveChanges();
                return View(feedback);
            }
            else if (User.IsInRole("CurriculumHead") && (feedback.CreatorId == currentUserId))
            {
                feedback.IsReadByCurriculumHead = true;
                db.SaveChanges();
                return View(feedback);
            }
            else if (User.IsInRole("Student") && feedback.StudentId == currentUserId)
            {
                return View(feedback);
            }
            

            throw new InvalidOperationException("Nice try.");
        }



        [HttpGet]
        [Authorize(Roles = "GuidanceTeacher, CurriculumHead")]
        public ActionResult CreateDetailedFeedbackRequest()
        {
            var currentUserId = User.Identity.GetUserId();
            var viewModel = new RequestedDetailedFeedbackViewModel();

            // Check if the user is a GuidanceTeacher
            if (User.IsInRole("GuidanceTeacher"))
            {
                // Get classes assigned to this guidance teacher
                var guidanceTeacher = db.GuidanceTeachers.FirstOrDefault(g => g.Id == currentUserId);
                if (guidanceTeacher != null)
                {
                    var assignedClasses = db.Students
                        .Where(s => s.GuidanceTeacherId == currentUserId)
                        .Select(s => s.Class)
                        .Distinct()
                        .ToList();

                    viewModel.Classes = assignedClasses.Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = c.ClassName
                    }).ToList();
                }
            }
            // Check if the user is a CurriculumHead
            else if (User.IsInRole("CurriculumHead"))
            {
                // Get curriculum head's department
                var curriculumHead = db.CurriculumHeads.FirstOrDefault(c => c.Id == currentUserId);
                if (curriculumHead != null)
                {
                    // Get all classes that have enrollments in courses from this department
                    var departmentClasses = db.Classes
                        .Where(c => c.Enrollments.Any(e => e.Course.DepartmentId == curriculumHead.DepartmentId))
                        .Distinct()
                        .ToList();

                    viewModel.Classes = departmentClasses.Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = c.ClassName
                    }).ToList();
                }
            }

            return View(viewModel);
        }

        public ActionResult CreateDetailedFeedbackRequest(RequestedDetailedFeedbackViewModel viewModel)
        {
            var studentsInClass = db.Students.Where(s => s.ClassId == viewModel.ClassId).ToList();
            if (ModelState.IsValid)
            {
                var currentUserId = User.Identity.GetUserId();

                // Create a request for each student
                foreach (var student in studentsInClass)
                {
                    var request = new RequestedDetailedForm
                    {
                        ClassId = viewModel.ClassId,
                        CreatorId = currentUserId,
                        DateRequested = DateTime.Now,
                        StudentId = student.Id
                    };

                    db.RequestedDetailedForms.Add(request);
                }

                db.SaveChanges();
                return RedirectToAction("ReceivedFeedbackDash", "DetailedFeedback");
            }
            
            if (User.IsInRole("GuidanceTeacher"))
            {
                var guidanceTeacherId = User.Identity.GetUserId();
                var assignedClasses = db.Students
                    .Where(s => s.GuidanceTeacherId == guidanceTeacherId)
                    .Select(s => s.Class)
                    .Distinct()
                    .ToList();
                viewModel.Classes = assignedClasses.Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.ClassName
                
                }).ToList();


        }
            else if (User.IsInRole("CurriculumHead"))
            {
                var curriculumHead = db.CurriculumHeads.FirstOrDefault(c => c.Id == User.Identity.GetUserId());
                if (curriculumHead != null)
                {
                    var departmentClasses = db.Classes
                        .Where(c => c.Enrollments.Any(e => e.Course.DepartmentId == curriculumHead.DepartmentId))
                        .Distinct()
                        .ToList();

                    viewModel.Classes = departmentClasses.Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = c.ClassName
                    }).ToList();
                }
            }

            return View(viewModel);
        }

            

        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult CreateDetailedFeedback(int requestId)
        {
            // Get the feedback request
            var request = db.RequestedDetailedForms.Find(requestId);

            var studentId = User.Identity.GetUserId();
            var student = db.Students.FirstOrDefault(s => s.Id == studentId);

            student = db.Students
                    .Include(s => s.GuidanceTeacher)
                    .Include(s => s.Class.Enrollments.Select(e => e.Course.Department.CurriculumHead))
                    .FirstOrDefault(s => s.Id == studentId);

            var targetClass = db.Classes.Find(request.ClassId);
            var course = targetClass.Enrollments.FirstOrDefault()?.Course;

            // Create the view model
            var viewModel = new CreateDetailedFeedbackViewModel
            {
                RequestId = request.RequestId,
                ClassId = request.ClassId,
                Class = targetClass.ClassName,
                Course = course?.CourseName,
                FeedbackDate = DateTime.Now,
                StudentId = studentId,
                CreatorId = request.CreatorId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public ActionResult CreateDetailedFeedback(CreateDetailedFeedbackViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Get the current student
                var studentId = User.Identity.GetUserId();

                // Create the detailed feedback
                var feedback = new DetailedFeedback
                {
                    // Basic information
                    Course = viewModel.Course,
                    Class = viewModel.Class,

                    // Student information
                    StudentId = studentId,

                    // Class information
                    ClassId = viewModel.ClassId,

                    // Creator information
                    CreatorId = viewModel.CreatorId,

                    // Overall comments
                    BestFeatures = viewModel.BestFeatures,
                    AreasForImprovement = viewModel.AreasForImprovement,

                    // Learning & Teaching
                    LearningTeachingKeyIssues = viewModel.LearningTeachingKeyIssues,
                    LearningTeachingStrengths = viewModel.LearningTeachingStrengths,
                    LearningTeachingImprovements = viewModel.LearningTeachingImprovements,
                    LearningTeachingComments = viewModel.LearningTeachingComments,

                    // Assessment
                    AssessmentKeyIssues = viewModel.AssessmentKeyIssues,
                    AssessmentStrengths = viewModel.AssessmentStrengths,
                    AssessmentImprovements = viewModel.AssessmentImprovements,
                    AssessmentComments = viewModel.AssessmentComments,

                    // Resources
                    ResourcesKeyIssues = viewModel.ResourcesKeyIssues,
                    ResourcesStrengths = viewModel.ResourcesStrengths,
                    ResourcesImprovements = viewModel.ResourcesImprovements,
                    ResourcesComments = viewModel.ResourcesComments,

                    DateCreated = DateTime.Now,
                    IsSubmitted = true,
                };

                // Save the new feedback
                db.DetailedFeedbacks.Add(feedback);

                // Find and delete only this student's request
                var requestToDelete = db.RequestedDetailedForms
                    .FirstOrDefault(r => r.RequestId == viewModel.RequestId && r.StudentId == studentId);

                if (requestToDelete != null)
                {
                    db.RequestedDetailedForms.Remove(requestToDelete);
                }

                db.SaveChanges();
                return RedirectToAction("StudentDash", "Student");
            }

            return View(viewModel);
        }
    }
}