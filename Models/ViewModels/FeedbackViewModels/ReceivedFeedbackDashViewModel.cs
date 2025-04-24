using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels.FeedbackViewModels
{
    public class ReceivedFeedbackDashViewModel
    {
        public List<DetailedFeedback> DetailedFeedbacks { get; set; }
        public List<SimpleFeedback> SimpleFeedbacks { get; set; }

        public ReceivedFeedbackDashViewModel()
        {
            DetailedFeedbacks = new List<DetailedFeedback>();
            SimpleFeedbacks = new List<SimpleFeedback>();
        }
    }
}