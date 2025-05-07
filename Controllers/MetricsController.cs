using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using System.Data.Entity;

public class MetricsController : Controller
{
    
    private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
    [Authorize(Roles = "GuidanceTeacher")]
    public ActionResult Index(int? classId, DateTime? startDate, DateTime? endDate)
    {
        // if no date range is provided the default start date is set to last month
        if (!startDate.HasValue)
        {
            startDate = DateTime.Today.AddMonths(-1); 
        }

        if (!endDate.HasValue)
        {
            endDate = DateTime.Today; // default end date is today
        }
        // variable to hold issues
        var issuesQuery = db.Issues.AsQueryable();

        // if class is selected, filter issues for students in that class
        if (classId.HasValue)
        {
            issuesQuery = issuesQuery.Where(i => i.Student.ClassId == classId.Value);
        }
        // add date range filter
        issuesQuery = issuesQuery.Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

        // group issues by date
        var issuesOverTime = issuesQuery
            .GroupBy(i => DbFunctions.TruncateTime(i.CreatedAt))
            .Select(g => new IssuesOverTime
            {
                Date = g.Key.Value,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();
        // group issues by type
        var issuesByType = issuesQuery
                .GroupBy(i => i.IssueTitle.ToString())
                .Select(g => new IssueByType
                {
                    IssueType = g.Key,
                    Count = g.Count()
                }).ToList();

        var model = new MetricsViewModel
        {
            // total number of issues
            TotalIssues = issuesQuery.Count(),
            // issues by type (filtered)
            IssuesByType = issuesByType,            
            // populated the class dropdown menu
            Classes = db.Classes.ToList(),
            // if provided set the selected class id
            SelectedClassId = classId,
            StartDate = startDate,
            EndDate = endDate,
            IssuesOverTime = issuesOverTime
        };

        return View(model);
    }
}