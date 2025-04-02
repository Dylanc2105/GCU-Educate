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
            var isStaff = User.IsInRole("Lecturer") || User.IsInRole("GuidanceTeacher") || User.IsInRole("CurriculumHead");
            var isStudent = User.IsInRole("Student");

            // Basic visibility: posts by the current user, global posts, and role-based posts
            var visiblePosts = db.Posts
                .Where(p =>
                    p.AuthorId == currentUserId ||
                    p.Visibility == VisibilityType.Global ||
                    (isStaff && p.Visibility == VisibilityType.Staff) ||
                    (isStudent && p.Visibility == VisibilityType.Student)
                );

            // For Department visibility
            if (isStaff && db.Posts.Any(p => p.Visibility == VisibilityType.Department))
            {
                if (User.IsInRole("Lecturer"))
                {
                    // For lecturers: Get departments of courses that contain classes where the lecturer teaches units
                    var departmentIds = db.Units
                        .Where(u => u.LecturerId == currentUserId)
                        .SelectMany(u => u.Classes)
                        .SelectMany(c => c.Enrollments)
                        .Select(e => e.Course.DepartmentId)
                        .Distinct()
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        db.Posts.Where(p =>
                            p.Visibility == VisibilityType.Department &&
                            // Check if there's a matching entry in a PostDepartment junction table
                            // If you don't have this table, you'd need to add it or use a different approach
                            p.Departments.Any(d => departmentIds.Contains(d.DepartmentId))
                        )
                    );
                }
                else if (User.IsInRole("GuidanceTeacher"))
                {
                    // For guidance teachers: Get departments of courses that their assigned classes are enrolled in
                    var departmentIds = db.Classes
                        .Where(c => c.GuidanceTeacherId == currentUserId)
                        .SelectMany(c => c.Enrollments)
                        .Select(e => e.Course.DepartmentId)
                        .Distinct()
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        db.Posts.Where(p =>
                            p.Visibility == VisibilityType.Department &&
                            p.Departments.Any(d => departmentIds.Contains(d.DepartmentId))
                        )
                    );
                }
                else if (User.IsInRole("CurriculumHead"))
                {
                    // For curriculum heads: Get departments they're responsible for
                    var departmentIds = db.Departments
                        .Where(d => d.CurriculumHead.Id == currentUserId)
                        .Select(d => d.DepartmentId)
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        db.Posts.Where(p =>
                            p.Visibility == VisibilityType.Department &&
                            p.Departments.Any(d => departmentIds.Contains(d.DepartmentId))
                        )
                    );
                }
            }
            else if (isStudent && db.Posts.Any(p => p.Visibility == VisibilityType.Department))
            {
                // For students: Get departments of courses they're enrolled in
                var departmentIds = db.Students
                    .Where(s => s.Id == currentUserId)
                    .SelectMany(s => s.Class.Enrollments)
                    .Select(e => e.Course.DepartmentId)
                    .Distinct()
                    .ToList();

                visiblePosts = visiblePosts.Union(
                    db.Posts.Where(p =>
                        p.Visibility == VisibilityType.Department &&
                        p.Departments.Any(d => departmentIds.Contains(d.DepartmentId))
                    )
                );
            }

            // For Course visibility
            if (isStaff && db.Posts.Any(p => p.Visibility == VisibilityType.Course))
            {
                if (User.IsInRole("Lecturer"))
                {
                    // For lecturers: Get courses that have classes where the lecturer teaches units
                    var courseIds = db.Units
                        .Where(u => u.LecturerId == currentUserId)
                        .SelectMany(u => u.Classes)
                        .SelectMany(c => c.Enrollments)
                        .Select(e => e.CourseId)
                        .Distinct()
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        db.Posts.Where(p =>
                            p.Visibility == VisibilityType.Course &&
                            p.Courses.Any(c => courseIds.Contains(c.CourseId))
                        )
                    );
                }
                else if (User.IsInRole("GuidanceTeacher"))
                {
                    // For guidance teachers: Get courses that their assigned classes are enrolled in
                    var courseIds = db.Classes
                        .Where(c => c.GuidanceTeacherId == currentUserId)
                        .SelectMany(c => c.Enrollments)
                        .Select(e => e.CourseId)
                        .Distinct()
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        db.Posts.Where(p =>
                            p.Visibility == VisibilityType.Course &&
                            p.Courses.Any(d => courseIds.Contains(d.CourseId))
                        )
                    );
                }
            }
            else if (isStudent && db.Posts.Any(p => p.Visibility == VisibilityType.Course))
            {
                // For students: Get courses they're enrolled in
                var courseIds = db.Students
                    .Where(s => s.Id == currentUserId)
                    .SelectMany(s => s.Class.Enrollments)
                    .Select(e => e.CourseId)
                    .Distinct()
                    .ToList();

                visiblePosts = visiblePosts.Union(
                    db.Posts.Where(p =>
                        p.Visibility == VisibilityType.Course &&
                        p.Courses.Any(d => courseIds.Contains(d.CourseId))
                    )
                );
            }

            // For Class visibility
            if (isStaff && db.Posts.Any(p => p.Visibility == VisibilityType.Class))
            {
                if (User.IsInRole("Lecturer"))
                {
                    // For lecturers: Get classes where they teach units
                    var classIds = db.Units
                        .Where(u => u.LecturerId == currentUserId)
                        .SelectMany(u => u.Classes)
                        .Select(c => c.ClassId)
                        .Distinct()
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        db.Posts.Where(p =>
                            p.Visibility == VisibilityType.Class &&
                            p.Classes.Any(d => classIds.Contains(d.ClassId))
                        )
                    );
                }
                else if (User.IsInRole("GuidanceTeacher"))
                {
                    // For guidance teachers: Get classes they're responsible for
                    var classIds = db.Classes
                        .Where(c => c.GuidanceTeacherId == currentUserId)
                        .Select(c => c.ClassId)
                        .ToList();

                    visiblePosts = visiblePosts.Union(
                        db.Posts.Where(p =>
                            p.Visibility == VisibilityType.Class &&
                            p.Classes.Any(d => classIds.Contains(d.ClassId))
                        )
                    );
                }
            }
            else if (isStudent && db.Posts.Any(p => p.Visibility == VisibilityType.Class))
            {
                // For students: Get their class
                var classId = db.Students
                    .Where(s => s.Id == currentUserId)
                    .Select(s => s.ClassId)
                    .FirstOrDefault();

                visiblePosts = visiblePosts.Union(
                    db.Posts.Where(p =>
                        p.Visibility == VisibilityType.Class &&
                        p.Classes.Any(d => d.ClassId == classId)
                    )
                );
            }

            // Include the author and order by date
            var posts = visiblePosts
                .Include(p => p.Author)
                .OrderByDescending(p => p.PostDate)
                .ToList();

            return View(posts);
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
    }
}
    
