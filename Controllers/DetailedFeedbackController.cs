using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using GuidanceTracker.Models.ViewModels.FeedbackViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    /// <summary>
    /// This controller handles all the detailed feedback functionality in the system.
    /// It manages the complete lifecycle of detailed feedback - from requesting feedback from students,
    /// through to collecting individual responses, aggregating them into class summaries, and finally
    /// delivering the results to guidance teachers and curriculum heads.
    /// </summary>
    public class DetailedFeedbackController : Controller
    {
        #region Private Fields



        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        #endregion

        #region Dashboard and Overview Methods

        /// <summary>
        /// This displays the main dashboard for guidance teachers and curriculum heads.
        /// It shows them how many feedback responses they've received and which ones they haven't read yet.
        /// </summary>
        /// <returns>A view showing counts of read and unread feedback for the logged-in user</returns>
        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ReceivedFeedbackDash()
        {
            var currentUserId = User.Identity.GetUserId();

            // Count how many simple feedback forms this user hasn't read yet
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

            // Count how many simple feedback forms this user has already read
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

            // Count how many detailed class rep feedback reports this user hasn't read yet
            int unreadDetailedFeedbackCount = 0;
            if (User.IsInRole("GuidanceTeacher"))
            {
                unreadDetailedFeedbackCount = db.ClassRepFeedbacks
                    .Count(f => f.CreatorId == currentUserId && !f.IsReadByGuidanceTeacher && f.IsSubmittedByClassRep == true);
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                unreadDetailedFeedbackCount = db.ClassRepFeedbacks
                    .Count(f => f.CreatorId == currentUserId && !f.IsReadByCurriculumHead && f.IsSubmittedByClassRep == true);
            }

            // Count how many detailed class rep feedback reports this user has already read
            int readDetailedFeedbackCount = 0;
            if (User.IsInRole("GuidanceTeacher"))
            {
                readDetailedFeedbackCount = db.ClassRepFeedbacks
                    .Count(f => f.CreatorId == currentUserId && f.IsReadByGuidanceTeacher && f.IsSubmittedByClassRep == true);
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                readDetailedFeedbackCount = db.ClassRepFeedbacks
                    .Count(f => f.CreatorId == currentUserId && f.IsReadByCurriculumHead && f.IsSubmittedByClassRep == true);
            }

            // Pass all these counts to the view so it can display the dashboard properly
            ViewBag.UnreadSimpleFeedbackCount = unreadSimpleFeedbackCount;
            ViewBag.ReadSimpleFeedbackCount = readSimpleFeedbackCount;
            ViewBag.UnreadDetailedFeedbackCount = unreadDetailedFeedbackCount;
            ViewBag.ReadDetailedFeedbackCount = readDetailedFeedbackCount;

            db.SaveChanges();
            return View();
        }

        #endregion

        #region Simple Feedback Viewing Methods

        /// <summary>
        /// Shows all the simple feedback forms that the current user hasn't read yet.
        /// </summary>
        /// <returns>A list of unread simple feedback forms for the current user</returns>
        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewUnreadReceivedSimpleFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();
            var simpleFeedbacks = new List<SimpleFeedback>();

            if (User.IsInRole("GuidanceTeacher"))
            {
                // For guidance teachers - get feedback sent specifically to them that they haven't read
                simpleFeedbacks = db.SimpleFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Unit)
                    .Where(f => f.GuidanceTeacherId == currentUserId)
                    .Where(f => f.IsReadByGuidanceTeacher == false)
                    .ToList();
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                // For curriculum heads - get feedback sent specifically to them that they haven't read
                simpleFeedbacks = db.SimpleFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Unit)
                    .Where(f => f.CurriculumHeadId == currentUserId)
                    .Where(f => f.IsReadByCurriculumHead == false)
                    .ToList();
            }

            return View(simpleFeedbacks);
        }

        /// <summary>
        /// Shows all the simple feedback forms that the current user has already read.
        /// </summary>
        /// <returns>A list of previously read simple feedback forms for the current user</returns>
        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewreadReceivedSimpleFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();
            var simpleFeedbacks = new List<SimpleFeedback>();

            if (User.IsInRole("GuidanceTeacher"))
            {
                // For guidance teachers - get feedback they've already marked as read
                simpleFeedbacks = db.SimpleFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Unit)
                    .Where(f => f.GuidanceTeacherId == currentUserId)
                    .Where(f => f.IsReadByGuidanceTeacher == true)
                    .ToList();
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                // For curriculum heads - get feedback they've already marked as read
                simpleFeedbacks = db.SimpleFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Unit)
                    .Where(f => f.CurriculumHeadId == currentUserId)
                    .Where(f => f.IsReadByCurriculumHead == true)
                    .ToList();
            }

            return View(simpleFeedbacks);
        }

        #endregion

        #region Detailed Feedback Viewing Methods

        /// <summary>
        /// Shows all the detailed class feedback reports that the current user hasn't read yet.
        /// These are the comprehensive reports that class representatives create after collecting
        /// feedback from all students in their class. They're much more detailed than simple feedback.
        /// </summary>
        /// <returns>A list of unread detailed feedback reports ordered by submission date</returns>
        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewUnreadReceivedDetailedFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();
            var detailedFeedbacks = new List<ClassRepFeedback>();

            if (User.IsInRole("GuidanceTeacher"))
            {
                detailedFeedbacks = db.ClassRepFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.TargetClass)
                    .Where(f => f.CreatorId == currentUserId)
                    .Where(f => f.IsReadByGuidanceTeacher == false)
                    .Where(f => f.IsSubmittedByClassRep == true)
                    .OrderByDescending(f => f.DateSubmittedByClassRep)
                    .ToList();
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                detailedFeedbacks = db.ClassRepFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.TargetClass)
                    .Where(f => f.CreatorId == currentUserId)
                    .Where(f => f.IsReadByCurriculumHead == false)
                    .Where(f => f.IsSubmittedByClassRep == true)
                    .OrderByDescending(f => f.DateSubmittedByClassRep)
                    .ToList();
            }

            return View(detailedFeedbacks);
        }

        /// <summary>
        /// Shows all the detailed class feedback reports that the current user has already read.
        /// This provides access to historical feedback reports for review and reference purposes.
        /// The reports are sorted with the most recent ones first.
        /// </summary>
        /// <returns>A list of previously read detailed feedback reports ordered by submission date</returns>
        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewreadReceivedDetailedFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();
            var detailedFeedbacks = new List<ClassRepFeedback>();

            if (User.IsInRole("GuidanceTeacher"))
            {
                detailedFeedbacks = db.ClassRepFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.TargetClass)
                    .Where(f => f.CreatorId == currentUserId)
                    .Where(f => f.IsReadByGuidanceTeacher == true)
                    .Where(f => f.IsSubmittedByClassRep == true)
                    .OrderByDescending(f => f.DateSubmittedByClassRep)
                    .ToList();
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                detailedFeedbacks = db.ClassRepFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.TargetClass)
                    .Where(f => f.CreatorId == currentUserId)
                    .Where(f => f.IsReadByCurriculumHead == true)
                    .Where(f => f.IsSubmittedByClassRep == true)
                    .OrderByDescending(f => f.DateSubmittedByClassRep)
                    .ToList();
            }

            return View(detailedFeedbacks);
        }

        /// <summary>
        /// Shows the complete details of a specific class representative feedback report.
        /// When a user clicks on a feedback report to read it, this method displays all the details
        /// and automatically marks it as read so it won't appear in their unread list anymore.
        /// </summary>
        /// <param name="id">The unique identifier of the feedback report to display</param>
        /// <returns>The detailed view of the feedback report, or an error if not found or unauthorised</returns>
        [Authorize(Roles = "GuidanceTeacher,CurriculumHead")]
        public ActionResult ViewClassRepFeedbackDetails(int id)
        {
            var currentUserId = User.Identity.GetUserId();
            var feedback = db.ClassRepFeedbacks
                .Include(f => f.Student)
                .Include(f => f.TargetClass)
                .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null)
            {
                return HttpNotFound();
            }

            // Make sure this user is authorised to view this feedback - they must be the one who requested it
            if (feedback.CreatorId != currentUserId)
            {
                return new HttpStatusCodeResult(403, "Unauthorized");
            }

            // Mark this feedback as read so it moves from the unread to the read pile
            if (User.IsInRole("GuidanceTeacher"))
            {
                feedback.IsReadByGuidanceTeacher = true;
            }
            else if (User.IsInRole("CurriculumHead"))
            {
                feedback.IsReadByCurriculumHead = true;
            }

            db.SaveChanges();

            return View(feedback);
        }

        #endregion

        #region Student Feedback Viewing Methods

        /// <summary>
        /// Shows students all the detailed feedback forms they've submitted in the past.
        /// This lets students keep track of what feedback they've given and when.
        /// It also shows them who requested each piece of feedback (which guidance teacher or curriculum head).
        /// </summary>
        /// <returns>A list of all detailed feedback forms submitted by the current student</returns>
        [Authorize(Roles = "Student")]
        public ActionResult ViewSentDetailedFeedbacks()
        {
            var currentUserId = User.Identity.GetUserId();

            // Get all the detailed feedback this student has submitted
            var detailedFeedbacks = db.DetailedFeedbacks
                .Where(f => f.StudentId == currentUserId)
                .ToList();

            // For each feedback, work out who requested it and add their name to the display
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

        /// <summary>
        /// Shows the complete details of a specific detailed feedback form.
        /// Students can use this to review exactly what they submitted, whilst guidance teachers
        /// and curriculum heads can view feedback that was submitted for requests they created.
        /// </summary>
        /// <param name="id">The unique identifier of the detailed feedback to display</param>
        /// <returns>The detailed view of the feedback form, or an error if unauthorised</returns>
        [Authorize(Roles = "GuidanceTeacher,CurriculumHead,Student")]
        public ActionResult ViewDetailedFeedbackDetails(string id)
        {
            var currentUserId = User.Identity.GetUserId();
            var feedback = db.DetailedFeedbacks
                .Include(f => f.Student)
                .Include(f => f.TargetClass)
                .FirstOrDefault(f => f.FeedbackId == id);

            // Work out who originally requested this feedback and add their details to the view
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

            // Only allow students to view their own feedback submissions
            if (User.IsInRole("Student") && feedback.StudentId == currentUserId)
            {
                return View(feedback);
            }

            // If we get here, someone's trying to access feedback they shouldn't see
            throw new InvalidOperationException("Nice try.");
        }

        #endregion

        #region Feedback Request Creation Methods

        /// <summary>
        /// Shows the form for creating a new detailed feedback request.
        /// This is where guidance teachers and curriculum heads can select a class
        /// and request detailed feedback from all the students in that class.
        /// The dropdown list only shows classes that the user is responsible for.
        /// </summary>
        /// <returns>A form for creating a new detailed feedback request</returns>
        [HttpGet]
        [Authorize(Roles = "GuidanceTeacher, CurriculumHead")]
        public ActionResult CreateDetailedFeedbackRequest()
        {
            var currentUserId = User.Identity.GetUserId();
            var viewModel = new RequestedDetailedFeedbackViewModel();

            // Set up the dropdown list of classes based on the user's role and responsibilities
            if (User.IsInRole("GuidanceTeacher"))
            {
                // Guidance teachers can only request feedback from classes they're assigned to
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
            else if (User.IsInRole("CurriculumHead"))
            {
                // Curriculum heads can request feedback from any class studying courses in their department
                var curriculumHead = db.CurriculumHeads.FirstOrDefault(c => c.Id == currentUserId);
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

        /// <summary>
        /// Processes the creation of a new detailed feedback request.
        /// When a guidance teacher or curriculum head submits the request form, this method:
        /// 1. Creates a feedback request for every student in the selected class
        /// 2. Sets a flag on the class to indicate there's an active feedback request
        /// 3. Sends the user back to their dashboard
        /// If there are validation errors, it reshows the form with the errors highlighted.
        /// </summary>
        /// <param name="viewModel">The form data containing the selected class and other details</param>
        /// <returns>Redirect to dashboard on success, or the form again with errors</returns>
        [HttpPost]
        public ActionResult CreateDetailedFeedbackRequest(RequestedDetailedFeedbackViewModel viewModel)
        {
            var students = db.Students
              .Where(s => s.ClassId == viewModel.ClassId)
              .ToList();
            if (ModelState.IsValid)
            {
                var currentUserId = User.Identity.GetUserId();

                // Set a flag on the class to indicate there's now an active detailed feedback request
                // For use in the class rep feedback dashboard
                var targetClass = db.Classes.Find(viewModel.ClassId);
                if (targetClass != null)
                {
                    targetClass.HasActiveDetailedFeedbackRequest = true;
                    targetClass.DateRequestStarted = DateTime.UtcNow; // Track when this cycle began
                }
                // Create an individual feedback request for each student in the class
                // Each student will see this in their dashboard and can then fill out their feedback
                foreach (var student in students)
                {
                    var request = new RequestedDetailedForm
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ClassId = viewModel.ClassId,
                        CreatorId = currentUserId,
                        DateRequested = DateTime.UtcNow,
                        StudentId = student.Id,
                    };

                    db.RequestedDetailedForms.Add(request);
                }

                db.SaveChanges();
                return RedirectToAction("ReceivedFeedbackDash", "DetailedFeedback");
            }

            // If we get here, there were validation errors, so we need to rebuild the class dropdown
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

        #endregion

        #region Student Feedback Submission Methods

        /// <summary>
        /// Shows the detailed feedback form for a student to fill out.
        /// When a student clicks on a feedback request in their dashboard, this method
        /// creates a pre-populated form with all the questions they need to answer.
        /// The form includes ratings, yes/no questions, and text areas for detailed comments.
        /// </summary>
        /// <param name="requestId">The unique identifier of the feedback request</param>
        /// <returns>A comprehensive feedback form for the student to complete</returns>
        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult CreateDetailedFeedback(string requestId)
        {
            // Get the specific feedback request and work out all the context information
            var request = db.RequestedDetailedForms.Find(requestId);
            var studentId = User.Identity.GetUserId();
            var student = db.Students.FirstOrDefault(s => s.Id == studentId);
            student = db.Students
                    .Include(s => s.GuidanceTeacher)
                    .Include(s => s.Class.Enrollments.Select(e => e.Course.Department.CurriculumHead))
                    .FirstOrDefault(s => s.Id == studentId);
            var targetClass = db.Classes.Find(request.ClassId);
            var course = targetClass.Enrollments.FirstOrDefault()?.Course;

            // Create a view model with all the form fields pre set to sensible defaults
            var viewModel = new CreateDetailedFeedbackViewModel
            {
                RequestId = request.RequestId,
                ClassId = request.ClassId,
                Class = targetClass.ClassName,
                Course = course?.CourseName,
                FeedbackDate = DateTime.UtcNow,
                StudentId = studentId,
                CreatorId = request.CreatorId,

                // Set all rating questions to 5 (middle of the scale) as a starting point
                OverallRating = 5,
                LearningExperienceRating = 5,
                LearningTeachingRating = 5,
                AssessmentRating = 5,
                ResourcesRating = 5,
                SupportEffectivenessRating = 5,
                SkillsDevelopmentRating = 5,

                // Leave all the yes/no questions blank so students have to actively choose
                MeetsExpectations = null,
                WouldRecommend = null,
                WorkloadManageable = null,
                ConceptsPresented = null,
                MaterialsAvailable = null,
                AccommodatesStyles = null,
                LecturerResponsive = null,
                AssessmentConfidence = null,
                TimelyFeedback = null,
                SpecificFeedback = null,
                AssessmentsAligned = null,
                SufficientTime = null,
                MaterialsAccessible = null,
                PlatformOrganised = null,
                EquipmentWorking = null,
                SupplementaryResources = null,
                SpecializedEquipment = null,
                LibraryResources = null,
                StaffResponsive = null,
                AdditionalHelpAvailable = null,
                AccommodationsProvided = null,
                ClearPointsOfContact = null,
                DevelopingCriticalThinking = null,
                EnhancingProblemSolving = null,
                GainingPracticalSkills = null,
                ImprovingCommunication = null,
                DevelopingResearchSkills = null,

                // Initialise all text areas as empty strings so they're ready for input
                MeetsExpectationsNotes = string.Empty,
                WouldRecommendNotes = string.Empty,
                WorkloadManageableNotes = string.Empty,
                LearningExperienceKeyIssues = string.Empty,
                LearningExperienceStrengths = string.Empty,
                LearningExperienceImprovements = string.Empty,
                LearningExperienceComments = string.Empty,
                ConceptsPresentedNotes = string.Empty,
                MaterialsAvailableNotes = string.Empty,
                AccommodatesStylesNotes = string.Empty,
                LecturerResponsiveNotes = string.Empty,
                LearningTeachingKeyIssues = string.Empty,
                LearningTeachingStrengths = string.Empty,
                LearningTeachingImprovements = string.Empty,
                LearningTeachingComments = string.Empty,
                AssessmentConfidenceNotes = string.Empty,
                TimelyFeedbackNotes = string.Empty,
                SpecificFeedbackNotes = string.Empty,
                AssessmentsAlignedNotes = string.Empty,
                SufficientTimeNotes = string.Empty,
                AssessmentKeyIssues = string.Empty,
                AssessmentStrengths = string.Empty,
                AssessmentImprovements = string.Empty,
                AssessmentComments = string.Empty,
                MaterialsAccessibleNotes = string.Empty,
                PlatformOrganisedNotes = string.Empty,
                EquipmentWorkingNotes = string.Empty,
                SupplementaryResourcesNotes = string.Empty,
                SpecializedEquipmentNotes = string.Empty,
                LibraryResourcesNotes = string.Empty,
                ResourcesKeyIssues = string.Empty,
                ResourcesStrengths = string.Empty,
                ResourcesImprovements = string.Empty,
                ResourcesComments = string.Empty,
                StaffResponsiveNotes = string.Empty,
                AdditionalHelpAvailableNotes = string.Empty,
                AccommodationsProvidedNotes = string.Empty,
                ClearPointsOfContactNotes = string.Empty,
                SupportEffectivenesssKeyIssues = string.Empty,
                SupportEffectivenessStrengths = string.Empty,
                SupportEffectivenessImprovements = string.Empty,
                SupportEffectivenessComments = string.Empty,
                DevelopingCriticalThinkingNotes = string.Empty,
                EnhancingProblemSolvingNotes = string.Empty,
                GainingPracticalSkillsNotes = string.Empty,
                ImprovingCommunicationNotes = string.Empty,
                DevelopingResearchSkillsNotes = string.Empty,
                SkillsDevelopmentKeyIssues = string.Empty,
                SkillsDevelopmentStrengths = string.Empty,
                SkillsDevelopmentImprovements = string.Empty,
                SkillsDevelopmentComments = string.Empty,
                BestFeatures = string.Empty,
                AreasForImprovement = string.Empty
            };

            return View(viewModel);
        }

        /// <summary>
        /// Processes a completed detailed feedback form from a student.
        /// This method takes all the information the student has entered and saves it to the database.
        /// It also removes the original request (since it's now been completed) and works out
        /// who the class representative is for future use in the aggregation process.
        /// </summary>
        /// <param name="viewModel">All the form data the student has submitted</param>
        /// <returns>Redirect to student dashboard on success, or back to the form if there are errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public ActionResult CreateDetailedFeedback(CreateDetailedFeedbackViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var studentId = User.Identity.GetUserId();
                var requestId = viewModel.RequestId;

                // Find out who the class representative is for this class
                var classRep = db.Students
                .FirstOrDefault(s => s.ClassId == viewModel.ClassId && s.IsClassRep == true);

                // Create a new detailed feedback record with all the student's responses
                var feedback = new DetailedFeedback
                {
                    FeedbackId = Guid.NewGuid().ToString(),
                    // Basic course and class information
                    Course = viewModel.Course,
                    Class = viewModel.Class,

                    // Who submitted this feedback
                    StudentId = studentId,

                    // Which class this feedback is about
                    ClassId = viewModel.ClassId,

                    // Who the class rep is (they'll need this later for aggregation)
                    ClassRepId = classRep?.Id,

                    // Who originally requested this feedback
                    CreatorId = viewModel.CreatorId,

                    // All the learning experience responses
                    MeetsExpectations = viewModel.MeetsExpectations,
                    MeetsExpectationsNotes = viewModel.MeetsExpectationsNotes,
                    WouldRecommend = viewModel.WouldRecommend,
                    WouldRecommendNotes = viewModel.WouldRecommendNotes,
                    WorkloadManageable = viewModel.WorkloadManageable,
                    WorkloadManageableNotes = viewModel.WorkloadManageableNotes,
                    LearningExperienceKeyIssues = viewModel.LearningExperienceKeyIssues,
                    LearningExperienceStrengths = viewModel.LearningExperienceStrengths,
                    LearningExperienceImprovements = viewModel.LearningExperienceImprovements,
                    LearningExperienceComments = viewModel.LearningExperienceComments,
                    LearningExperienceRating = viewModel.LearningExperienceRating,

                    // All the teaching and learning method responses
                    ConceptsPresented = viewModel.ConceptsPresented,
                    ConceptsPresentedNotes = viewModel.ConceptsPresentedNotes,
                    MaterialsAvailable = viewModel.MaterialsAvailable,
                    MaterialsAvailableNotes = viewModel.MaterialsAvailableNotes,
                    AccommodatesStyles = viewModel.AccommodatesStyles,
                    AccommodatesStylesNotes = viewModel.AccommodatesStylesNotes,
                    LecturerResponsive = viewModel.LecturerResponsive,
                    LecturerResponsiveNotes = viewModel.LecturerResponsiveNotes,
                    LearningTeachingKeyIssues = viewModel.LearningTeachingKeyIssues,
                    LearningTeachingStrengths = viewModel.LearningTeachingStrengths,
                    LearningTeachingImprovements = viewModel.LearningTeachingImprovements,
                    LearningTeachingComments = viewModel.LearningTeachingComments,
                    LearningTeachingRating = viewModel.LearningTeachingRating,

                    // All the assessment and progress tracking responses
                    AssessmentConfidence = viewModel.AssessmentConfidence,
                    AssessmentConfidenceNotes = viewModel.AssessmentConfidenceNotes,
                    TimelyFeedback = viewModel.TimelyFeedback,
                    TimelyFeedbackNotes = viewModel.TimelyFeedbackNotes,
                    SpecificFeedback = viewModel.SpecificFeedback,
                    SpecificFeedbackNotes = viewModel.SpecificFeedbackNotes,
                    AssessmentsAligned = viewModel.AssessmentsAligned,
                    AssessmentsAlignedNotes = viewModel.AssessmentsAlignedNotes,
                    SufficientTime = viewModel.SufficientTime,
                    SufficientTimeNotes = viewModel.SufficientTimeNotes,
                    AssessmentKeyIssues = viewModel.AssessmentKeyIssues,
                    AssessmentStrengths = viewModel.AssessmentStrengths,
                    AssessmentImprovements = viewModel.AssessmentImprovements,
                    AssessmentComments = viewModel.AssessmentComments,
                    AssessmentRating = viewModel.AssessmentRating,

                    // All the learning resources and environment responses
                    MaterialsAccessible = viewModel.MaterialsAccessible,
                    MaterialsAccessibleNotes = viewModel.MaterialsAccessibleNotes,
                    PlatformOrganised = viewModel.PlatformOrganised,
                    PlatformOrganisedNotes = viewModel.PlatformOrganisedNotes,
                    EquipmentWorking = viewModel.EquipmentWorking,
                    EquipmentWorkingNotes = viewModel.EquipmentWorkingNotes,
                    SupplementaryResources = viewModel.SupplementaryResources,
                    SupplementaryResourcesNotes = viewModel.SupplementaryResourcesNotes,
                    SpecializedEquipment = viewModel.SpecializedEquipment,
                    SpecializedEquipmentNotes = viewModel.SpecializedEquipmentNotes,
                    LibraryResources = viewModel.LibraryResources,
                    LibraryResourcesNotes = viewModel.LibraryResourcesNotes,
                    ResourcesKeyIssues = viewModel.ResourcesKeyIssues,
                    ResourcesStrengths = viewModel.ResourcesStrengths,
                    ResourcesImprovements = viewModel.ResourcesImprovements,
                    ResourcesComments = viewModel.ResourcesComments,
                    ResourcesRating = viewModel.ResourcesRating,

                    // All the communication and support responses
                    StaffResponsive = viewModel.StaffResponsive,
                    StaffResponsiveNotes = viewModel.StaffResponsiveNotes,
                    AdditionalHelpAvailable = viewModel.AdditionalHelpAvailable,
                    AdditionalHelpAvailableNotes = viewModel.AdditionalHelpAvailableNotes,
                    AccommodationsProvided = viewModel.AccommodationsProvided,
                    AccommodationsProvidedNotes = viewModel.AccommodationsProvidedNotes,
                    ClearPointsOfContact = viewModel.ClearPointsOfContact,
                    ClearPointsOfContactNotes = viewModel.ClearPointsOfContactNotes,
                    SupportEffectivenesssKeyIssues = viewModel.SupportEffectivenesssKeyIssues,
                    SupportEffectivenessStrengths = viewModel.SupportEffectivenessStrengths,
                    SupportEffectivenessImprovements = viewModel.SupportEffectivenessImprovements,
                    SupportEffectivenessComments = viewModel.SupportEffectivenessComments,
                    SupportEffectivenessRating = viewModel.SupportEffectivenessRating,

                    // All the skills development responses
                    DevelopingCriticalThinking = viewModel.DevelopingCriticalThinking,
                    DevelopingCriticalThinkingNotes = viewModel.DevelopingCriticalThinkingNotes,
                    EnhancingProblemSolving = viewModel.EnhancingProblemSolving,
                    EnhancingProblemSolvingNotes = viewModel.EnhancingProblemSolvingNotes,
                    GainingPracticalSkills = viewModel.GainingPracticalSkills,
                    GainingPracticalSkillsNotes = viewModel.GainingPracticalSkillsNotes,
                    ImprovingCommunication = viewModel.ImprovingCommunication,
                    ImprovingCommunicationNotes = viewModel.ImprovingCommunicationNotes,
                    DevelopingResearchSkills = viewModel.DevelopingResearchSkills,
                    DevelopingResearchSkillsNotes = viewModel.DevelopingResearchSkillsNotes,
                    SkillsDevelopmentKeyIssues = viewModel.SkillsDevelopmentKeyIssues,
                    SkillsDevelopmentStrengths = viewModel.SkillsDevelopmentStrengths,
                    SkillsDevelopmentImprovements = viewModel.SkillsDevelopmentImprovements,
                    SkillsDevelopmentComments = viewModel.SkillsDevelopmentComments,
                    SkillsDevelopmentRating = viewModel.SkillsDevelopmentRating,

                    // Overall summary responses
                    BestFeatures = viewModel.BestFeatures,
                    AreasForImprovement = viewModel.AreasForImprovement,
                    OverallRating = viewModel.OverallRating,

                    // Administrative information
                    DateCreated = DateTime.UtcNow,
                    IsSubmitted = true,
                };

                // Save the completed feedback to the database
                db.DetailedFeedbacks.Add(feedback);

                // Remove the original request since it's now been completed
                var requestToDelete = db.RequestedDetailedForms
                    .FirstOrDefault(r => r.RequestId == requestId && r.StudentId == studentId);
                if (requestToDelete != null)
                {
                    db.RequestedDetailedForms.Remove(requestToDelete);
                }

                db.SaveChanges();
                return RedirectToAction("StudentDash", "Student");
            }
            return View(viewModel);
        }

        #endregion

        #region Class Representative Dashboard and Management

        /// <summary>
        /// Shows the dashboard for class representatives to manage the feedback collection process.
        /// This dashboard shows how many students in their class have submitted feedback,
        /// how many are still pending, and whether they can proceed to create the class summary.
        /// It handles different states: collection in progress, ready for review, and completed.
        /// </summary>
        /// <returns>A dashboard showing the current state of feedback collection for the class</returns>
        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult ClassRepDashboard()
        {
            var currentUserId = User.Identity.GetUserId();
            var student = db.Students
                .Include(s => s.Class) // Include the class to access the flag
                .FirstOrDefault(s => s.Id == currentUserId);

            if (student == null || !student.IsClassRep)
            {
                return new HttpStatusCodeResult(403, "Unauthorized - You must be a class representative to access this page.");
            }

            // Work out the current status of feedback collection for this class
            var totalStudentsInClass = db.Students.Count(s => s.ClassId == student.ClassId);

            int submittedFeedbacks;
            if (student.Class.HasActiveDetailedFeedbackRequest && student.Class.DateRequestStarted.HasValue)
            {
                // Count only submissions made after the current request cycle started
                submittedFeedbacks = db.DetailedFeedbacks
                    .Where(f => f.ClassId == student.ClassId &&
                               f.IsSubmitted == true &&
                               f.DateCreated >= student.Class.DateRequestStarted.Value)
                    .Count();
            }
            else
            {
                // No active request, so count should be 0
                submittedFeedbacks = 0;
            }

            var pendingRequests = db.RequestedDetailedForms
                .Where(r => r.ClassId == student.ClassId)
                .Count();

            // Find the most recent class rep feedback submission (if any)
            var existingClassRepFeedback = db.ClassRepFeedbacks
                .Where(f => f.ClassId == student.ClassId && f.StudentId == currentUserId && f.IsSubmittedByClassRep == true)
                .OrderByDescending(f => f.DateSubmittedByClassRep)
                .FirstOrDefault();

            // Determine the current state of the feedback collection process
            var allSubmitted = submittedFeedbacks >= totalStudentsInClass && pendingRequests == 0;
            var hasActiveRequest = student.Class.HasActiveDetailedFeedbackRequest;


            // Pass all the status information to the view
            ViewBag.TotalStudents = totalStudentsInClass;
            ViewBag.SubmittedCount = submittedFeedbacks;
            ViewBag.PendingCount = pendingRequests;
            ViewBag.ClassName = student.Class?.ClassName;

            // Show the "Review Aggregated Results" button only when all students have submitted AND there's an active request
            ViewBag.AllSubmitted = allSubmitted && hasActiveRequest;

            // Show the "New Request" alert when there are pending requests AND there's already been a previous submission
            ViewBag.HasNewRequest = pendingRequests > 0 && existingClassRepFeedback != null;

            // Show the completion message when there's no active request but there is a previous submission
            if (!hasActiveRequest && existingClassRepFeedback != null)
            {
                ViewBag.SubmissionDate = existingClassRepFeedback.DateSubmittedByClassRep;
                ViewBag.ShowCompletionMessage = true;
            }

            return View();
        }

        #endregion

        #region Feedback Aggregation and Review

        /// <summary>
        /// Shows the class representative a summary of all the feedback collected from their classmates.
        /// This takes all the individual student responses and combines them into a single view
        /// showing counts, averages, and aggregated comments. The class rep can review this
        /// before adding their own class discussion notes and submitting the final report.
        /// </summary>
        /// <returns>An aggregated view of all feedback for the class, ready for review and submission</returns>
        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult ReviewAggregatedFeedback()
        {
            var currentUserId = User.Identity.GetUserId();
            var student = db.Students
                .Include(s => s.Class)
                .FirstOrDefault(s => s.Id == currentUserId);


            // Make sure all students have actually submitted their feedback
            var totalStudentsInClass = db.Students.Count(s => s.ClassId == student.ClassId);
            // Only get submissions from the current request cycle
            var submittedFeedbacks = new List<DetailedFeedback>();
            if (student.Class.DateRequestStarted.HasValue)
            {
                submittedFeedbacks = db.DetailedFeedbacks
                    .Where(f => f.ClassId == student.ClassId &&
                               f.IsSubmitted == true &&
                               f.DateCreated >= student.Class.DateRequestStarted.Value)
                    .ToList();
            }

            // Double check there are no pending requests still outstanding
            var pendingRequests = db.RequestedDetailedForms
                .Where(r => r.ClassId == student.ClassId)
                .Count();


            // Create the aggregated summary from all the individual feedback responses
            var aggregatedModel = CreateAggregatedModel(submittedFeedbacks, student);

            return View(aggregatedModel);
        }

        /// <summary>
        /// Processes the final submission of class representative feedback.
        /// This takes the aggregated feedback data plus the class rep's discussion notes
        /// and creates the final report that gets sent to guidance teachers/curriculum heads.
        /// It also clears the active request flag and marks all individual feedback as processed.
        /// </summary>
        /// <param name="model">The aggregated feedback data with class discussion notes</param>
        /// <param name="ClassDiscussionNotes">Additional general discussion notes</param>
        /// <param name="AdditionalClassComments">Any extra comments from the class representative</param>
        /// <returns>Redirect to dashboard on success, or back to review page on error</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public ActionResult SubmitClassRepFeedback(ClassRepFeedback model, string ClassDiscussionNotes, string AdditionalClassComments)
        {
            var currentUserId = User.Identity.GetUserId();
            var student = db.Students
                .Include(s => s.Class)
                .FirstOrDefault(s => s.Id == currentUserId);

            // Final verification that all students have submitted their feedback
            var totalStudentsInClass = db.Students.Count(s => s.ClassId == student.ClassId);
            var submittedFeedbacks = new List<DetailedFeedback>();
            if (student.Class.DateRequestStarted.HasValue)
            {
                submittedFeedbacks = db.DetailedFeedbacks
                    .Where(f => f.ClassId == student.ClassId &&
                               f.IsSubmitted == true &&
                               f.DateCreated >= student.Class.DateRequestStarted.Value)
                    .ToList();
            }


            try
            {
                // Create the final aggregated feedback report
                var finalFeedback = CreateAggregatedModel(submittedFeedbacks, student);

                // Add all the class discussion notes that the class rep has provided
                finalFeedback.LearningExperienceClassDiscussion = model.LearningExperienceClassDiscussion ?? string.Empty;
                finalFeedback.LearningTeachingClassDiscussion = model.LearningTeachingClassDiscussion ?? string.Empty;
                finalFeedback.AssessmentClassDiscussion = model.AssessmentClassDiscussion ?? string.Empty;
                finalFeedback.ResourcesClassDiscussion = model.ResourcesClassDiscussion ?? string.Empty;
                finalFeedback.SupportEffectivenessClassDiscussion = model.SupportEffectivenessClassDiscussion ?? string.Empty;
                finalFeedback.SkillsDevelopmentClassDiscussion = model.SkillsDevelopmentClassDiscussion ?? string.Empty;
                finalFeedback.OverallClassDiscussionNotes = model.OverallClassDiscussionNotes ?? string.Empty;
                finalFeedback.AdditionalClassComments = AdditionalClassComments ?? string.Empty;

                // Mark the feedback as completed and submitted
                finalFeedback.DateSubmittedByClassRep = DateTime.UtcNow;
                finalFeedback.IsSubmittedByClassRep = true;
                finalFeedback.IsRequested = false;

                // Clear the active request flag on the class - this feedback cycle is now complete
                student.Class.HasActiveDetailedFeedbackRequest = false;
                student.Class.DateRequestStarted = null;

                // Save the final feedback report to the database
                db.ClassRepFeedbacks.Add(finalFeedback);

                // Mark all the individual student feedback as processed by the class rep
                foreach (var feedback in submittedFeedbacks)
                {
                    feedback.IsReadByClassRep = true;
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = "Class feedback has been successfully submitted! The results have been sent to your guidance teacher/curriculum head.";
                return RedirectToAction("ClassRepDashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting the feedback. Please try again.";
                return RedirectToAction("ReviewAggregatedFeedback");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Creates an aggregated summary from all the individual detailed feedback responses.
        /// This method takes a list of individual student feedback forms and combines them into
        /// a single summary showing counts for yes/no questions, average ratings, and
        /// all the text comments grouped together. This is the core logic that turns
        /// individual responses into a class summary report.
        /// </summary>
        /// <returns>A comprehensive aggregated feedback report ready for class rep review</returns>
        private ClassRepFeedback CreateAggregatedModel(List<DetailedFeedback> feedbacks, Student classRep)
        {
            if (!feedbacks.Any()) return new ClassRepFeedback();

            var firstFeedback = feedbacks.First();

            return new ClassRepFeedback
            {
                Course = firstFeedback.Course,
                Class = firstFeedback.Class,
                StudentId = classRep.Id,
                ClassId = classRep.ClassId,
                CreatorId = firstFeedback.CreatorId,

                // Learning Experience Questions - count yes/no responses and combines all notes
                MeetsExpectationsYesCount = feedbacks.Count(f => f.MeetsExpectations == true),
                MeetsExpectationsNoCount = feedbacks.Count(f => f.MeetsExpectations == false),
                MeetsExpectationsNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.MeetsExpectationsNotes)).Select(f => f.MeetsExpectationsNotes).Distinct()),

                WouldRecommendYesCount = feedbacks.Count(f => f.WouldRecommend == true),
                WouldRecommendNoCount = feedbacks.Count(f => f.WouldRecommend == false),
                WouldRecommendNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.WouldRecommendNotes)).Select(f => f.WouldRecommendNotes).Distinct()),

                WorkloadManageableYesCount = feedbacks.Count(f => f.WorkloadManageable == true),
                WorkloadManageableNoCount = feedbacks.Count(f => f.WorkloadManageable == false),
                WorkloadManageableNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.WorkloadManageableNotes)).Select(f => f.WorkloadManageableNotes).Distinct()),

                // Combine all the text feedback into comprehensive lists
                LearningExperienceKeyIssues = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LearningExperienceKeyIssues)).Select(f => f.LearningExperienceKeyIssues).Distinct()),
                LearningExperienceStrengths = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LearningExperienceStrengths)).Select(f => f.LearningExperienceStrengths).Distinct()),
                LearningExperienceImprovements = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LearningExperienceImprovements)).Select(f => f.LearningExperienceImprovements).Distinct()),
                LearningExperienceComments = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LearningExperienceComments)).Select(f => f.LearningExperienceComments).Distinct()),
                LearningExperienceRating = (int)Math.Round(feedbacks.Average(f => f.LearningExperienceRating)),

                // Teaching & Learning Methods Questions 
                ConceptsPresentedYesCount = feedbacks.Count(f => f.ConceptsPresented == true),
                ConceptsPresentedNoCount = feedbacks.Count(f => f.ConceptsPresented == false),
                ConceptsPresentedNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.ConceptsPresentedNotes)).Select(f => f.ConceptsPresentedNotes).Distinct()),

                MaterialsAvailableYesCount = feedbacks.Count(f => f.MaterialsAvailable == true),
                MaterialsAvailableNoCount = feedbacks.Count(f => f.MaterialsAvailable == false),
                MaterialsAvailableNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.MaterialsAvailableNotes)).Select(f => f.MaterialsAvailableNotes).Distinct()),

                AccommodatesStylesYesCount = feedbacks.Count(f => f.AccommodatesStyles == true),
                AccommodatesStylesNoCount = feedbacks.Count(f => f.AccommodatesStyles == false),
                AccommodatesStylesNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AccommodatesStylesNotes)).Select(f => f.AccommodatesStylesNotes).Distinct()),

                LecturerResponsiveYesCount = feedbacks.Count(f => f.LecturerResponsive == true),
                LecturerResponsiveNoCount = feedbacks.Count(f => f.LecturerResponsive == false),
                LecturerResponsiveNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LecturerResponsiveNotes)).Select(f => f.LecturerResponsiveNotes).Distinct()),

                LearningTeachingKeyIssues = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LearningTeachingKeyIssues)).Select(f => f.LearningTeachingKeyIssues).Distinct()),
                LearningTeachingStrengths = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LearningTeachingStrengths)).Select(f => f.LearningTeachingStrengths).Distinct()),
                LearningTeachingImprovements = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LearningTeachingImprovements)).Select(f => f.LearningTeachingImprovements).Distinct()),
                LearningTeachingComments = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LearningTeachingComments)).Select(f => f.LearningTeachingComments).Distinct()),
                LearningTeachingRating = (int)Math.Round(feedbacks.Average(f => f.LearningTeachingRating)),

                // Assessment & Progress Tracking Questions 
                AssessmentConfidenceYesCount = feedbacks.Count(f => f.AssessmentConfidence == true),
                AssessmentConfidenceNoCount = feedbacks.Count(f => f.AssessmentConfidence == false),
                AssessmentConfidenceNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AssessmentConfidenceNotes)).Select(f => f.AssessmentConfidenceNotes).Distinct()),

                TimelyFeedbackYesCount = feedbacks.Count(f => f.TimelyFeedback == true),
                TimelyFeedbackNoCount = feedbacks.Count(f => f.TimelyFeedback == false),
                TimelyFeedbackNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.TimelyFeedbackNotes)).Select(f => f.TimelyFeedbackNotes).Distinct()),

                SpecificFeedbackYesCount = feedbacks.Count(f => f.SpecificFeedback == true),
                SpecificFeedbackNoCount = feedbacks.Count(f => f.SpecificFeedback == false),
                SpecificFeedbackNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SpecificFeedbackNotes)).Select(f => f.SpecificFeedbackNotes).Distinct()),

                AssessmentsAlignedYesCount = feedbacks.Count(f => f.AssessmentsAligned == true),
                AssessmentsAlignedNoCount = feedbacks.Count(f => f.AssessmentsAligned == false),
                AssessmentsAlignedNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AssessmentsAlignedNotes)).Select(f => f.AssessmentsAlignedNotes).Distinct()),

                SufficientTimeYesCount = feedbacks.Count(f => f.SufficientTime == true),
                SufficientTimeNoCount = feedbacks.Count(f => f.SufficientTime == false),
                SufficientTimeNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SufficientTimeNotes)).Select(f => f.SufficientTimeNotes).Distinct()),

                AssessmentKeyIssues = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AssessmentKeyIssues)).Select(f => f.AssessmentKeyIssues).Distinct()),
                AssessmentStrengths = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AssessmentStrengths)).Select(f => f.AssessmentStrengths).Distinct()),
                AssessmentImprovements = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AssessmentImprovements)).Select(f => f.AssessmentImprovements).Distinct()),
                AssessmentComments = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AssessmentComments)).Select(f => f.AssessmentComments).Distinct()),
                AssessmentRating = (int)Math.Round(feedbacks.Average(f => f.AssessmentRating)),

                // Learning Resources & Environment Questions 
                MaterialsAccessibleYesCount = feedbacks.Count(f => f.MaterialsAccessible == true),
                MaterialsAccessibleNoCount = feedbacks.Count(f => f.MaterialsAccessible == false),
                MaterialsAccessibleNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.MaterialsAccessibleNotes)).Select(f => f.MaterialsAccessibleNotes).Distinct()),

                PlatformOrganisedYesCount = feedbacks.Count(f => f.PlatformOrganised == true),
                PlatformOrganisedNoCount = feedbacks.Count(f => f.PlatformOrganised == false),
                PlatformOrganisedNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.PlatformOrganisedNotes)).Select(f => f.PlatformOrganisedNotes).Distinct()),

                EquipmentWorkingYesCount = feedbacks.Count(f => f.EquipmentWorking == true),
                EquipmentWorkingNoCount = feedbacks.Count(f => f.EquipmentWorking == false),
                EquipmentWorkingNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.EquipmentWorkingNotes)).Select(f => f.EquipmentWorkingNotes).Distinct()),

                SupplementaryResourcesYesCount = feedbacks.Count(f => f.SupplementaryResources == true),
                SupplementaryResourcesNoCount = feedbacks.Count(f => f.SupplementaryResources == false),
                SupplementaryResourcesNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SupplementaryResourcesNotes)).Select(f => f.SupplementaryResourcesNotes).Distinct()),

                SpecialisedEquipmentYesCount = feedbacks.Count(f => f.SpecializedEquipment == true),
                SpecialisedEquipmentNoCount = feedbacks.Count(f => f.SpecializedEquipment == false),
                SpecialisedEquipmentNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SpecializedEquipmentNotes)).Select(f => f.SpecializedEquipmentNotes).Distinct()),

                LibraryResourcesYesCount = feedbacks.Count(f => f.LibraryResources == true),
                LibraryResourcesNoCount = feedbacks.Count(f => f.LibraryResources == false),
                LibraryResourcesNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.LibraryResourcesNotes)).Select(f => f.LibraryResourcesNotes).Distinct()),

                ResourcesKeyIssues = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.ResourcesKeyIssues)).Select(f => f.ResourcesKeyIssues).Distinct()),
                ResourcesStrengths = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.ResourcesStrengths)).Select(f => f.ResourcesStrengths).Distinct()),
                ResourcesImprovements = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.ResourcesImprovements)).Select(f => f.ResourcesImprovements).Distinct()),
                ResourcesComments = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.ResourcesComments)).Select(f => f.ResourcesComments).Distinct()),
                ResourcesRating = (int)Math.Round(feedbacks.Average(f => f.ResourcesRating)),

                // Communication & Support Questions - maintaining consistency with previous sections
                StaffResponsiveYesCount = feedbacks.Count(f => f.StaffResponsive == true),
                StaffResponsiveNoCount = feedbacks.Count(f => f.StaffResponsive == false),
                StaffResponsiveNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.StaffResponsiveNotes)).Select(f => f.StaffResponsiveNotes).Distinct()),

                AdditionalHelpAvailableYesCount = feedbacks.Count(f => f.AdditionalHelpAvailable == true),
                AdditionalHelpAvailableNoCount = feedbacks.Count(f => f.AdditionalHelpAvailable == false),
                AdditionalHelpAvailableNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AdditionalHelpAvailableNotes)).Select(f => f.AdditionalHelpAvailableNotes).Distinct()),

                AccommodationsProvidedYesCount = feedbacks.Count(f => f.AccommodationsProvided == true),
                AccommodationsProvidedNoCount = feedbacks.Count(f => f.AccommodationsProvided == false),
                AccommodationsProvidedNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AccommodationsProvidedNotes)).Select(f => f.AccommodationsProvidedNotes).Distinct()),

                ClearPointsOfContactYesCount = feedbacks.Count(f => f.ClearPointsOfContact == true),
                ClearPointsOfContactNoCount = feedbacks.Count(f => f.ClearPointsOfContact == false),
                ClearPointsOfContactNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.ClearPointsOfContactNotes)).Select(f => f.ClearPointsOfContactNotes).Distinct()),

                SupportEffectivenesssKeyIssues = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SupportEffectivenesssKeyIssues)).Select(f => f.SupportEffectivenesssKeyIssues).Distinct()),
                SupportEffectivenessStrengths = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SupportEffectivenessStrengths)).Select(f => f.SupportEffectivenessStrengths).Distinct()),
                SupportEffectivenessImprovements = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SupportEffectivenessImprovements)).Select(f => f.SupportEffectivenessImprovements).Distinct()),
                SupportEffectivenessComments = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SupportEffectivenessComments)).Select(f => f.SupportEffectivenessComments).Distinct()),
                SupportEffectivenessRating = (int)Math.Round(feedbacks.Average(f => f.SupportEffectivenessRating)),

                // Skills Development Questions - final section using the same aggregation logic
                DevelopingCriticalThinkingYesCount = feedbacks.Count(f => f.DevelopingCriticalThinking == true),
                DevelopingCriticalThinkingNoCount = feedbacks.Count(f => f.DevelopingCriticalThinking == false),
                DevelopingCriticalThinkingNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.DevelopingCriticalThinkingNotes)).Select(f => f.DevelopingCriticalThinkingNotes).Distinct()),

                EnhancingProblemSolvingYesCount = feedbacks.Count(f => f.EnhancingProblemSolving == true),
                EnhancingProblemSolvingNoCount = feedbacks.Count(f => f.EnhancingProblemSolving == false),
                EnhancingProblemSolvingNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.EnhancingProblemSolvingNotes)).Select(f => f.EnhancingProblemSolvingNotes).Distinct()),

                GainingPracticalSkillsYesCount = feedbacks.Count(f => f.GainingPracticalSkills == true),
                GainingPracticalSkillsNoCount = feedbacks.Count(f => f.GainingPracticalSkills == false),
                GainingPracticalSkillsNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.GainingPracticalSkillsNotes)).Select(f => f.GainingPracticalSkillsNotes).Distinct()),

                ImprovingCommunicationYesCount = feedbacks.Count(f => f.ImprovingCommunication == true),
                ImprovingCommunicationNoCount = feedbacks.Count(f => f.ImprovingCommunication == false),
                ImprovingCommunicationNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.ImprovingCommunicationNotes)).Select(f => f.ImprovingCommunicationNotes).Distinct()),

                DevelopingResearchSkillsYesCount = feedbacks.Count(f => f.DevelopingResearchSkills == true),
                DevelopingResearchSkillsNoCount = feedbacks.Count(f => f.DevelopingResearchSkills == false),
                DevelopingResearchSkillsNotes = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.DevelopingResearchSkillsNotes)).Select(f => f.DevelopingResearchSkillsNotes).Distinct()),

                SkillsDevelopmentKeyIssues = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SkillsDevelopmentKeyIssues)).Select(f => f.SkillsDevelopmentKeyIssues).Distinct()),
                SkillsDevelopmentStrengths = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SkillsDevelopmentStrengths)).Select(f => f.SkillsDevelopmentStrengths).Distinct()),
                SkillsDevelopmentImprovements = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SkillsDevelopmentImprovements)).Select(f => f.SkillsDevelopmentImprovements).Distinct()),
                SkillsDevelopmentComments = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.SkillsDevelopmentComments)).Select(f => f.SkillsDevelopmentComments).Distinct()),
                SkillsDevelopmentRating = (int)Math.Round(feedbacks.Average(f => f.SkillsDevelopmentRating)),

                // Overall summary questions - combining the final thoughts from all students
                OverallRating = (int)Math.Round(feedbacks.Average(f => f.OverallRating)),
                BestFeatures = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.BestFeatures)).Select(f => f.BestFeatures).Distinct()),
                AreasForImprovement = string.Join("\n ", feedbacks.Where(f => !string.IsNullOrWhiteSpace(f.AreasForImprovement)).Select(f => f.AreasForImprovement).Distinct())
            };
        }

        #endregion
    }
}
