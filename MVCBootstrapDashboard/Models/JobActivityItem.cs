using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCBootstrapDashboard.Models
{
    public enum RanOutcome { Error_Failed = 0, Succeeded = 1, Retry = 2, Cancelled = 3, In_Progress = 4, Status_Unknown }

    public class JobActivityItem
    {
        public string name { get; set; }
        public bool enabled { get; set; }
        public bool isExecuting { get; set; }
        public RanOutcome last_ran_outcome { get; set; }
        public DateTime? last_ran_datetime { get; set; }
        public DateTime? next_ran_datetime { get; set; }
    }
}