namespace Employee_Management_System.Models
{
    public class MonthlyAttendanceReport
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public int TotalPresentDays { get; set; }
        public int TotalAbsentDays { get; set; }
        public int TotalHalfDays { get; set; }
        public int LateCheckIns { get; set; }
        public decimal AverageWorkingHours { get; set; }
        public int TotalRecords { get; set; }
    }
}