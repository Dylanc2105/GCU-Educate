using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using static GuidanceTracker.Models.ViewModels.MetricsViewModel;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;


public class MetricsController : Controller
{
    //new instance of database context
    private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

    // GET: Metrics
    public ActionResult Index(int? courseId, int? classId, int? lecturerId, int? timePeriod)
    {
        var issuesQuery = db.Issues.AsQueryable(); 

        // Apply filters if selected
        if (courseId.HasValue) { issuesQuery = issuesQuery.Where(i => i.Student.CourseId == courseId); }
        if (classId.HasValue) { issuesQuery = issuesQuery.Where(i => i.Student.ClassId == classId); }
        if (lecturerId.HasValue) { issuesQuery = issuesQuery.Where(i => i.LecturerId == lecturerId); }
        if (timePeriod.HasValue)
        {
            var startDate = DateTime.Now.AddMonths(-timePeriod.Value);
            issuesQuery = issuesQuery.Where(i => i.CreatedAt >= startDate);
        }

        var model = new MetricsViewModel
        {
            TotalIssues = issuesQuery.Count(),
            NewIssues = issuesQuery.Count(i => i.IsResolved),
            InProgressIssues = issuesQuery.Count(i => !i.IsResolved),
            ArchivedIssues = issuesQuery.Count(i => i.IsArchived),
            IssuesByType = issuesQuery.GroupBy(i => i.Type)
                .Select(g => new IssueSummary { Type = g.Key.ToString(), Count = g.Count() }).ToList(),

            StudentsWithMostIssues = issuesQuery.GroupBy(i => i.StudentId).OrderByDescending(g => g.Count()).Take(5)
                .Select(g => new StudentIssueSummary
                {
                    StudentId = g.Key,
                    StudentName = db.Students.Where(s => s.Id == g.Key).Select(s => s.Name).FirstOrDefault(),
                    IssueCount = g.Count()
                }).ToList(),

            TotalAppointments = db.Appointments.Count(),
            MissedAppointments = db.Appointments.Count(a => !a.Attended),

            IssuesRaisedByLecturers = db.Issues.GroupBy(i => i.LecturerId)
                .Select(g => new LecturerIssueSummary
                {
                    LecturerName = db.Lecturers.Where(l => l.Id == g.Key).Select(l => l.Name).FirstOrDefault(),
                    IssueCount = g.Count()
                }).ToList(),

            IssuesOverTime = db.Issues.GroupBy(i => i.CreatedAt.Month)
                .Select(g => new IssueTrends { TimePeriod = "Month " + g.Key, IssueCount = g.Count() }).ToList()
        };

        return View(model);

    }
}