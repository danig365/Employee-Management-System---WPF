using System;

namespace Employee_Management_System.Models
{
    public class LeaveRequest
    {
        public int LeaveId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public DateTime RequestDate { get; set; }
        public int? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public string AdminRemarks { get; set; }
        public Employee Employee { get; set; }
    }
}