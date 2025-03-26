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

        // lists to hold issues by type and class
        public List<IssueByType> IssuesByType { get; set; }
        public List<IssueByClass> IssuesByClass { get; set; }

        // dropdown data
        public List<Class> Classes { get; set; }

        // filter properties, can be nullable, as dont have to select anything
        public int? SelectedClassId { get; set; }
    }

    // helper classes for issue type and issue by class
    public class IssueByType
    {
        public string IssueType { get; set; }
        public int Count { get; set; }
    }

    public class IssueByClass
    {
        public string ClassName { get; set; }
        public int IssueCount { get; set; }
    }

    
}
