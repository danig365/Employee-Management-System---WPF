using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Employee_Management_System.Models;
using Employee_Management_System.Services;

namespace Employee_Management_System.UserControls
{
    /// <summary>
    /// Interaction logic for MyAttendanceControl.xaml
    /// View attendance history and statistics for the logged-in employee
    /// </summary>
    public partial class MyAttendanceControl : UserControl
    {
        private readonly AttendanceManagementService _attendanceService;
        private readonly int _employeeId;

        public MyAttendanceControl()
        {
            InitializeComponent();

            _attendanceService = new AttendanceManagementService();
            _employeeId = SessionManager.CurrentEmployeeId;

            InitializeDatePickers();
            LoadAttendanceData();
        }

        /// <summary>
        /// Initializes date pickers with default values (current month)
        /// </summary>
        private void InitializeDatePickers()
        {
            // Set default date range: First day of current month to today
            StartDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDatePicker.SelectedDate = DateTime.Now;
        }

        /// <summary>
        /// Loads attendance data based on selected date range
        /// </summary>
        private void LoadAttendanceData()
        {
            try
            {
                var startDate = StartDatePicker.SelectedDate ?? DateTime.Today.AddMonths(-1);
                var endDate = EndDatePicker.SelectedDate ?? DateTime.Today;

                // Validate date range
                if (endDate < startDate)
                {
                    MessageBox.Show("End date must be greater than or equal to start date.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get attendance records from service
                var attendanceList = _attendanceService.GetMyAttendance(_employeeId, startDate, endDate);

                // Sort by date descending (most recent first)
                var sortedList = attendanceList.OrderByDescending(a => a.AttendanceDate).ToList();

                // Bind to DataGrid
                AttendanceDataGrid.ItemsSource = sortedList;

                // Calculate and display statistics
                CalculateStatistics(attendanceList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Calculates and displays attendance statistics
        /// </summary>
        /// <param name="attendanceList">List of attendance records</param>
        private void CalculateStatistics(List<Attendance> attendanceList)
        {
            if (attendanceList == null || !attendanceList.Any())
            {
                // No data - reset all statistics to zero
                TotalPresentText.Text = "0";
                TotalAbsentText.Text = "0";
                TotalHalfDaysText.Text = "0";
                AvgHoursText.Text = "0.00";
                return;
            }

            // Count different attendance statuses
            var totalPresent = attendanceList.Count(a => a.Status == "Present");
            var totalAbsent = attendanceList.Count(a => a.Status == "Absent");
            var totalHalfDays = attendanceList.Count(a => a.Status == "Half-day");

            // Calculate average hours (only for Present days with hours > 0)
            var hoursData = attendanceList
                .Where(a => a.Status == "Present" && a.TotalHours > 0)
                .ToList();

            var avgHours = hoursData.Any() ? hoursData.Average(a => (double)a.TotalHours) : 0;

            // Update UI with calculated statistics
            TotalPresentText.Text = totalPresent.ToString();
            TotalAbsentText.Text = totalAbsent.ToString();
            TotalHalfDaysText.Text = totalHalfDays.ToString();
            AvgHoursText.Text = avgHours.ToString("F2");
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAttendanceData();
        }

        /// <summary>
        /// Handles the Reset button click event
        /// Resets date pickers to default values and reloads data
        /// </summary>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeDatePickers();
            LoadAttendanceData();
        }
    }
}