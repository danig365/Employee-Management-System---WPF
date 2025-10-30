using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Employee_Management_System.Services;
using Employee_Management_System.UserControls;

namespace Employee_Management_System.Views
{
    public partial class EmployeeProfile : Window
    {
        private readonly AttendanceManagementService _attendanceService;
        private readonly LeaveService _leaveService;
        private readonly EmployeeManagementService _employeeService;

        public EmployeeProfile()
        {
            InitializeComponent();

            _attendanceService = new AttendanceManagementService();
            _leaveService = new LeaveService();
            _employeeService = new EmployeeManagementService();

            LoadEmployeeData();
            LoadDashboardStatistics();
        }

        private void LoadEmployeeData()
        {
            try
            {
                var employeeId = SessionManager.CurrentEmployeeId;
                var employee = _employeeService.GetEmployeeById(employeeId);

                if (employee != null)
                {
                    WelcomeText.Text = $"Welcome back, {employee.FirstName}!";
                }

                CurrentDateText.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employee data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDashboardStatistics()
        {
            try
            {
                var employeeId = SessionManager.CurrentEmployeeId;

                // Load Today's Attendance Status
                var todayAttendance = _attendanceService.GetEmployeeAttendanceForToday(employeeId);
                if (todayAttendance != null)
                {
                    if (todayAttendance.CheckOut != null)
                    {
                        TodayStatusText.Text = "Checked Out";
                        TodayStatusText.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else if (todayAttendance.CheckIn != null)
                    {
                        TodayStatusText.Text = "Checked In";
                        TodayStatusText.Foreground = System.Windows.Media.Brushes.Orange;
                    }
                }
                else
                {
                    TodayStatusText.Text = "Not Checked In";
                    TodayStatusText.Foreground = System.Windows.Media.Brushes.Red;
                }

                // Load Leave Statistics
                var allLeaves = _leaveService.GetLeaveHistory(employeeId);
                TotalLeavesText.Text = allLeaves.Count.ToString();

                var pendingLeaves = allLeaves.Count(l => l.Status == "Pending");
                PendingLeavesText.Text = pendingLeaves.ToString();

                // Load Leave Balance
                var annualLeaveBalance = _leaveService.GetLeaveBalance(employeeId, "Annual Leave");
                LeaveBalanceText.Text = $"{annualLeaveBalance} days";

                // Load Recent Attendance
                LoadRecentAttendance();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard statistics: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentAttendance()
        {
            try
            {
                var employeeId = SessionManager.CurrentEmployeeId;
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-7); // Last 7 days

                var recentAttendance = _attendanceService.GetMyAttendance(employeeId, startDate, endDate);

                // Sort by date descending (most recent first)
                var sortedAttendance = recentAttendance.OrderByDescending(a => a.AttendanceDate).ToList();

                RecentActivityGrid.ItemsSource = sortedAttendance;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent attendance: {ex.Message}");
            }
        }

        private void MarkAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(new EmployeeAttendanceControl());
        }

        private void ViewAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(new MyAttendanceControl());
        }

        private void ApplyLeaveButton_Click(object sender, RoutedEventArgs e)
        {
            var applyWindow = new ApplyLeaveWindow();
            if (applyWindow.ShowDialog() == true)
            {
                LoadDashboardStatistics(); // Refresh stats after applying leave
            }
        }

        private void MyLeavesButton_Click(object sender, RoutedEventArgs e)
        {
            var leavesWindow = new MyLeavesWindow
            {
                DataContext = new ViewModels.MyLeavesViewModel()
            };
            leavesWindow.ShowDialog();
            LoadDashboardStatistics(); // Refresh stats after closing
        }

        private void ShowUserControl(UserControl control)
        {
            DynamicContent.Content = control;
            DashboardPanel.Visibility = Visibility.Collapsed;
            ContentArea.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DynamicContent.Content = null;
            ContentArea.Visibility = Visibility.Collapsed;
            DashboardPanel.Visibility = Visibility.Visible;
            LoadDashboardStatistics(); // Refresh stats when returning to dashboard
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