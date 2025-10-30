using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Employee_Management_System.Services;

namespace Employee_Management_System.UserControls
{
    /// <summary>
    /// Interaction logic for EmployeeAttendanceControl.xaml
    /// Mark Check-In and Check-Out for employees
    /// </summary>
    public partial class EmployeeAttendanceControl : UserControl
    {
        private readonly AttendanceManagementService _attendanceService;
        private readonly int _employeeId;

        public EmployeeAttendanceControl()
        {
            InitializeComponent();

            _attendanceService = new AttendanceManagementService();
            _employeeId = SessionManager.CurrentEmployeeId;

            LoadTodayAttendance();
            TodayDateText.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        /// <summary>
        /// Loads today's attendance status for the current employee
        /// </summary>
        private void LoadTodayAttendance()
        {
            try
            {
                var attendance = _attendanceService.GetEmployeeAttendanceForToday(_employeeId);

                if (attendance != null)
                {
                    // Display Check In
                    if (attendance.CheckIn.HasValue)
                    {
                        CheckInTimeText.Text = attendance.CheckIn.Value.ToString("hh:mm tt");
                        CheckInButton.IsEnabled = false;
                        CheckOutButton.IsEnabled = true;
                    }

                    // Display Check Out
                    if (attendance.CheckOut.HasValue)
                    {
                        CheckOutTimeText.Text = attendance.CheckOut.Value.ToString("hh:mm tt");
                        CheckInButton.IsEnabled = false;
                        CheckOutButton.IsEnabled = false;

                        ShowStatusMessage(
                            $"✅ You have completed your attendance for today. Total hours: {attendance.TotalHours:F2}",
                            Brushes.Green);
                    }
                    else
                    {
                        ShowStatusMessage(
                            "⏰ You are checked in. Don't forget to check out when you leave!",
                            Brushes.Orange);
                    }
                }
                else
                {
                    // No attendance record for today
                    CheckInButton.IsEnabled = true;
                    CheckOutButton.IsEnabled = false;
                    ShowStatusMessage(
                        "📌 You haven't checked in today. Click 'Check In' to mark your attendance.",
                        Brushes.DodgerBlue);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"❌ Error loading attendance: {ex.Message}", Brushes.Red);
            }
        }

        /// <summary>
        /// Handles the Check In button click event
        /// </summary>
        private void CheckInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to check in at {DateTime.Now:hh:mm tt}?",
                    "Confirm Check In",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _attendanceService.MarkCheckIn(_employeeId);

                    MessageBox.Show(
                        "✅ Check-in recorded successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    LoadTodayAttendance();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error during check-in: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Check Out button click event
        /// </summary>
        private void CheckOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to check out at {DateTime.Now:hh:mm tt}?",
                    "Confirm Check Out",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _attendanceService.MarkCheckOut(_employeeId);

                    var attendance = _attendanceService.GetEmployeeAttendanceForToday(_employeeId);
                    var hoursWorked = attendance?.TotalHours ?? 0;

                    MessageBox.Show(
                        $"✅ Check-out recorded successfully!\n\nTotal hours worked: {hoursWorked:F2} hours",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    LoadTodayAttendance();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error during check-out: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Displays a status message to the user
        /// </summary>
        private void ShowStatusMessage(string message, Brush color)
        {
            StatusMessageText.Text = message;
            StatusMessageText.Foreground = color;
            StatusMessageBorder.Visibility = Visibility.Visible;
        }
    }
}