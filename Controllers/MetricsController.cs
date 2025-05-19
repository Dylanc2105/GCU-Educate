using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using System.Data.Entity;
/// <summary>
/// Author: Karina Fatkullina
/// File: Metrics controller that contains the logic for the metrics page
/// </summary>
public class MetricsController : Controller
{
    
    private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
    [Authorize(Roles = "GuidanceTeacher")]
    public ActionResult Index(int? classId, DateTime? startDate, DateTime? endDate)
    {
        /// <summary>
        /// if no date range is provided the default start date is set to last month
        /// default end date is set to today
        /// </summary>
        if (!startDate.HasValue)
        {
            startDate = DateTime.Today.AddMonths(-1); 
        }

        if (!endDate.HasValue)
        {
            endDate = DateTime.Today; 
        }
        
        var issuesQuery = db.Issues.AsQueryable();

        
        /// <summary> if class is selected, filter issues for students in that class </summary>
        if (classId.HasValue)
        {
            issuesQuery = issuesQuery.Where(i => i.Student.ClassId == classId.Value);
        }
       
        issuesQuery = issuesQuery.Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

        /// <summary> group issues by date, using DbFunctions.TruncateTime to ignore time part </summary>
        var issuesOverTime = issuesQuery
            .GroupBy(i => DbFunctions.TruncateTime(i.CreatedAt))
            .Select(g => new IssuesOverTime
            {
                Date = g.Key.Value,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();
        
        /// <summary> group issues by type </summary>
        var issuesByType = issuesQuery
            .GroupBy(i => i.IssueTitle.ToString())
            .Select(g => new IssueByType
            {
                IssueType = g.Key,
                Count = g.Count()
            }).ToList();

        /// <summary> populate the view model with the data </summary>
        var model = new MetricsViewModel
        {
            
            TotalIssues = issuesQuery.Count(),
            IssuesByType = issuesByType, 
            Classes = db.Classes.ToList(),
            SelectedClassId = classId,
            StartDate = startDate,
            EndDate = endDate,
            IssuesOverTime = issuesOverTime
        };

        return View(model);
    }
}