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

        if (unitId.HasValue)
        {
            query = query.Where(i => i.Student.Class.Units.Any(u => u.UnitId == unitId));
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
            byUnit = query
                .Where(i => i.Student.ClassId == classId)
                .SelectMany(i => i.Student.Class.Units)
                .GroupBy(u => new { u.UnitId, u.UnitName })
                .Select(g => new UnitSummary
                {
                    UnitId = g.Key.UnitId,
                    UnitName = g.Key.UnitName,
                    Count = query.Count(i => i.Student.ClassId == classId)
                })
                .OrderByDescending(u => u.Count)
                .ToList();
        }
        else
        {
            /// <summary> get all units with issue counts </summary>
            var classUnits = db.Classes
                .SelectMany(c => c.Units.Select(u => new { ClassId = c.ClassId, Unit = u }))
                .ToList();

            byUnit = classUnits
                .GroupBy(cu => new { cu.Unit.UnitId, cu.Unit.UnitName })
                .Select(g => new UnitSummary
                {
                    UnitId = g.Key.UnitId,
                    UnitName = g.Key.UnitName,
                    Count = g.Sum(cu => query.Count(i => i.Student.ClassId == cu.ClassId))
                })
                .Where(u => u.Count > 0)
                .OrderByDescending(u => u.Count)
                .ToList();
        }

        /// <summary> comparison data for previous period, set to one month </summary>
        var comparisonStart = startDate.Value.AddMonths(-1);
        var comparisonEnd = endDate.Value.AddMonths(-1);
        var prevPeriodCount = db.Issues
            .Count(i => i.CreatedAt >= comparisonStart && i.CreatedAt <= comparisonEnd);

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
        // Same filtering logic as Index action
        var query = db.Issues.AsQueryable()
            .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

        if (classId.HasValue) query = query.Where(i => i.Student.ClassId == classId);
        if (unitId.HasValue) query = query.Where(i => i.Student.Class.Units.Any(u => u.UnitId == unitId));
        if (issueType.HasValue) query = query.Where(i => i.IssueTitle == issueType.Value);

        // Class analytics
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

        // Fixed unit analytics for FilterData
        var byUnit = new List<object>();
        if (classId.HasValue)
        {
            // Get units for the specific class
            var classIssues = query.Where(i => i.Student.ClassId == classId).ToList();
            var classUnits = db.Classes.Where(c => c.ClassId == classId)
                .SelectMany(c => c.Units)
                .ToList();

            byUnit = classUnits.Select(u => new
            {
                unitId = u.UnitId,
                unitName = u.UnitName,
                count = classIssues.Count // All issues in the class affect all units in that class
            })
            .Where(u => u.count > 0)
            .OrderByDescending(u => u.count)
            .Cast<object>()
            .ToList();
        }
        else
        {
            // Get all classes and their units
            var allClassesWithIssues = query.GroupBy(i => i.Student.Class).ToList();
            var unitCounts = new Dictionary<int, int>();

            foreach (var classGroup in allClassesWithIssues)
            {
                var issueCount = classGroup.Count();
                foreach (var unit in classGroup.Key.Units)
                {
                    if (unitCounts.ContainsKey(unit.UnitId))
                        unitCounts[unit.UnitId] += issueCount;
                    else
                        unitCounts[unit.UnitId] = issueCount;
                }
            }

            var allUnits = db.Units.ToList();
            byUnit = allUnits
                .Where(u => unitCounts.ContainsKey(u.UnitId))
                .Select(u => new
                {
                    unitId = u.UnitId,
                    unitName = u.UnitName,
                    count = unitCounts[u.UnitId]
                })
                .OrderByDescending(u => u.count)
                .Cast<object>()
                .ToList();
        }

        // Comparison data
        var comparisonStart = startDate.AddMonths(-1);
        var comparisonEnd = endDate.AddMonths(-1);
        var prevPeriodCount = db.Issues
            .Count(i => i.CreatedAt >= comparisonStart && i.CreatedAt <= comparisonEnd);

        // Prepare response
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

//public class ExportViewModel
//{
//    public MetricsViewModel Metrics { get; set; }
//    public string GeneratedDate { get; } = DateTime.Now.ToString("f");
//}