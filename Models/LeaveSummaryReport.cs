namespace Employee_Management_System.Models
{
    public class LeaveSummaryReport
    {
        public string Department { get; set; }
        public int TotalLeaveRequests { get; set; }
        public int ApprovedLeaves { get; set; }
        public int RejectedLeaves { get; set; }
        public int PendingLeaves { get; set; }
        public string MostCommonLeaveType { get; set; }
        public int TotalLeaveDays { get; set; }
    }
}