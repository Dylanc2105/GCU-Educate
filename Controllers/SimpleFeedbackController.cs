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
    /// <summary>
    /// Controller responsible for managing simple feedback operations in the Guidance Tracker application.
    /// Handles student feedback creation, viewing, editing, and deletion with role-based access controls.
    /// Supports feedback submission to guidance teachers and curriculum heads with optional unit associations.
    /// Also manages class representative functionality for detailed feedback aggregation workflows.
    /// </summary>
    public class SimpleFeedbackController : Controller
    {
        #region Private Fields

        /// <summary>
        /// Database context for accessing and manipulating guidance tracker data.
        /// </summary>
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        #endregion

        #region Student Dashboard

        /// <summary>
        /// Displays the main feedback dashboard for students with dynamic content based on their role and status.
        /// Shows different interface elements for regular students versus class representatives.
        /// For class representatives, displays status information about detailed feedback collection processes.
        /// </summary>
        /// <returns>
        /// A view containing dashboard information tailored to the student's role and current feedback status.
        /// </returns>
        /// <remarks>
        /// This method provides a comprehensive dashboard experience by:
        /// 1. Determining if the current student is a class representative
        /// 2. For class reps: calculating detailed feedback collection status including submission counts and pending requests
        /// 3. For all students: checking for pending detailed feedback requests assigned to them
        /// 4. Setting appropriate ViewBag properties to control dashboard UI elements
        /// 
        /// Dashboard Status Indicators:
        /// - HasPendingClassFeedback: Shows when class rep can review aggregated results
        /// - HasSubmittedClassFeedback: Indicates if class rep has already submitted final feedback
        /// - HasDetailedFeedbackRequest: Shows if student has pending detailed feedback to complete
        /// 
        /// The dashboard serves as a central hub for both simple feedback operations and complex
        /// detailed feedback workflows, adapting its interface based on user permissions and current state.
        /// </remarks>
        public ActionResult StudentFeedbackDashboard()
        {
            var studentId = User.Identity.GetUserId();
            var currentStudent = db.Students.FirstOrDefault(s => s.Id == studentId);

            ViewBag.IsClassRep = currentStudent?.IsClassRep ?? false;

            // Handle class representative specific functionality
            if (currentStudent?.IsClassRep == true)
            {
                // Calculate detailed feedback collection status for class representative oversight
                var totalStudentsInClass = db.Students.Count(s => s.ClassId == currentStudent.ClassId);
                var submittedFeedbacks = db.DetailedFeedbacks
                    .Count(f => f.ClassId == currentStudent.ClassId && f.IsSubmitted == true);
                var pendingRequests = db.RequestedDetailedForms
                    .Count(r => r.ClassId == currentStudent.ClassId);

                // Check if class rep has already completed their aggregation responsibilities
                var submittedClassRepFeedback = db.ClassRepFeedbacks
                    .Any(f => f.ClassId == currentStudent.ClassId &&
                             f.StudentId == currentStudent.Id &&
                             f.IsSubmittedByClassRep == true);

                // Determine if class rep can proceed with feedback aggregation
                ViewBag.HasPendingClassFeedback = (submittedFeedbacks >= totalStudentsInClass &&
                                                  pendingRequests == 0 &&
                                                  !submittedClassRepFeedback);
                ViewBag.HasSubmittedClassFeedback = submittedClassRepFeedback;
            }

            // Check for detailed feedback requests assigned to this student
            var hasRequest = db.RequestedDetailedForms.Any(r => r.StudentId == studentId);

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

        #endregion

        #region Create Simple Feedback

        /// <summary>
        /// Displays the form for creating new simple feedback with pre-populated recipient and unit options.
        /// Loads the student's guidance teacher and curriculum head information for easy selection.
        /// Populates unit dropdown with courses the student is enrolled in.
        /// </summary>
        /// <returns>
        /// A view containing the SimpleStudentFeedbackViewModel with populated dropdown options and recipient information.
        /// </returns>
        /// <remarks>
        /// This GET action prepares the feedback creation form by:
        /// 1. Loading the current student with all necessary related entities (guidance teacher, class, course, department)
        /// 2. Pre-populating recipient information including guidance teacher and curriculum head details
        /// 3. Loading available units based on the student's course enrollment
        /// 4. Setting up the view model with all necessary data for form rendering
        /// 
        /// The form allows students to:
        /// - Enter feedback title and content
        /// - Select optional unit association
        /// - Choose recipients (guidance teacher and/or curriculum head)
        /// - Submit feedback for review and response
        /// 
        /// Access is restricted to students only through the Authorize attribute.
        /// </remarks>
        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult CreateSimpleFeedback()
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Include(s => s.Class).FirstOrDefault(s => s.Id == studentId);

            // Load the student with comprehensive relationship data for recipient identification
            student = db.Students
                .Include("GuidanceTeacher")
                .Include("Class.Enrollments.Course.Department.CurriculumHead")
                .FirstOrDefault(s => s.Id == studentId);

            var viewModel = new SimpleStudentFeedbackViewModel
            {
                SendToGuidanceTeacher = false,
                SendToCurriculumHead = false
            };

            // Set up guidance teacher information if available
            if (student.GuidanceTeacher != null)
            {
                viewModel.GuidanceTeacherId = student.GuidanceTeacherId;
                viewModel.GuidanceTeacherName = $"{student.GuidanceTeacher.FirstName} {student.GuidanceTeacher.LastName}";
            }

            // Set up curriculum head information if available
            var ch = student.Class?.Enrollments.FirstOrDefault()?.Course?.Department?.CurriculumHead;
            if (ch != null)
            {
                viewModel.CurriculumHeadId = ch.Id;
                viewModel.CurriculumHeadName = $"{ch.FirstName} {ch.LastName}";
            }

            // Load units available to the student based on their class enrollment
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

        /// <summary>
        /// Processes the creation of new simple feedback with validation and recipient assignment.
        /// Handles recipient selection validation and saves feedback to the database.
        /// Supports feedback submission to guidance teachers and/or curriculum heads.
        /// </summary>
        /// <param name="viewModel">The view model containing the feedback data from the form submission.</param>
        /// <returns>
        /// On success: Redirects to Student Dashboard with success message.
        /// On validation failure: Returns the create view with validation errors and reloaded dropdown data.
        /// </returns>
        /// <remarks>
        /// This POST action handles the complete feedback creation process:
        /// 1. Validates the submitted form data including recipient selection
        /// 2. Creates a new SimpleFeedback entity with timestamp and student association
        /// 3. Assigns recipients based on user selection (guidance teacher and/or curriculum head)
        /// 4. Validates that at least one recipient is selected
        /// 5. Saves the feedback to the database and provides user confirmation
        /// 
        /// Validation Rules:
        /// - At least one recipient must be selected (guidance teacher or curriculum head)
        /// - All standard model validation rules must pass
        /// - Unit association is optional
        /// 
        /// On successful submission, the user receives a success message and is redirected
        /// to their dashboard. On failure, the form is redisplayed with error messages
        /// and repopulated dropdown data.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSimpleFeedback(SimpleStudentFeedbackViewModel viewModel)
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Include(s => s.Class).FirstOrDefault(s => s.Id == studentId);

            if (ModelState.IsValid)
            {
                // Create new feedback entity with basic information
                var feedback = new SimpleFeedback
                {
                    FeedbackTitle = viewModel.Title,
                    FeedbackContent = viewModel.content,
                    DateOfCreation = DateTime.UtcNow,
                    StudentId = studentId,
                    UnitId = viewModel.UnitId
                };

                bool hasRecipient = false;

                // Assign guidance teacher as recipient if selected
                if (viewModel.SendToGuidanceTeacher && !string.IsNullOrEmpty(viewModel.GuidanceTeacherId))
                {
                    feedback.GuidanceTeacherId = viewModel.GuidanceTeacherId;
                    feedback.Reciepient = Reciepient.GuidanceTeacher;
                    hasRecipient = true;
                }

                // Assign curriculum head as recipient if selected
                if (viewModel.SendToCurriculumHead && !string.IsNullOrEmpty(viewModel.CurriculumHeadId))
                {
                    feedback.CurriculumHeadId = viewModel.CurriculumHeadId;
                    feedback.Reciepient = Reciepient.CurriculumHead;
                    hasRecipient = true;
                }

                // Validate that at least one recipient is selected
                if (!hasRecipient)
                {
                    ModelState.AddModelError("", "Please select at least one recipient");
                    return View(viewModel);
                }

                // Save feedback and provide user confirmation
                db.SimpleFeedbacks.Add(feedback);
                db.SaveChanges();

                TempData["Success"] = "Feedback submitted successfully!";
                return RedirectToAction("StudentDash", "Student");
            }

            // If validation fails, reload unit dropdown data
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

        #endregion

        #region View Simple Feedbacks

        /// <summary>
        /// Displays all simple feedbacks created by the currently logged-in student.
        /// Shows feedback history with recipient and unit information for student review.
        /// </summary>
        /// <returns>
        /// A view containing a list of all simple feedbacks submitted by the current student,
        /// including related entity information for guidance teachers, curriculum heads, and units.
        /// </returns>
        /// <remarks>
        /// This method provides students with a comprehensive view of their feedback history by:
        /// 1. Filtering feedbacks to show only those created by the current student
        /// 2. Including related entity data (guidance teachers, curriculum heads, units) for display
        /// 3. Presenting feedbacks in a list format for easy review and management
        /// 
        /// The view typically includes:
        /// - Feedback title and creation date
        /// - Recipient information (guidance teacher or curriculum head)
        /// - Associated unit (if specified)
        /// - Read status indicators
        /// - Action links for viewing details, editing, or deleting
        /// 
        /// This serves as a personal feedback management interface for students to track
        /// their submissions and monitor response status.
        /// </remarks>
        public ActionResult ViewSimpleFeedbacks()
        {
            var studentId = User.Identity.GetUserId();

            // Load all feedbacks for the current student with related entity information
            var feedbacks = db.SimpleFeedbacks
                .Include(f => f.GuidanceTeacher)
                .Include(f => f.CurriculumHead)
                .Include(f => f.Unit)
                .Where(f => f.StudentId == studentId)
                .ToList();

            return View(feedbacks);
        }

        #endregion

        #region Delete Simple Feedback

        /// <summary>
        /// Deletes a specific simple feedback entry from the database.
        /// Provides immediate feedback removal without confirmation dialog.
        /// </summary>
        /// <param name="id">The unique identifier of the feedback to delete.</param>
        /// <returns>
        /// Redirects to ViewSimpleFeedbacks action after deletion attempt.
        /// </returns>
        /// <remarks>
        /// This method handles feedback deletion by:
        /// 1. Locating the feedback by its unique identifier
        /// 2. Removing the feedback from the database if found
        /// 3. Saving changes to commit the deletion
        /// 4. Redirecting back to the feedback list view
        /// 
        /// Note: This method does not include explicit authorization checks,
        /// so it relies on application-level security to ensure only appropriate
        /// users can access this action. In a production environment, additional
        /// security validation should verify that the current user owns the feedback
        /// being deleted.
        /// 
        /// The method gracefully handles cases where the feedback is not found
        /// by simply proceeding with the redirect without throwing errors.
        /// </remarks>
        public ActionResult DeleteSimpleFeedback(int id)
        {
            // Locate and remove the specified feedback
            var feedback = db.SimpleFeedbacks.Find(id);
            if (feedback != null)
            {
                db.SimpleFeedbacks.Remove(feedback);
                db.SaveChanges();
            }

            return RedirectToAction("ViewSimpleFeedbacks");
        }

        #endregion

        #region View Feedback Details

        /// <summary>
        /// Displays detailed information for a specific simple feedback with role-based access control.
        /// Automatically marks feedback as read when viewed by the intended recipient.
        /// Supports access by students (feedback authors), guidance teachers, and curriculum heads.
        /// </summary>
        /// <param name="id">The unique identifier of the feedback to display.</param>
        /// <returns>
        /// A view showing the complete feedback details including student information, content, and recipient data.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when the current user does not have permission to view the specified feedback.
        /// </exception>
        /// <remarks>
        /// This method implements comprehensive access control and read tracking:
        /// 
        /// Access Control Rules:
        /// - Guidance Teachers: Can view feedbacks where they are the assigned recipient
        /// - Curriculum Heads: Can view feedbacks where they are the assigned recipient
        /// - Students: Can view feedbacks they authored
        /// 
        /// Read Tracking Functionality:
        /// - When guidance teachers view feedback, IsReadByGuidanceTeacher is set to true
        /// - When curriculum heads view feedback, IsReadByCurriculumHead is set to true
        /// - Students can view without affecting read status (they authored the feedback)
        /// 
        /// The method loads comprehensive related entity data including:
        /// - Student information (feedback author)
        /// - Guidance teacher details (if applicable)
        /// - Curriculum head details (if applicable)
        /// - Unit information (if associated)
        /// 
        /// This ensures that all stakeholders have appropriate access to feedback information
        /// while maintaining security boundaries and providing accurate read status tracking
        /// for administrative oversight.
        /// </remarks>
        public ActionResult ViewSimpleFeedbackDetails(int id)
        {
            var currentUserId = User.Identity.GetUserId();

            // Load feedback with all related entity information
            var feedback = db.SimpleFeedbacks
                .Include(f => f.Student)
                .Include(f => f.GuidanceTeacher)
                .Include(f => f.CurriculumHead)
                .Include(f => f.Unit)
                .FirstOrDefault(f => f.FeedbackId == id);

            // Handle access control and read tracking for guidance teachers
            if (User.IsInRole("GuidanceTeacher") && feedback.GuidanceTeacherId == currentUserId)
            {
                if (!feedback.IsReadByGuidanceTeacher)
                {
                    feedback.IsReadByGuidanceTeacher = true;
                    db.SaveChanges();
                }
                return View(feedback);
            }
            // Handle access control and read tracking for curriculum heads
            else if (User.IsInRole("CurriculumHead") && feedback.CurriculumHeadId == currentUserId)
            {
                if (!feedback.IsReadByCurriculumHead)
                {
                    feedback.IsReadByCurriculumHead = true;
                    db.SaveChanges();
                }
                return View(feedback);
            }
            // Handle access control for student authors
            else if (User.IsInRole("Student") && feedback.StudentId == currentUserId)
            {
                return View(feedback);
            }

            // Deny access for unauthorized users
            throw new UnauthorizedAccessException("You do not have permission to view this feedback.");
        }

        #endregion

        #region Edit Simple Feedback

        /// <summary>
        /// Displays the edit form for an existing simple feedback with current values pre-populated.
        /// Allows students to modify their feedback content and recipient selections.
        /// </summary>
        /// <param name="id">The unique identifier of the feedback to edit.</param>
        /// <returns>
        /// On success: A view with the edit form pre-populated with existing feedback data.
        /// On feedback not found: HTTP 404 Not Found result.
        /// </returns>
        /// <remarks>
        /// This GET action prepares the edit form by:
        /// 1. Locating the feedback by its unique identifier
        /// 2. Creating a view model populated with existing feedback data
        /// 3. Setting recipient selection flags based on current assignments
        /// 4. Presenting the edit form for user modification
        /// 
        /// Pre-populated Data:
        /// - Feedback title and content
        /// - Associated unit selection
        /// - Recipient selection status (guidance teacher and/or curriculum head)
        /// 
        /// Note: This method currently does not include authorization checks to verify
        /// that the current user owns the feedback being edited. In a production environment,
        /// additional security validation should be implemented to ensure only feedback
        /// authors can modify their submissions.
        /// 
        /// The method returns HTTP 404 if the specified feedback cannot be found,
        /// providing appropriate error handling for invalid feedback identifiers.
        /// </remarks>
        public ActionResult EditSimpleFeedback(int id)
        {
            // Locate the specified feedback
            var feedback = db.SimpleFeedbacks.Find(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }

            // Create view model with existing feedback data
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

        #endregion

        #region Resource Management

        /// <summary>
        /// Disposes of the database context when the controller is disposed.
        /// Ensures proper cleanup of database connections and resources.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources; otherwise, false.</param>
        /// <remarks>
        /// This method is called by the framework when the controller is being disposed.
        /// It ensures that the database context is properly disposed to prevent memory leaks
        /// and connection pool exhaustion.
        /// 
        /// The disposing parameter indicates whether the method is being called from
        /// the Dispose method (true) or from the finalizer (false).
        /// 
        /// Proper resource disposal is essential for maintaining application performance
        /// and preventing resource leaks in long-running applications.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}