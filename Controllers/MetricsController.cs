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

    public ActionResult Index(int? classId)
    {
        // variable to hold issues
        var issuesQuery = db.Issues.AsQueryable();

        // if class is selected, filter issues for students in that class
        if (classId.HasValue)
        {
            issuesQuery = issuesQuery.Where(i => i.Student.ClassId == classId.Value);
        }

        

        var model = new MetricsViewModel
        {
            // total number of issues
            TotalIssues = issuesQuery.Count(),

            // issues by type (filtered)
            IssuesByType = issuesQuery
                .GroupBy(i => i.IssueTitle.ToString())
                .Select(g => new IssueByType
                {
                    IssueType = g.Key,
                    Count = g.Count()
                }).ToList(),

            

            // populated the class dropdown menu
            Classes = db.Classes.ToList(),
            // if provided set the selected class id
            SelectedClassId = classId
        };

        return View(model);
    }
}