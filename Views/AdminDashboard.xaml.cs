using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Employee_Management_System.Services;
using Employee_Management_System.UserControls;

namespace Employee_Management_System.Views
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard()
        {
            InitializeComponent();

            // Set current date
            CurrentDateText.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");

            // Load dashboard statistics
            LoadDashboardStatistics();
        }

        private void LoadDashboardStatistics()
        {
            try
            {
                var employeeService = new EmployeeManagementService();
                var attendanceService = new AttendanceManagementService();
                var leaveService = new LeaveService();

                // Total Active Employees
                TotalEmployeesText.Text = GetTotalActiveEmployees(employeeService).ToString();

                // Employees Present Today
                PresentTodayText.Text = GetEmployeesPresentToday(attendanceService).ToString();

                // Pending Leave Requests
                PendingLeavesText.Text = GetPendingLeaveRequestsCount(leaveService).ToString();

                // Approved Leaves This Month
                ApprovedLeavesText.Text = GetApprovedLeavesThisMonth(leaveService).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard statistics: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetTotalActiveEmployees(EmployeeManagementService service)
        {
            try
            {
                var employees = service.GetAllEmployees();
                var activeCount = employees.Count(e => e.Status == "Active");

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Total Employees: {employees.Count}, Active: {activeCount}");

                return activeCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTotalActiveEmployees Error: {ex.Message}");
                return 0;
            }
        }

        private int GetEmployeesPresentToday(AttendanceManagementService service)
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Get attendance records for today
                var attendance = service.GetAttendanceReport(null, today, tomorrow);
                var presentCount = attendance.Count(a => a.Status == "Present" && a.AttendanceDate.Date == today);

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Total Attendance Records: {attendance.Count}, Present Today: {presentCount}");

                return presentCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetEmployeesPresentToday Error: {ex.Message}");
                return 0;
            }
        }

        private int GetPendingLeaveRequestsCount(LeaveService service)
        {
            try
            {
                // Use GetPendingLeaves() method that already exists
                var pendingLeaves = service.GetPendingLeaves();

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Pending Leaves: {pendingLeaves.Count}");

                return pendingLeaves.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPendingLeaveRequestsCount Error: {ex.Message}");
                return 0;
            }
        }

        private int GetApprovedLeavesThisMonth(LeaveService service)
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                // Get all approved leaves
                var allApprovedLeaves = service.GetAllLeaveRequests("Approved", null);

                // Filter for current month
                var thisMonthCount = allApprovedLeaves.Count(l =>
                    l.StartDate.Month == currentMonth && l.StartDate.Year == currentYear);

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Total Approved: {allApprovedLeaves.Count}, This Month: {thisMonthCount}");

                return thisMonthCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetApprovedLeavesThisMonth Error: {ex.Message}");
                return 0;
            }
        }

        private void EmployeeManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(new EmployeeManagement());
        }

        private void AttendanceTrackingButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(new AttendanceManagement());
        }

        private void LeaveRequestsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(new LeaveManagement());
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(new Reports());
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardStatistics();
            MessageBox.Show("Dashboard statistics refreshed!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings module coming soon!", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowUserControl(UserControl control)
        {
            DynamicContent.Content = control;
            ContentArea.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DynamicContent.Content = null;
            ContentArea.Visibility = Visibility.Collapsed;
            LoadDashboardStatistics();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SessionManager.ClearSession();

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                this.Close();
            }
        }
    }
}