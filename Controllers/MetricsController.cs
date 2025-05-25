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

    /// <summary>
    /// index action handles most of the logic for queries and filters for metrics and also the metrics view model an build
    /// </summary>

    [Authorize(Roles = "GuidanceTeacher")]
    public ActionResult Index(int? classId, int? unitId, IssueTitle? issueType, DateTime? startDate, DateTime? endDate, string export = null)
    {
        /// <summary> if no date range is provided, the default is the current month </summary>
        if (!startDate.HasValue || !endDate.HasValue)
        {
            startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            endDate = startDate.Value.AddMonths(1).AddDays(-1);
        }
        /// <summary> base query </summary>
        var query = db.Issues.AsQueryable()
            .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

        /// <summary> check if filter are selected and apply them if necessary </summary>
        if (classId.HasValue)
        {
            query = query.Where(i => i.Student.ClassId == classId);
        }

        /// <summary> if a unit is selected, get the unit name and filter issues that have comments containing that unit name </summary>
        if (unitId.HasValue)
        {
            var unitName = db.Units.Where(u => u.UnitId == unitId).Select(u => u.UnitName).FirstOrDefault();
            if (!string.IsNullOrEmpty(unitName))
            {
                var issueIdsWithUnitComments = db.Comments
                    .Where(c => c.Content.Contains(unitName))
                    .Select(c => c.IssueId)
                    .Distinct();
                query = query.Where(i => issueIdsWithUnitComments.Contains(i.IssueId));
            }
        }

        if (issueType.HasValue)
        {
            query = query.Where(i => i.IssueTitle == issueType.Value);
        }

        /// <summary> get unnits based on class filters </summary>
        var units = classId.HasValue
            ? db.Classes.Where(c => c.ClassId == classId).SelectMany(c => c.Units).ToList()
            : db.Units.ToList();

        /// <summary> these queries prepare data for daily trends and type charts </summary>
        var dailyTrends = query
            .GroupBy(i => DbFunctions.TruncateTime(i.CreatedAt))
            .Select(g => new DailyTrend
            {
                Date = g.Key.Value,
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        var byType = query
            .GroupBy(i => i.IssueTitle)
            .Select(g => new IssueTypeSummary
            {
                IssueType = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(t => t.Count)
            .ToList();

        /// <summary> prepare data for class and unit charts </summary>
        var byClass = query
            .GroupBy(i => i.Student.Class)
            .Select(g => new ClassSummary
            {
                ClassId = g.Key.ClassId,
                ClassName = g.Key.ClassName,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        var byUnit = new List<UnitSummary>();
        /// <summary> if a class is selected get units for that class </summary>
        if (classId.HasValue)
        {
            /// <summary> get units for the selected class and count issues in that class </summary>
            var classUnits = db.Classes
                .Where(c => c.ClassId == classId)
                .SelectMany(c => c.Units)
                .ToList();

            byUnit = classUnits.Select(u => new UnitSummary
            {
                UnitId = u.UnitId,
                UnitName = u.UnitName,
                Count = GetUnitSpecificIssueCount(u.UnitId, u.UnitName, startDate.Value, endDate.Value, classId, issueType)
            })
            .Where(u => u.Count > 0)
            .OrderByDescending(u => u.Count)
            .ToList();
        }
        else
        {
            /// <summary> get all units and their associated issue counts </summary>
            var allUnits = db.Units.ToList();

            byUnit = allUnits.Select(u => new UnitSummary
            {
                UnitId = u.UnitId,
                UnitName = u.UnitName,
                Count = GetUnitSpecificIssueCount(u.UnitId, u.UnitName, startDate.Value, endDate.Value, null, issueType)
            })
            .Where(u => u.Count > 0)
            .OrderByDescending(u => u.Count)
            .ToList();
        }

        /// <summary> comparison data for previous period, set to one month </summary>
        var comparisonStart = startDate.Value.AddMonths(-1);
        var comparisonEnd = endDate.Value.AddMonths(-1);

        var prevQuery = db.Issues
            .Where(i => i.CreatedAt >= comparisonStart && i.CreatedAt <= comparisonEnd);

        if (classId.HasValue)
        {
            prevQuery = prevQuery.Where(i => i.Student.ClassId == classId);
        }

        if (unitId.HasValue)
        {
            var unitName = db.Units.Where(u => u.UnitId == unitId).Select(u => u.UnitName).FirstOrDefault();
            if (!string.IsNullOrEmpty(unitName))
            {
                var issueIdsWithUnitComments = db.Comments
                    .Where(c => c.Content.Contains(unitName))
                    .Select(c => c.IssueId)
                    .Distinct();
                prevQuery = prevQuery.Where(i => issueIdsWithUnitComments.Contains(i.IssueId));
            }
        }

        if (issueType.HasValue)
        {
            prevQuery = prevQuery.Where(i => i.IssueTitle == issueType.Value);
        }

        var prevPeriodCount = prevQuery.Count();

        /// <summary> get enum values for dropdowns </summary>
        var allIssueTypes = Enum.GetValues(typeof(IssueTitle)).Cast<IssueTitle>().ToList();

        /// <summary> create the view model </summary>
        var model = new MetricsViewModel
        {
            StartDate = startDate.Value,
            EndDate = endDate.Value,
            SelectedClassId = classId,
            SelectedUnitId = unitId,
            SelectedIssueType = issueType,
            Classes = db.Classes.ToList(),
            Units = units,
            IssueTypes = allIssueTypes,
            TotalIssues = query.Count(),
            PercentageChange = CalculatePercentageChange(query.Count(), prevPeriodCount),
            DailyTrends = dailyTrends,
            ByType = byType,
            ByClass = byClass,
            ByUnit = byUnit
        };

        /// <summary> if export pdf button is clicked it will call the Rotativa methods to export a pdf file </summary>
        if (export == "pdf")
        {
            return new Rotativa.ViewAsPdf("Export", model)
            {
                FileName = $"GuidanceMetrics_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageMargins = new Rotativa.Options.Margins(10, 10, 10, 10)
            };
        }

        return View(model);
    }

    /// <summary> export pdf action that has a corresponding partial for metrics data export </summary>
    public ActionResult Export(MetricsViewModel model)
    {
        return View(model);
    }

    /// <summary> helper method to calculate percentage changes </summary>
    private decimal CalculatePercentageChange(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return ((current - previous) / (decimal)previous) * 100;
    }

    /// <summary>
    /// count issues where the unit is mentioned in comments
    /// count both original issues ceated plus the issues added in comments
    /// </summary>
    private int GetUnitSpecificIssueCount(int unitId, string unitName, DateTime startDate, DateTime endDate, int? classId, IssueTitle? issueType)
    {
        var baseIssueQuery = db.Issues
            .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

        if (classId.HasValue)
        {
            baseIssueQuery = baseIssueQuery.Where(i => i.Student.ClassId == classId);
        }

        if (issueType.HasValue)
        {
            baseIssueQuery = baseIssueQuery.Where(i => i.IssueTitle == issueType.Value);
        }

        var issuesWithUnitComments = baseIssueQuery
            .Where(i => i.Comments.Any(c =>
                c.Content.Contains(unitName)
            ))
            .Count();

        return issuesWithUnitComments;
    }
    public JsonResult GetUnitsByClass(int classId)
    {
        var units = db.Classes
            .Where(c => c.ClassId == classId)
            .SelectMany(c => c.Units)
            .Select(u => new { u.UnitId, u.UnitName })
            .ToList();
        return Json(units, JsonRequestBehavior.AllowGet);
    }

    public JsonResult FilterData(DateTime startDate, DateTime endDate, int? classId, int? unitId, IssueTitle? issueType)
    {
        var query = db.Issues.AsQueryable()
            .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

        if (classId.HasValue) query = query.Where(i => i.Student.ClassId == classId);
        if (unitId.HasValue)
        {
            var unitName = db.Units.Where(u => u.UnitId == unitId).Select(u => u.UnitName).FirstOrDefault();
            if (!string.IsNullOrEmpty(unitName))
            {
                var issueIdsWithUnitComments = db.Comments
                    .Where(c => c.Content.Contains(unitName))
                    .Select(c => c.IssueId)
                    .Distinct();
                query = query.Where(i => issueIdsWithUnitComments.Contains(i.IssueId));
            }
        }
        if (issueType.HasValue) query = query.Where(i => i.IssueTitle == issueType.Value);

        var byClass = query
            .GroupBy(i => i.Student.Class)
            .Select(g => new
            {
                classId = g.Key.ClassId,
                className = g.Key.ClassName,
                count = g.Count()
            })
            .OrderByDescending(c => c.count)
            .ToList();

        var byUnit = new List<object>();
        if (classId.HasValue)
        {
            var classUnits = db.Classes.Where(c => c.ClassId == classId)
                .SelectMany(c => c.Units)
                .ToList();

            byUnit = classUnits.Select(u => new
            {
                unitId = u.UnitId,
                unitName = u.UnitName,
                count = GetUnitSpecificIssueCount(u.UnitId, u.UnitName, startDate, endDate, classId, issueType)
            })
            .Where(u => u.count > 0)
            .OrderByDescending(u => u.count)
            .Cast<object>()
            .ToList();
        }
        else
        {
            var allUnits = db.Units.ToList();

            byUnit = allUnits.Select(u => new
            {
                unitId = u.UnitId,
                unitName = u.UnitName,
                count = GetUnitSpecificIssueCount(u.UnitId, u.UnitName, startDate, endDate, null, issueType)
            })
            .Where(u => u.count > 0)
            .OrderByDescending(u => u.count)
            .Cast<object>()
            .ToList();
        }

        /// <summary> comparison data for previous period </summary>
        var comparisonStart = startDate.AddMonths(-1);
        var comparisonEnd = endDate.AddMonths(-1);
        var prevQuery = db.Issues
            .Where(i => i.CreatedAt >= comparisonStart && i.CreatedAt <= comparisonEnd);

        if (classId.HasValue)
            prevQuery = prevQuery.Where(i => i.Student.ClassId == classId);

        if (unitId.HasValue)
        {
            var unitName = db.Units.Where(u => u.UnitId == unitId).Select(u => u.UnitName).FirstOrDefault();
            if (!string.IsNullOrEmpty(unitName))
            {
                var issueIdsWithUnitComments = db.Comments
                    .Where(c => c.Content.Contains(unitName))
                    .Select(c => c.IssueId)
                    .Distinct();
                prevQuery = prevQuery.Where(i => issueIdsWithUnitComments.Contains(i.IssueId));
            }
        }

        if (issueType.HasValue)
            prevQuery = prevQuery.Where(i => i.IssueTitle == issueType.Value);

        var prevPeriodCount = prevQuery.Count();

        /// <summary> prepare response  </summary>
        var result = new
        {
            totalIssues = query.Count(),
            percentageChange = CalculatePercentageChange(query.Count(), prevPeriodCount),
            dailyTrends = query
                .GroupBy(i => DbFunctions.TruncateTime(i.CreatedAt))
                .Select(g => new
                {
                    date = g.Key.Value,
                    count = g.Count()
                })
                .OrderBy(d => d.date)
                .ToList()
                .Select(d => new
                {
                    date = d.date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    count = d.count
                })
                .ToList(),

            byType = query
                .GroupBy(i => i.IssueTitle)
                .Select(g => new
                {
                    issueType = g.Key.ToString(),
                    count = g.Count()
                })
                .OrderByDescending(t => t.count)
                .ToList(),

            byClass = byClass,
            byUnit = byUnit
        };

        return Json(result, JsonRequestBehavior.AllowGet);
    }
}
