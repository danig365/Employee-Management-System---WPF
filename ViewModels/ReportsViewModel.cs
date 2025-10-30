using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Employee_Management_System.Commands;
using Employee_Management_System.Models;
using Employee_Management_System.Services;
using System.IO;
using System.Text;

namespace Employee_Management_System.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly ReportService _reportService;
        private readonly LeaveService _leaveService;

        private int _selectedMonth;
        private int _selectedYear;
        private int? _selectedEmployeeId;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _selectedDepartment;

        private ObservableCollection<MonthlyAttendanceReport> _attendanceReports;
        private ObservableCollection<LeaveSummaryReport> _leaveSummaryReports;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<string> _departments;

        public ReportsViewModel()
        {
            _reportService = new ReportService();
            _leaveService = new LeaveService();

            _selectedMonth = DateTime.Now.Month;
            _selectedYear = DateTime.Now.Year;

            LoadFilters();

            GenerateAttendanceReportCommand = new RelayCommand(ExecuteGenerateAttendanceReport);
            GenerateLeaveSummaryCommand = new RelayCommand(ExecuteGenerateLeaveSummary);
            ExportAttendanceToCsvCommand = new RelayCommand(ExecuteExportAttendanceToCsv, CanExecuteExportAttendance);
            ExportLeaveSummaryToCsvCommand = new RelayCommand(ExecuteExportLeaveSummaryToCsv, CanExecuteExportLeaveSummary);
        }

        #region Properties

        public ObservableCollection<MonthlyAttendanceReport> AttendanceReports
        {
            get => _attendanceReports;
            set
            {
                _attendanceReports = value;
                OnPropertyChanged();
                ((RelayCommand)ExportAttendanceToCsvCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<LeaveSummaryReport> LeaveSummaryReports
        {
            get => _leaveSummaryReports;
            set
            {
                _leaveSummaryReports = value;
                OnPropertyChanged();
                ((RelayCommand)ExportLeaveSummaryToCsvCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Departments
        {
            get => _departments;
            set
            {
                _departments = value;
                OnPropertyChanged();
            }
        }

        public List<int> Months => Enumerable.Range(1, 12).ToList();
        public List<int> Years => Enumerable.Range(2020, DateTime.Now.Year - 2019).ToList();

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged();
            }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged();
            }
        }

        public int? SelectedEmployeeId
        {
            get => _selectedEmployeeId;
            set
            {
                _selectedEmployeeId = value;
                OnPropertyChanged();
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public string SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                _selectedDepartment = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand GenerateAttendanceReportCommand { get; }
        public ICommand GenerateLeaveSummaryCommand { get; }
        public ICommand ExportAttendanceToCsvCommand { get; }
        public ICommand ExportLeaveSummaryToCsvCommand { get; }

        #endregion

        #region Methods

        private void LoadFilters()
        {
            var employeeList = _leaveService.GetAllEmployees();
            var allEmployee = new Employee { EmployeeId = 0, FirstName = "All", LastName = "Employees" };
            Employees = new ObservableCollection<Employee>(new[] { allEmployee }.Concat(employeeList));

            var deptList = employeeList.Select(e => e.Department).Distinct().ToList();
            Departments = new ObservableCollection<string>(new[] { "All" }.Concat(deptList));
        }

        private void ExecuteGenerateAttendanceReport(object parameter)
        {
            var employeeId = SelectedEmployeeId == 0 ? (int?)null : SelectedEmployeeId;
            var reports = _reportService.GetMonthlyAttendanceReport(SelectedMonth, SelectedYear, employeeId);
            AttendanceReports = new ObservableCollection<MonthlyAttendanceReport>(reports);

            if (reports.Count == 0)
            {
                MessageBox.Show("No attendance data found for the selected criteria.", "No Data",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteGenerateLeaveSummary(object parameter)
        {
            var department = SelectedDepartment == "All" ? null : SelectedDepartment;
            var reports = _reportService.GetLeaveSummaryByDept(StartDate, EndDate, department);
            LeaveSummaryReports = new ObservableCollection<LeaveSummaryReport>(reports);

            if (reports.Count == 0)
            {
                MessageBox.Show("No leave data found for the selected criteria.", "No Data",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteExportAttendanceToCsv(object parameter)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"AttendanceReport_{SelectedMonth}_{SelectedYear}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("Employee ID,Employee Name,Department,Designation,Present Days,Absent Days,Half Days,Late Check-ins,Avg Working Hours");

                    foreach (var report in AttendanceReports)
                    {
                        csv.AppendLine($"{report.EmployeeId},{report.EmployeeName},{report.Department},{report.Designation}," +
                            $"{report.TotalPresentDays},{report.TotalAbsentDays},{report.TotalHalfDays}," +
                            $"{report.LateCheckIns},{report.AverageWorkingHours:F2}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString());
                    MessageBox.Show("Report exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExportLeaveSummaryToCsv(object parameter)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"LeaveSummary_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("Department,Total Requests,Approved,Rejected,Pending,Most Common Type,Total Days");

                    foreach (var report in LeaveSummaryReports)
                    {
                        csv.AppendLine($"{report.Department},{report.TotalLeaveRequests},{report.ApprovedLeaves}," +
                            $"{report.RejectedLeaves},{report.PendingLeaves},{report.MostCommonLeaveType},{report.TotalLeaveDays}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString());
                    MessageBox.Show("Report exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteExportAttendance(object parameter)
        {
            return AttendanceReports != null && AttendanceReports.Count > 0;
        }

        private bool CanExecuteExportLeaveSummary(object parameter)
        {
            return LeaveSummaryReports != null && LeaveSummaryReports.Count > 0;
        }

        #endregion
    }
}