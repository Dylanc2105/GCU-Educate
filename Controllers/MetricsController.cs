using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using System.Data.Entity;
using System.Web.UI.WebControls;
using Unit = GuidanceTracker.Models.Unit;
/// <summary>
/// 
/// </summary>
public class MetricsController : Controller
{
    
    private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
    [Authorize(Roles = "GuidanceTeacher")]
    public ActionResult Index(int? classId, int? unitId, DateTime? startDate, DateTime? endDate)
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
        List<Unit> filteredUnits = new List<Unit>();

        /// <summary> if class is selected, filter issues for students in that class </summary>
        if (classId.HasValue)
        {
            issuesQuery = issuesQuery.Where(i => i.Student.ClassId == classId.Value);
            filteredUnits = db.Classes
            .Where(c => c.ClassId == classId.Value)
            .SelectMany(c => c.Units)
            .ToList();

        }
        else
        {
            filteredUnits = db.Units.ToList();
        }


        /// <summary> if unit is selected, filter issues for students in that unit </summary>
        if (unitId.HasValue)
        {
            issuesQuery = issuesQuery.Where(i => i.Student.Class.Units.Any(u => u.UnitId == unitId.Value));
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

        var lateAttendanceOverTime = issuesQuery
            .Where(i => i.IssueTitle.ToString() == "LateAttendance")
            .GroupBy(i => DbFunctions.TruncateTime(i.CreatedAt))
            .Select(g => new IssuesOverTime
            {
                Date = g.Key.Value,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        var missingAttendanceOverTime = issuesQuery
            .Where(i => i.IssueTitle.ToString() == "MissingAttendance")
            .GroupBy(i => DbFunctions.TruncateTime(i.CreatedAt))
            .Select(g => new IssuesOverTime
            {
                Date = g.Key.Value,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();
        /// <summary> populate the view model with the data </summary>
        var model = new MetricsViewModel
        {
            
            TotalIssues = issuesQuery.Count(),
            IssuesByType = issuesByType, 
            Classes = db.Classes.ToList(),
            Units = filteredUnits,
            SelectedClassId = classId,
            SelectedUnitId = unitId,
            StartDate = startDate,
            EndDate = endDate,
            IssuesOverTime = issuesOverTime,
            LateAttendanceOverTime = lateAttendanceOverTime,
            MissingAttendanceOverTime = missingAttendanceOverTime
        };

        return View(model);
    }
}