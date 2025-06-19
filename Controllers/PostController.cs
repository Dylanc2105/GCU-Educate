using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.UI.WebControls;
using GuidanceTracker.Models.ViewModels;
using System.Data.Entity.Validation;
using System.Net;
using Antlr.Runtime.Misc;
using Humanizer;
using Newtonsoft.Json.Linq;
using static Humanizer.In;
using System.Web.Services.Description;
using System.Security.Principal;

namespace GuidanceTracker.Controllers
{
    /// <summary>
    /// Controller responsible for managing post-related operations in the Guidance Tracker application.
    /// Handles creating, viewing, editing, and deleting posts with visibility controls based on user roles and relationships.
    /// Supports different visibility levels including Global, Student, Staff, Department, Course, and Class-specific posts.
    /// </summary>
    public class PostController : Controller
    {
        #region Private Fields
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
        #endregion

        #region View My Posts

        /// <summary>
        /// Displays all posts created by the currently logged-in user.
        /// Shows posts in descending order by post date (newest first).
        /// </summary>
        /// <returns>
        /// A view containing a list of posts authored by the current user, ordered by post date descending.
        /// </returns>
        /// <remarks>
        /// This method filters posts to show only those where the AuthorId matches the current user's ID.
        /// No visibility filtering is applied since users can see all their own posts regardless of visibility settings.
        /// </remarks>
        public ActionResult ViewMyPosts()
        {
            // Get the current user's unique identifier
            var currentUserId = User.Identity.GetUserId();

            // Retrieve all posts where the current user is the author
            var visiblePosts = db.Posts.Where(p => p.AuthorId == currentUserId).ToList();

            // Order posts by date in descending order (newest first)
            var posts = visiblePosts
            .OrderByDescending(p => p.PostDate)
            .ToList();

            return View(posts);
        }

        #endregion

        #region View Posts

        /// <summary>
        /// Displays all posts visible to the current user based on their role and relationships.
        /// Automatically marks unread posts as read when viewed and provides unread count information.
        /// </summary>
        /// <returns>
        /// A view containing all posts visible to the current user, with ViewBag data for unread counts and read post tracking.
        /// </returns>
        /// <remarks>
        /// This method uses the PostVisibilityHelper to determine which posts the user can see based on:
        /// - User role (Student, Lecturer, GuidanceTeacher, CurriculumHead)
        /// - Department, Course, and Class relationships
        /// - Post visibility settings (Global, Student, Staff, Department, Course, Class)
        /// 
        /// The method also handles read/unread tracking by:
        /// - Identifying posts not yet marked as read by the current user
        /// - Creating PostRead records for newly viewed posts
        /// - Providing unread count and read post IDs to the view
        /// </remarks>
        public ActionResult ViewPosts()
        {
            var currentUserId = User.Identity.GetUserId();

            // Use the PostVisibilityHelper to get posts visible to the current user
            // Include Author information for display purposes
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(currentUserId, db, User)
                .Include(p => p.Author)
                .OrderByDescending(p => p.PostDate)
                .ToList();

            // Get IDs of posts already marked as read by the current user
            var readPostIds = db.PostReads
                .Where(pr => pr.UserId == currentUserId)
                .Select(pr => pr.PostId)
                .ToList();

            // Identify posts that haven't been read yet
            var unreadPosts = visiblePosts
                .Where(p => !readPostIds.Contains(p.PostId))
                .ToList();

            // Mark unread posts as read by creating PostRead records
            foreach (var unread in unreadPosts)
            {
                db.PostReads.Add(new PostRead
                {
                    PostId = unread.PostId,
                    UserId = currentUserId,
                    ReadOn = DateTime.UtcNow
                });
            }

            // Save the new PostRead records to the database
            db.SaveChanges();

            // Provide unread count and read post IDs to the view for UI indicators
            ViewBag.UnreadCount = unreadPosts.Count;
            ViewBag.ReadPostIds = readPostIds;

            return View(visiblePosts);
        }
        #endregion

        #region Create a New Post

        /// <summary>
        /// Displays the form for creating a new post with all necessary dropdown options for visibility settings.
        /// Prepares view model with visibility options and related entity selections (departments, courses, classes, units).
        /// </summary>
        /// <returns>
        /// A view containing the CreatePostViewModel with populated dropdown options for post creation.
        /// </returns>
        /// <remarks>
        /// This GET action prepares the create post form by:
        /// - Setting up visibility type options (Global, Student, Staff, Department, Course, Class)
        /// - Loading all departments, courses, classes, and units for dropdown selections
        /// - Ordering all dropdown items alphabetically for better user experience
        /// 
        /// The form supports granular visibility control where users can specify exactly who should see their posts.
        /// </remarks>
        [HttpGet]
        [Authorize]
        public ActionResult CreateNewPost()
        {
            var viewModel = new CreatePostViewModel();

            // Set up the visibility dropdown with all available visibility types
            viewModel.VisibilityOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = VisibilityType.Global.ToString(), Text = "Global (Everyone)" },
                new SelectListItem { Value = VisibilityType.Student.ToString(), Text = "Students Only" },
                new SelectListItem { Value = VisibilityType.Staff.ToString(), Text = "Lecturers Only" },
                new SelectListItem { Value = VisibilityType.Department.ToString(), Text = "Department" },
                new SelectListItem { Value = VisibilityType.Course.ToString(), Text = "Course" },
                new SelectListItem { Value = VisibilityType.Class.ToString(), Text = "Class" },
            };

            // Load dropdown data for departments, ordered alphabetically
            viewModel.Departments = db.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                })
                .ToList();

            // Load dropdown data for courses, ordered alphabetically
            viewModel.Courses = db.Courses
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                })
                .ToList();

            // Load dropdown data for classes, ordered alphabetically
            viewModel.Classes = db.Classes
                .OrderBy(c => c.ClassName)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.ClassName
                })
                .ToList();

            // Load dropdown data for units, ordered alphabetically
            viewModel.Units = db.Units
                .OrderBy(u => u.UnitName)
                .Select(u => new SelectListItem
                {
                    Value = u.UnitId.ToString(),
                    Text = u.UnitName
                })
                .ToList();

            return View(viewModel);
        }


        /// <summary>
        /// Processes the creation of a new post with the provided form data.
        /// Handles visibility-specific relationships and sends notifications to all users about the new post.
        /// </summary>
        /// <param name="viewModel">The view model containing the post data from the form submission.</param>
        /// <returns>
        /// On success: Redirects to ViewPosts action.
        /// On validation failure: Returns the create view with validation errors and reloaded dropdown data.
        /// </returns>
        /// <remarks>
        /// This POST action handles the complete post creation process:
        /// 1. Validates the submitted form data
        /// 2. Creates a new Post entity with a unique GUID identifier
        /// 3. Sets up visibility-specific relationships (Department, Course, or Class associations)
        /// 4. Creates notifications for all users in the system about the new post
        /// 5. Handles database validation errors gracefully
        /// 
        /// The method creates different entity relationships based on the selected visibility:
        /// - Department visibility: Associates post with selected department
        /// - Course visibility: Associates post with selected course
        /// - Class visibility: Associates post with selected class
        /// 
        /// All users receive notifications regardless of whether they can actually view the post,
        /// as visibility filtering happens at the view level.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateNewPost(CreatePostViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Create new post entity with unique identifier and current user as author
                var post = new Post
                {
                    PostId = Guid.NewGuid().ToString(),
                    Title = viewModel.Title,
                    Content = viewModel.Content,
                    AuthorId = User.Identity.GetUserId(),
                    PostDate = DateTime.UtcNow,
                    Visibility = viewModel.Visibility
                };

                // Set up visibility-specific entity relationships
                switch (viewModel.Visibility)
                {
                    case VisibilityType.Department:
                        if (!string.IsNullOrEmpty(viewModel.DepartmentId))
                        {
                            var department = db.Departments.Find(viewModel.DepartmentId);
                            if (department != null)
                            {
                                post.Departments.Add(department);
                            }
                        }
                        break;

                    case VisibilityType.Course:
                        if (viewModel.CourseId.HasValue)
                        {
                            var course = db.Courses.Find(viewModel.CourseId.Value);
                            if (course != null)
                            {
                                post.Courses.Add(course);
                            }
                        }
                        break;

                    case VisibilityType.Class:
                        if (viewModel.ClassId.HasValue)
                        {
                            var classEntity = db.Classes.Find(viewModel.ClassId.Value);
                            if (classEntity != null)
                            {
                                post.Classes.Add(classEntity);
                            }
                        }
                        break;
                }

                // Add the post to the database
                db.Posts.Add(post);

                // Create notifications for all users about the new post
                var users = db.Users.ToList();
                try
                {
                    foreach (var user in users)
                    {
                        db.Notifications.Add(new Notification
                        {
                            UserId = user.Id,
                            Type = NotificationType.Announcement,
                            Message = $"New announcement: {post.Title}",
                            RedirectUrl = "/Post/ViewPosts",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        });
                    }

                    db.SaveChanges();
                    return RedirectToAction("ViewPosts");
                }
                catch (DbEntityValidationException ex)
                {
                    // Log detailed validation errors for debugging
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                }
            }

            // If validation fails, reload all dropdown data for the form
            viewModel.VisibilityOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = VisibilityType.Global.ToString(), Text = "Global (Everyone)" },
                new SelectListItem { Value = VisibilityType.Student.ToString(), Text = "Students Only" },
                new SelectListItem { Value = VisibilityType.Staff.ToString(), Text = "Lecturers Only" },
                new SelectListItem { Value = VisibilityType.Department.ToString(), Text = "Department" },
                new SelectListItem { Value = VisibilityType.Course.ToString(), Text = "Course" },
                new SelectListItem { Value = VisibilityType.Class.ToString(), Text = "Class" },
            };

            viewModel.Departments = db.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                })
                .ToList();

            viewModel.Courses = db.Courses
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                })
                .ToList();

            viewModel.Classes = db.Classes
                .OrderBy(c => c.ClassName)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.ClassName
                })
                .ToList();

            viewModel.Units = db.Units
                .OrderBy(u => u.UnitName)
                .Select(u => new SelectListItem
                {
                    Value = u.UnitId.ToString(),
                    Text = u.UnitName
                })
            .ToList();

            return View(viewModel);
        }
        #endregion

        #region Edit Post

        /// <summary>
        /// Displays the edit form for an existing post, pre-populated with current values.
        /// Includes security checks to ensure only the post author can edit their posts.
        /// </summary>
        /// <param name="id">The unique identifier of the post to edit.</param>
        /// <returns>
        /// On success: A view with the edit form pre-populated with existing post data.
        /// On invalid ID: HTTP 400 Bad Request.
        /// On post not found: HTTP 404 Not Found.
        /// On unauthorized access: HTTP 403 Forbidden.
        /// </returns>
        /// <remarks>
        /// This GET action prepares the edit form by:
        /// 1. Validating the post ID parameter
        /// 2. Loading the post with all related entities (departments, courses, classes)
        /// 3. Performing security validation to ensure only the author can edit
        /// 4. Pre-populating the view model with existing values
        /// 5. Setting up all dropdown options with current selections marked
        /// 
        /// The method handles all visibility types and ensures the correct dropdown items
        /// are selected based on the post's current visibility settings and relationships.
        /// </remarks>
        [HttpGet]
        [Authorize]
        public ActionResult Edit(string id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Load the post with all related entities for visibility settings
            Post post = db.Posts
            .Include(p => p.Departments)
            .Include(p => p.Courses)
            .Include(p => p.Classes)
            .FirstOrDefault(p => p.PostId == id);

            if (post == null)
            {
                return HttpNotFound();
            }

            // Security check - only allow editing of own posts
            if (post.AuthorId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // Create view model with existing post data
            var viewModel = new CreatePostViewModel
            {
                Title = post.Title,
                Content = post.Content,
                Visibility = post.Visibility
            };

            ViewBag.PostId = post.PostId;

            // Set selected values based on the post's current visibility and relationships
            switch (post.Visibility)
            {
                case VisibilityType.Department:
                    if (post.Departments.Any())
                    {
                        viewModel.DepartmentId = post.Departments.First().DepartmentId;
                    }
                    break;
                case VisibilityType.Course:
                    if (post.Courses.Any())
                    {
                        viewModel.CourseId = post.Courses.First().CourseId;
                    }
                    break;
                case VisibilityType.Class:
                    if (post.Classes.Any())
                    {
                        viewModel.ClassId = post.Classes.First().ClassId;
                    }
                    break;
            }

            // Load visibility options with current selection marked
            viewModel.VisibilityOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = VisibilityType.Global.ToString(), Text = "Global (Everyone)", Selected = post.Visibility == VisibilityType.Global },
                new SelectListItem { Value = VisibilityType.Student.ToString(), Text = "Students Only", Selected = post.Visibility == VisibilityType.Student },
                new SelectListItem { Value = VisibilityType.Staff.ToString(), Text = "Lecturers Only", Selected = post.Visibility == VisibilityType.Staff },
                new SelectListItem { Value = VisibilityType.Department.ToString(), Text = "Department", Selected = post.Visibility == VisibilityType.Department },
                new SelectListItem { Value = VisibilityType.Course.ToString(), Text = "Course", Selected = post.Visibility == VisibilityType.Course },
                new SelectListItem { Value = VisibilityType.Class.ToString(), Text = "Class", Selected = post.Visibility == VisibilityType.Class },
            };

            // Load departments with current selection marked
            viewModel.Departments = db.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName,
                    Selected = post.Visibility == VisibilityType.Department && post.Departments.Any(pd => pd.DepartmentId == d.DepartmentId)
                })
                .ToList();

            // Load courses with current selection marked
            viewModel.Courses = db.Courses
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName,
                    Selected = post.Visibility == VisibilityType.Course && post.Courses.Any(pc => pc.CourseId == c.CourseId)
                })
                .ToList();

            // Load classes with current selection marked
            viewModel.Classes = db.Classes
                .OrderBy(c => c.ClassName)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.ClassName,
                    Selected = post.Visibility == VisibilityType.Class && post.Classes.Any(pc => pc.ClassId == c.ClassId)
                })
                .ToList();

            // Load units (no selection needed as units aren't used in visibility)
            viewModel.Units = db.Units
                .OrderBy(u => u.UnitName)
                .Select(u => new SelectListItem
                {
                    Value = u.UnitId.ToString(),
                    Text = u.UnitName
                })
                .ToList();

            ViewBag.PostId = id;
            return View(viewModel);
        }

        /// <summary>
        /// Processes the update of an existing post with the provided form data.
        /// Handles updating visibility-specific relationships and maintains security restrictions.
        /// </summary>
        /// <param name="id">The unique identifier of the post to update.</param>
        /// <param name="viewModel">The view model containing the updated post data from the form submission.</param>
        /// <returns>
        /// On success: Redirects to ViewPosts action.
        /// On invalid ID: HTTP 400 Bad Request.
        /// On post not found: HTTP 404 Not Found.
        /// On unauthorized access: HTTP 403 Forbidden.
        /// On validation failure: Returns the edit view with validation errors and reloaded dropdown data.
        /// </returns>
        /// <remarks>
        /// This POST action handles the complete post update process:
        /// 1. Validates the post ID and loads the existing post
        /// 2. Performs security validation to ensure only the author can edit
        /// 3. Updates the post's basic properties (title, content, visibility)
        /// 4. Clears all existing visibility-specific relationships
        /// 5. Establishes new relationships based on the updated visibility settings
        /// 6. Handles database validation errors gracefully
        /// 
        /// The method completely rebuilds visibility relationships on each update to ensure
        /// data consistency, as changing visibility types requires different entity associations.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(string id, CreatePostViewModel viewModel)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Load the post with all related entities
            Post post = db.Posts
                .Include(p => p.Departments)
                .Include(p => p.Courses)
                .Include(p => p.Classes)
                .FirstOrDefault(p => p.PostId == id);

            if (post == null)
            {
                return HttpNotFound();
            }

            // Security check - only allow editing of own posts
            if (post.AuthorId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                // Update basic properties
                post.Title = viewModel.Title;
                post.Content = viewModel.Content;
                post.Visibility = viewModel.Visibility;

                // Clear all existing relationships to rebuild them fresh
                post.Departments.Clear();
                post.Courses.Clear();
                post.Classes.Clear();

                // Add new relationships based on selected visibility
                switch (viewModel.Visibility)
                {
                    case VisibilityType.Department:
                        if (!string.IsNullOrEmpty(viewModel.DepartmentId))
                        {
                            var department = db.Departments.Find(viewModel.DepartmentId);
                            if (department != null)
                            {
                                post.Departments.Add(department);
                            }
                        }
                        break;

                    case VisibilityType.Course:
                        if (viewModel.CourseId.HasValue)
                        {
                            var course = db.Courses.Find(viewModel.CourseId.Value);
                            if (course != null)
                            {
                                post.Courses.Add(course);
                            }
                        }
                        break;

                    case VisibilityType.Class:
                        if (viewModel.ClassId.HasValue)
                        {
                            var classEntity = db.Classes.Find(viewModel.ClassId.Value);
                            if (classEntity != null)
                            {
                                post.Classes.Add(classEntity);
                            }
                        }
                        break;
                }

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("ViewPosts");
                }
                catch (DbEntityValidationException ex)
                {
                    // Log detailed validation errors for debugging
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                }
            }

            // If validation fails, reload dropdown data with current selections
            viewModel.VisibilityOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = VisibilityType.Global.ToString(), Text = "Global (Everyone)", Selected = post.Visibility == VisibilityType.Global },
                new SelectListItem { Value = VisibilityType.Student.ToString(), Text = "Students Only", Selected = post.Visibility == VisibilityType.Student },
                new SelectListItem { Value = VisibilityType.Staff.ToString(), Text = "Lecturers Only", Selected = post.Visibility == VisibilityType.Staff },
                new SelectListItem { Value = VisibilityType.Department.ToString(), Text = "Department", Selected = post.Visibility == VisibilityType.Department },
                new SelectListItem { Value = VisibilityType.Course.ToString(), Text = "Course", Selected = post.Visibility == VisibilityType.Course },
                new SelectListItem { Value = VisibilityType.Class.ToString(), Text = "Class", Selected = post.Visibility == VisibilityType.Class },
            };

            viewModel.Departments = db.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName,
                    Selected = post.Visibility == VisibilityType.Department && post.Departments.Any(pd => pd.DepartmentId == d.DepartmentId)
                })
                .ToList();

            viewModel.Courses = db.Courses
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName,
                    Selected = post.Visibility == VisibilityType.Course && post.Courses.Any(pc => pc.CourseId == c.CourseId)
                })
                .ToList();

            viewModel.Classes = db.Classes
                .OrderBy(c => c.ClassName)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.ClassName,
                    Selected = post.Visibility == VisibilityType.Class && post.Classes.Any(pc => pc.ClassId == c.ClassId)
                })
                .ToList();

            viewModel.Units = db.Units
                .OrderBy(u => u.UnitName)
                .Select(u => new SelectListItem
                {
                    Value = u.UnitId.ToString(),
                    Text = u.UnitName
                })
                .ToList();

            ViewBag.PostId = id;
            return View(viewModel);
        }

        #endregion

        #region Delete Post

        /// <summary>
        /// Displays the delete confirmation page for a specific post.
        /// Includes security checks to ensure only the post author can delete their posts.
        /// </summary>
        /// <param name="id">The unique identifier of the post to delete.</param>
        /// <returns>
        /// On success: A view showing the post details for deletion confirmation.
        /// On invalid ID: HTTP 400 Bad Request.
        /// On post not found: HTTP 404 Not Found.
        /// On unauthorized access: HTTP 403 Forbidden.
        /// </returns>
        /// <remarks>
        /// This GET action prepares the delete confirmation by:
        /// 1. Validating the post ID parameter
        /// 2. Loading the post with author information for display
        /// 3. Performing security validation to ensure only the author can delete
        /// 4. Displaying the post details for user confirmation
        /// 
        /// This follows the standard HTTP GET/POST pattern for delete operations,
        /// requiring explicit user confirmation before performing the destructive action.
        /// </remarks>
        [HttpGet]
        [Authorize]
        public ActionResult Delete(string id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Load the post with author information for display
            Post post = db.Posts
                .Include(p => p.Author)
                .FirstOrDefault(p => p.PostId == id);

            if (post == null)
            {
                return HttpNotFound();
            }

            // Security check - only allow deletion of own posts
            if (post.AuthorId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(post);
        }

        /// <summary>
        /// Processes the confirmed deletion of a post and all its related data.
        /// Handles cleanup of all visibility-specific relationships before removing the post.
        /// </summary>
        /// <param name="id">The unique identifier of the post to delete.</param>
        /// <returns>
        /// On success: Redirects to ViewPosts action.
        /// On post not found: HTTP 404 Not Found.
        /// On unauthorized access: HTTP 403 Forbidden.
        /// </returns>
        /// <remarks>
        /// This POST action handles the complete post deletion process:
        /// 1. Loads the post with all related entities
        /// 2. Performs security validation to ensure only the author can delete
        /// 3. Clears all visibility-specific relationships (departments, courses, classes)
        /// 4. Removes the post from the database
        /// 5. Saves changes to commit the deletion
        /// 
        /// The method uses the ActionName attribute to map the POST request to "Delete"
        /// while using a different method name to avoid conflicts with the GET Delete action.
        /// 
        /// Relationship cleanup is essential to maintain referential integrity and
        /// prevent foreign key constraint violations during deletion.
        /// </remarks>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(string id)
        {
            // Load the post with all related entities for proper cleanup
            Post post = db.Posts
                .Include(p => p.Departments)
                .Include(p => p.Courses)
                .Include(p => p.Classes)
                .FirstOrDefault(p => p.PostId == id);

            if (post == null)
            {
                return HttpNotFound();
            }

            // Security check - only allow deletion of own posts
            if (post.AuthorId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // Clear all relationships first to avoid foreign key constraint issues
            post.Departments.Clear();
            post.Courses.Clear();
            post.Classes.Clear();

            // Remove the post from the database
            db.Posts.Remove(post);
            db.SaveChanges();

            return RedirectToAction("ViewPosts");
        }

        #endregion

        #region Post Details

        /// <summary>
        /// Displays detailed information about a specific post.
        /// Shows the post content along with author information.
        /// </summary>
        /// <param name="id">The unique identifier of the post to display.</param>
        /// <returns>
        /// On success: A view showing the complete post details.
        /// On invalid ID: HTTP 400 Bad Request.
        /// On post not found: HTTP 404 Not Found.
        /// </returns>
        /// <remarks>
        /// This action provides a detailed view of a single post, including:
        /// - Post title, content, and publication date
        /// - Author information
        /// - Any other relevant post metadata
        /// 
        /// Note: This action does not include visibility filtering, so it assumes
        /// that access control is handled at a higher level (e.g., through routing
        /// or by only providing links to posts that users should be able to see).
        /// </remarks>
        [HttpGet]
        [Authorize]
        public ActionResult Details(string id)
        {
            // Validate that an ID was provided
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Load the post with author information for display
            var post = db.Posts
                .Include(p => p.Author)
                .FirstOrDefault(p => p.PostId == id);

            if (post == null)
            {
                return HttpNotFound();
            }

            return View(post);
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

        #region Helper Classes

        /// <summary>
        /// Helper class providing post visibility filtering functionality.
        /// Determines which posts a user can see based on their role and organizational relationships.
        /// Written by Billy, reused by Karina for filtering read and unread posts across dashboards.
        /// </summary>
        /// <remarks>
        /// This static helper class encapsulates the complex logic for determining post visibility
        /// based on the following factors:
        /// - User roles (Student, Lecturer, GuidanceTeacher, CurriculumHead)
        /// - Organizational relationships (Department, Course, Class associations)
        /// - Post visibility settings (Global, Student, Staff, Department, Course, Class)
        /// 
        /// The class is designed to be reusable across different parts of the application
        /// where post filtering is needed, promoting code reuse and consistency.
        /// </remarks>
        public static class PostVisibilityHelper
        {
            /// <summary>
            /// Determines which posts are visible to a specific user based on their role and relationships.
            /// Applies complex filtering logic to return only posts that the user should be able to see.
            /// </summary>
            /// <param name="userId">The unique identifier of the user requesting posts.</param>
            /// <param name="dbContext">The database context for accessing post and user data.</param>
            /// <param name="user">The current user's principal object containing role information.</param>
            /// <returns>
            /// An IQueryable of Post entities that are visible to the specified user.
            /// The query is not executed until enumerated, allowing for further filtering or ordering.
            /// </returns>
            /// <remarks>
            /// This method implements a comprehensive visibility system with the following rules:
            /// 
            /// Base Visibility Rules:
            /// - Users can always see their own posts
            /// - Global posts are visible to everyone
            /// - Staff posts are visible to staff members (Lecturer, GuidanceTeacher, CurriculumHead)
            /// - Student posts are visible to students
            /// 
            /// Department-Level Visibility:
            /// - Lecturers see department posts for departments where they teach
            /// - Guidance Teachers see department posts for departments of their classes
            /// - Curriculum Heads see posts for their departments
            /// - Students see department posts for their course's department
            /// 
            /// Course-Level Visibility:
            /// - Lecturers see course posts for courses they teach
            /// - Guidance Teachers see course posts for courses their classes are enrolled in
            /// - Students see course posts for courses they're enrolled in
            /// 
            /// Class-Level Visibility:
            /// - Lecturers see class posts for classes they teach
            /// - Guidance Teachers see posts for their assigned classes
            /// - Students see posts for their assigned class
            /// 
            /// The method uses Union operations to combine different visibility criteria,
            /// ensuring that posts matching any applicable rule are included in the results.
            /// </remarks>
            public static IQueryable<Post> GetVisiblePosts(string userId, GuidanceTrackerDbContext dbContext, IPrincipal user)
            {
                // Determine user's role categories for visibility filtering
                var isStaff = user.IsInRole("Lecturer") || user.IsInRole("GuidanceTeacher") || user.IsInRole("CurriculumHead");
                var isStudent = user.IsInRole("Student");

                // Base visibility filter: own posts, global posts, and role-based posts
                var visiblePosts = dbContext.Posts.Where(p =>
                    p.AuthorId == userId ||  // User's own posts
                    p.Visibility == VisibilityType.Global ||  // Posts visible to everyone
                    (isStaff && p.Visibility == VisibilityType.Staff) ||  // Staff-only posts for staff members
                    (isStudent && p.Visibility == VisibilityType.Student)  // Student-only posts for students
                );

                // Handle department-level visibility for staff members
                if (isStaff && dbContext.Posts.Any(p => p.Visibility == VisibilityType.Department))
                {
                    List<string> departmentIds = new List<string>();

                    // Get department IDs based on staff role
                    if (user.IsInRole("Lecturer"))
                    {
                        // Lecturers see department posts for departments where they teach
                        departmentIds = dbContext.Units
                            .Where(u => u.LecturerId == userId)
                            .SelectMany(u => u.Classes)
                            .SelectMany(c => c.Enrollments)
                            .Select(e => e.Course.DepartmentId)
                            .Distinct()
                            .ToList();
                    }
                    else if (user.IsInRole("GuidanceTeacher"))
                    {
                        // Guidance teachers see department posts for departments of their classes
                        departmentIds = dbContext.Classes
                            .Where(c => c.GuidanceTeacherId == userId)
                            .SelectMany(c => c.Enrollments)
                            .Select(e => e.Course.DepartmentId)
                            .Distinct()
                            .ToList();
                    }
                    else if (user.IsInRole("CurriculumHead"))
                    {
                        // Curriculum heads see posts for their departments
                        departmentIds = dbContext.Departments
                            .Where(d => d.CurriculumHead.Id == userId)
                            .Select(d => d.DepartmentId)
                            .ToList();
                    }

                    // Add department-specific posts to visible posts
                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Department &&
                            p.Departments.Any(d => departmentIds.Contains(d.DepartmentId))
                        )
                    );
                }
                else if (isStudent)
                {
                    // Students see department posts for their course's department
                    var departmentIds = dbContext.Students
                        .Where(s => s.Id == userId)
                        .SelectMany(s => s.Class.Enrollments)
                        .Select(e => e.Course.DepartmentId)
                        .Distinct()
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Department &&
                            p.Departments.Any(d => departmentIds.Contains(d.DepartmentId))
                        )
                    );
                }

                // Handle course-level visibility
                if (isStaff)
                {
                    List<int> courseIds = new List<int>();

                    // Get course IDs based on staff role
                    if (user.IsInRole("Lecturer"))
                    {
                        // Lecturers see course posts for courses they teach
                        courseIds = dbContext.Units
                            .Where(u => u.LecturerId == userId)
                            .SelectMany(u => u.Classes)
                            .SelectMany(c => c.Enrollments)
                            .Select(e => e.CourseId)
                            .Distinct()
                            .ToList();
                    }
                    else if (user.IsInRole("GuidanceTeacher"))
                    {
                        // Guidance teachers see course posts for courses their classes are enrolled in
                        courseIds = dbContext.Classes
                            .Where(c => c.GuidanceTeacherId == userId)
                            .SelectMany(c => c.Enrollments)
                            .Select(e => e.CourseId)
                            .Distinct()
                            .ToList();
                    }

                    // Add course-specific posts to visible posts
                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Course &&
                            p.Courses.Any(c => courseIds.Contains(c.CourseId))
                        )
                    );
                }
                else if (isStudent)
                {
                    // Students see course posts for courses they're enrolled in
                    var courseIds = dbContext.Students
                        .Where(s => s.Id == userId)
                        .SelectMany(s => s.Class.Enrollments)
                        .Select(e => e.CourseId)
                        .Distinct()
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Course &&
                            p.Courses.Any(d => courseIds.Contains(d.CourseId))
                        )
                    );
                }

                // Handle class-level visibility 
                if (isStaff)
                {
                    List<int> classIds = new List<int>();

                    // Get class IDs based on staff role
                    if (user.IsInRole("Lecturer"))
                    {
                        // Lecturers see class posts for classes they teach
                        classIds = dbContext.Units
                            .Where(u => u.LecturerId == userId)
                            .SelectMany(u => u.Classes)
                            .Select(c => c.ClassId)
                            .Distinct()
                            .ToList();
                    }
                    else if (user.IsInRole("GuidanceTeacher"))
                    {
                        // Guidance teachers see posts for their assigned classes
                        classIds = dbContext.Classes
                            .Where(c => c.GuidanceTeacherId == userId)
                            .Select(c => c.ClassId)
                            .ToList();
                    }

                    // Add class-specific posts to visible posts
                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Class &&
                            p.Classes.Any(d => classIds.Contains(d.ClassId))
                        )
                    );
                }
                else if (isStudent)
                {
                    // Students see posts for their assigned class
                    var classId = dbContext.Students
                        .Where(s => s.Id == userId)
                        .Select(s => s.ClassId)
                        .FirstOrDefault();

                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Class &&
                            p.Classes.Any(d => d.ClassId == classId)
                        )
                    );
                }

                return visiblePosts;
            }
        }

        #endregion
    }
}
