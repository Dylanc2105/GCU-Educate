using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Models.ViewModels
{
    public class MetricsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? SelectedClassId { get; set; }
        public int? SelectedUnitId { get; set; }
        public IssueTitle? SelectedIssueType { get; set; }
        public List<Class> Classes { get; set; }
        public List<Unit> Units { get; set; }
        public List<IssueTitle> IssueTypes { get; set; }
        public int TotalIssues { get; set; }
        public decimal PercentageChange { get; set; }
        public List<DailyTrend> DailyTrends { get; set; }
        public List<IssueTypeSummary> ByType { get; set; }
        public List<ClassSummary> ByClass { get; set; }
        public List<UnitSummary> ByUnit { get; set; }

        public List<IssueStatusSummary> ByStatus { get; set; }
        public int ActiveIssues { get; set; }
        public int ArchivedIssues { get; set; }
        public decimal ResolutionRate { get; set; }
        public decimal ResolutionRateChange { get; set; }
        public double AverageResolutionDays { get; set; }
        public List<StatusTrend> StatusTrends { get; set; }
    }

    /// <summary> helper classes for trends and issue, unit, and class summaries. also status</summary>
    /// 
    public class IssueStatusSummary
    {
        public IssueStatus Status { get; set; }
        public int Count { get; set; }
        public string DisplayName => Status.ToString().Replace("InProgress", "In Progress");
    }
    public class StatusTrend
    {
        public DateTime Date { get; set; }
        public IssueStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class DailyTrend
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string DayOfWeek => Date.ToString("ddd");
        public string FormattedDate => Date.ToString("MMM dd");
    }

    public class IssueTypeSummary
    {
        public IssueTitle IssueType { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string DisplayName => IssueType.ToString();
    }

    public class ClassSummary
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int Count { get; set; }
    }

    public class UnitSummary
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int Count { get; set; }
    }
}