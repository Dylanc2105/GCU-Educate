using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels
{
	public class MetricsViewModel
	{
        // issue tracking
        public int TotalIssues { get; set; }
        public int NewIssues { get; set; }
        public int InProgressIssues { get; set; }
        public int ArchivedIssues { get; set; }
        public List<IssueSummary> IssuesByType { get; set; }

        // student performance
        public List<StudentIssueSummary> StudentsWithMostIssues { get; set; }
        public List<StudentIssueSummary> StudentsWithRepeatedIssues { get; set; }
        public List<StudentPerformanceChange> PerformanceChanges { get; set; }

        // course/class performance
        public List<CourseIssueSummary> IssuesByCourse { get; set; }
        public List<ClassIssueSummary> IssuesByClass { get; set; }
        public List<SubjectIssueSummary> IssuesByUnit { get; set; }

        // lecturers and appointments
        public int TotalAppointments { get; set; }
        public int MissedAppointments { get; set; }
        public List<LecturerIssueSummary> IssuesRaisedByLecturers { get; set; }

        // trends
        public List<IssueTrends> IssuesOverTime { get; set; }


        //helper classes for summaries
        public class IssueSummary { public string IssueTitle { get; set; } public int Count { get; set; } }
        public class StudentIssueSummary { public int StudentId { get; set; } public string FirstName { get; set; } public string LastNameName { get; set; }  public int IssueCount { get; set; } }
        public class StudentPerformanceChange { public int StudentId { get; set; } public string FirstName { get; set; } public string LastName { get; set; }  public string Change { get; set; } }
        public class CourseIssueSummary { public string CourseName { get; set; } public int IssueCount { get; set; } }
        public class ClassIssueSummary { public string ClassName { get; set; } public int IssueCount { get; set; } }
        public class SubjectIssueSummary { public string UnitName { get; set; } public int IssueCount { get; set; } }
        public class LecturerIssueSummary { public string LecturerId { get; set; } public int IssueCount { get; set; } }
        public class IssueTrends { public string TimePeriod { get; set; } public int IssueCount { get; set; } }
    }

}
