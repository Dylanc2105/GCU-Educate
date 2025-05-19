using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
/// <summary>
/// Author: Karina Fatkullina
/// File: Metrics View Model to hold data for the metrics page
/// </summary>
namespace GuidanceTracker.Models.ViewModels
{
    public class MetricsViewModel
    {
        // int for the total issue metrics
        public int TotalIssues { get; set; }

        // lists to hold issues by type
        public List<IssueByType> IssuesByType { get; set; }

        // dropdown data
        public List<Class> Classes { get; set; }

        // filter properties, can be nullable, as dont have to select anything
        public int? SelectedClassId { get; set; }
        // properties for issues over time
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<IssuesOverTime> IssuesOverTime { get; set; }
        public List<IssuesOverTime> LateAttendanceOverTime { get; set; }
        public List<IssuesOverTime> MissingAttendanceOverTime { get; set; }

        public List<Unit> Units { get; set; }
        public int? SelectedUnitId { get; set; }

    }

    /// <summary>
    /// helper class for issues by type
    /// </summary>
    public class IssueByType
    {
        public string IssueType { get; set; }
        public int Count { get; set; }
    }
    /// <summary>
    /// helper class for issues over time
    /// </summary>
    
    public class IssuesOverTime
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    



}
