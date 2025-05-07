using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        
    }

    // helper class for issue type
    public class IssueByType
    {
        public string IssueType { get; set; }
        public int Count { get; set; }
    }

    // helper class for issues over time
    public class IssuesOverTime
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    



}
