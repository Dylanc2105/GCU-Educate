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

    public class PostController : Controller
    {
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
        // GET: Post

        public ActionResult ViewMyPosts()
        {
            var currentUserId = User.Identity.GetUserId();

            var visiblePosts = db.Posts.Where(p => p.AuthorId == currentUserId).ToList();

            var posts = visiblePosts
            .OrderByDescending(p => p.PostDate)
            .ToList();
            return View(posts);
        }
        public ActionResult ViewPosts()
        {
            var currentUserId = User.Identity.GetUserId();

            // karina: moved the visibility filter to a helper method after the postcontroller class in order to reuse it in other dashboards.
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(currentUserId, db, User)
                .Include(p => p.Author)
                .OrderByDescending(p => p.PostDate)
                .ToList();

            var readPostIds = db.PostReads
                .Where(pr => pr.UserId == currentUserId)
                .Select(pr => pr.PostId)
                .ToList();

            var unreadPosts = visiblePosts
                .Where(p => !readPostIds.Contains(p.PostId))
                .ToList();

            foreach (var unread in unreadPosts)
            {
                db.PostReads.Add(new PostRead
                {
                    PostId = unread.PostId,
                    UserId = currentUserId,
                    ReadOn = DateTime.Now
                });
            }

            db.SaveChanges();
            ViewBag.UnreadCount = unreadPosts.Count;
            ViewBag.ReadPostIds = readPostIds;

            return View(visiblePosts);
        }


        // GET: Posts/Create
        [HttpGet]
        [Authorize]
        public ActionResult CreateNewPost()
        {
            var viewModel = new CreatePostViewModel();

            // Set up the visibility dropdown
            viewModel.VisibilityOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = VisibilityType.Global.ToString(), Text = "Global (Everyone)" },
                new SelectListItem { Value = VisibilityType.Student.ToString(), Text = "Students Only" },
                new SelectListItem { Value = VisibilityType.Staff.ToString(), Text = "Lecturers Only" },
                new SelectListItem { Value = VisibilityType.Department.ToString(), Text = "Department" },
                new SelectListItem { Value = VisibilityType.Course.ToString(), Text = "Course" },
                new SelectListItem { Value = VisibilityType.Class.ToString(), Text = "Class" },

            };

            // Load dropdown data for departments, courses, classes, and units
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateNewPost(CreatePostViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var post = new Post
                {
                    PostId = Guid.NewGuid().ToString(),
                    Title = viewModel.Title,
                    Content = viewModel.Content,
                    AuthorId = User.Identity.GetUserId(),
                    PostDate = DateTime.Now,
                    Visibility = viewModel.Visibility
                };

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

                db.Posts.Add(post);
                // 
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
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        });
                    }

                    db.SaveChanges();
                    return RedirectToAction("ViewPosts");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                }
            }

            // If validation fails, reload dropdown data
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

        [HttpGet]
        [Authorize]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
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

            var viewModel = new CreatePostViewModel
            {

                Title = post.Title,
                Content = post.Content,
                Visibility = post.Visibility
            };

            ViewBag.PostId = post.PostId;

            // Set selected values based on the post's current visibility
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

            // Load dropdown data
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

        // POST: Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(string id, CreatePostViewModel viewModel)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

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

                // Clear all existing relationships
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
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                }
            }

            // If validation fails, reload dropdown data
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

        // GET: Post/Delete/5
        [HttpGet]
        [Authorize]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

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

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(string id)
        {
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

            // Clear relationships first
            post.Departments.Clear();
            post.Courses.Clear();
            post.Classes.Clear();

            // Remove the post
            db.Posts.Remove(post);
            db.SaveChanges();

            return RedirectToAction("ViewPosts");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        
    



        // karina:
        // a seperate method for visibility filters written by Billy.
        // i needed to reuse the filter for other dashboards in order to filter read an unread posts.
        public static class PostVisibilityHelper
        {
            public static IQueryable<Post> GetVisiblePosts(string userId, GuidanceTrackerDbContext dbContext, IPrincipal user)
            {

                var isStaff = user.IsInRole("Lecturer") || user.IsInRole("GuidanceTeacher") || user.IsInRole("CurriculumHead");
                var isStudent = user.IsInRole("Student");

                var visiblePosts = dbContext.Posts.Where(p =>
                    p.AuthorId == userId ||
                    p.Visibility == VisibilityType.Global ||
                    (isStaff && p.Visibility == VisibilityType.Staff) ||
                    (isStudent && p.Visibility == VisibilityType.Student)
                );

                if (isStaff && dbContext.Posts.Any(p => p.Visibility == VisibilityType.Department))
                {
                    List<string> departmentIds = new List<string>();

                    if (user.IsInRole("Lecturer"))
                    {
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
                        departmentIds = dbContext.Classes
                            .Where(c => c.GuidanceTeacherId == userId)
                            .SelectMany(c => c.Enrollments)
                            .Select(e => e.Course.DepartmentId)
                            .Distinct()
                            .ToList();
                    }
                    else if (user.IsInRole("CurriculumHead"))
                    {
                        departmentIds = dbContext.Departments
                            .Where(d => d.CurriculumHead.Id == userId)
                            .Select(d => d.DepartmentId)
                            .ToList();
                    }

                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Department &&
                            p.Departments.Any(d => departmentIds.Contains(d.DepartmentId))
                        )
                    );
                }
                else if (isStudent)
                {
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

                // Course-level
                if (isStaff)
                {
                    List<int> courseIds = new List<int>();

                    if (user.IsInRole("Lecturer"))
                    {
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
                        courseIds = dbContext.Classes
                            .Where(c => c.GuidanceTeacherId == userId)
                            .SelectMany(c => c.Enrollments)
                            .Select(e => e.CourseId)
                            .Distinct()
                            .ToList();
                    }

                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Course &&
                            p.Courses.Any(c => courseIds.Contains(c.CourseId))
                        )
                    );
                }
                else if (isStudent)
                {
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

                // Class-level
                if (isStaff)
                {
                    List<int> classIds = new List<int>();

                    if (user.IsInRole("Lecturer"))
                    {
                        classIds = dbContext.Units
                            .Where(u => u.LecturerId == userId)
                            .SelectMany(u => u.Classes)
                            .Select(c => c.ClassId)
                            .Distinct()
                            .ToList();
                    }
                    else if (user.IsInRole("GuidanceTeacher"))
                    {
                        classIds = dbContext.Classes
                            .Where(c => c.GuidanceTeacherId == userId)
                            .Select(c => c.ClassId)
                            .ToList();
                    }

                    visiblePosts = visiblePosts.Union(
                        dbContext.Posts.Where(p =>
                            p.Visibility == VisibilityType.Class &&
                            p.Classes.Any(d => classIds.Contains(d.ClassId))
                        )
                    );
                }
                else if (isStudent)
                {
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
    }
}
    
