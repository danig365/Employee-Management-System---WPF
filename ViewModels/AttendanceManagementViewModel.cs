using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Employee_Management_System.Commands;
using Employee_Management_System.Models;
using Employee_Management_System.Services;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Employee_Management_System.ViewModels
{
    public class AttendanceManagementViewModel : BaseViewModel
    {
        private readonly AttendanceManagementService _attendanceService;
        private readonly EmployeeManagementService _employeeService;

        // Properties for employee selection
        private ObservableCollection<Employee> _employees;
        private Employee _selectedEmployee;

        // Properties for today's attendance (Admin)
        private Attendance _todayAttendance;
        private string _checkInStatus;
        private string _checkOutStatus;

        // Properties for manual entry
        private DateTime _manualDate;
        private string _manualCheckInText;
        private string _manualCheckOutText;
        private string _manualStatus;

        // Properties for attendance report
        private ObservableCollection<Attendance> _attendanceRecords;
        private DateTime _filterStartDate;
        private DateTime _filterEndDate;
        private Employee _filterEmployee;

        // Summary properties
        private int _totalDays;
        private int _presentDays;
        private int _absentDays;
        private decimal _attendancePercentage;
        private string _avgCheckInTime;

        // UI state
        private string _errorMessage;
        private bool _isLoading;

        public AttendanceManagementViewModel()
        {
            _attendanceService = new AttendanceManagementService();
            _employeeService = new EmployeeManagementService();

            // Initialize collections
            Employees = new ObservableCollection<Employee>();
            AttendanceRecords = new ObservableCollection<Attendance>();

            // Initialize dates
            ManualDate = DateTime.Today;
            FilterStartDate = DateTime.Today.AddMonths(-1);
            FilterEndDate = DateTime.Today;

            // Initialize commands
            MarkCheckInCommand = new RelayCommand(ExecuteMarkCheckIn, CanExecuteMarkCheckIn);
            MarkCheckOutCommand = new RelayCommand(ExecuteMarkCheckOut, CanExecuteMarkCheckOut);
            SaveManualEntryCommand = new RelayCommand(ExecuteSaveManualEntry, CanExecuteSaveManualEntry);
            ApplyFilterCommand = new RelayCommand(ExecuteApplyFilter);
            ExportToCsvCommand = new RelayCommand(ExecuteExportToCsv, CanExecuteExportToCsv);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            ManualStatus = "Present"; // Add this line
            // Load data
            LoadInitialData();
        }

        #region Properties

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged(nameof(Employees));
            }
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged(nameof(SelectedEmployee));
                LoadEmployeeTodayAttendance();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Attendance TodayAttendance
        {
            get => _todayAttendance;
            set
            {
                _todayAttendance = value;
                OnPropertyChanged(nameof(TodayAttendance));
                UpdateAttendanceStatus();
            }
        }

        public string CheckInStatus
        {
            get => _checkInStatus;
            set
            {
                _checkInStatus = value;
                OnPropertyChanged(nameof(CheckInStatus));
            }
        }

        public string CheckOutStatus
        {
            get => _checkOutStatus;
            set
            {
                _checkOutStatus = value;
                OnPropertyChanged(nameof(CheckOutStatus));
            }
        }

        public DateTime ManualDate
        {
            get => _manualDate;
            set
            {
                _manualDate = value;
                OnPropertyChanged(nameof(ManualDate));
            }
        }

        public string ManualCheckIn
        {
            get => _manualCheckInText;
            set
            {
                _manualCheckInText = value;
                OnPropertyChanged(nameof(ManualCheckIn));
            }
        }

        public string ManualCheckOut
        {
            get => _manualCheckOutText;
            set
            {
                _manualCheckOutText = value;
                OnPropertyChanged(nameof(ManualCheckOut));
            }
        }

        public string ManualStatus
        {
            get => _manualStatus;
            set
            {
                _manualStatus = value;
                OnPropertyChanged(nameof(ManualStatus));
            }
        }

        public ObservableCollection<Attendance> AttendanceRecords
        {
            get => _attendanceRecords;
            set
            {
                _attendanceRecords = value;
                OnPropertyChanged(nameof(AttendanceRecords));
            }
        }

        public DateTime FilterStartDate
        {
            get => _filterStartDate;
            set
            {
                _filterStartDate = value;
                OnPropertyChanged(nameof(FilterStartDate));
            }
        }

        public DateTime FilterEndDate
        {
            get => _filterEndDate;
            set
            {
                _filterEndDate = value;
                OnPropertyChanged(nameof(FilterEndDate));
            }
        }

        public Employee FilterEmployee
        {
            get => _filterEmployee;
            set
            {
                _filterEmployee = value;
                OnPropertyChanged(nameof(FilterEmployee));
            }
        }

        public int TotalDays
        {
            get => _totalDays;
            set
            {
                _totalDays = value;
                OnPropertyChanged(nameof(TotalDays));
            }
        }

        public int PresentDays
        {
            get => _presentDays;
            set
            {
                _presentDays = value;
                OnPropertyChanged(nameof(PresentDays));
            }
        }

        public int AbsentDays
        {
            get => _absentDays;
            set
            {
                _absentDays = value;
                OnPropertyChanged(nameof(AbsentDays));
            }
        }

        public decimal AttendancePercentage
        {
            get => _attendancePercentage;
            set
            {
                _attendancePercentage = value;
                OnPropertyChanged(nameof(AttendancePercentage));
            }
        }

        public string AvgCheckInTime
        {
            get => _avgCheckInTime;
            set
            {
                _avgCheckInTime = value;
                OnPropertyChanged(nameof(AvgCheckInTime));
            }
        }

        // Use SessionManager for user role access
        public bool IsAdmin => SessionManager.IsAdmin;
        public bool IsEmployee => SessionManager.IsEmployee;
        public string CurrentUserRole => SessionManager.CurrentUserRole;
        public string CurrentUserName => SessionManager.CurrentUser?.UserName ?? "Unknown";

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string CurrentDate => DateTime.Today.ToString("dddd, MMMM dd, yyyy");

        #endregion

        #region Commands

        public ICommand MarkCheckInCommand { get; }
        public ICommand MarkCheckOutCommand { get; }
        public ICommand SaveManualEntryCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Command Methods

        private bool CanExecuteMarkCheckIn(object parameter)
        {
            return SelectedEmployee != null &&
                   (TodayAttendance == null || TodayAttendance.CheckIn == null);
        }

        private void ExecuteMarkCheckIn(object parameter)
        {
            try
            {
                if (SelectedEmployee == null) return;

                IsLoading = true;
                _attendanceService.MarkCheckIn(SelectedEmployee.EmployeeId);
                MessageBox.Show($"Check-in marked successfully for {SelectedEmployee.FirstName} {SelectedEmployee.LastName}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadEmployeeTodayAttendance();
                ExecuteRefresh(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking check-in: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteMarkCheckOut(object parameter)
        {
            return SelectedEmployee != null &&
                   TodayAttendance != null &&
                   TodayAttendance.CheckIn != null &&
                   TodayAttendance.CheckOut == null;
        }

        private void ExecuteMarkCheckOut(object parameter)
        {
            try
            {
                if (SelectedEmployee == null) return;

                IsLoading = true;
                _attendanceService.MarkCheckOut(SelectedEmployee.EmployeeId);
                MessageBox.Show($"Check-out marked successfully for {SelectedEmployee.FirstName} {SelectedEmployee.LastName}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadEmployeeTodayAttendance();
                ExecuteRefresh(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking check-out: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteSaveManualEntry(object parameter)
        {
            return SelectedEmployee != null && SessionManager.IsAdmin;
        }

        private void ExecuteSaveManualEntry(object parameter)
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Please select an employee.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime? checkInDateTime = null;
                DateTime? checkOutDateTime = null;

                if (!string.IsNullOrWhiteSpace(ManualCheckIn))
                {
                    if (DateTime.TryParse($"{ManualDate:yyyy-MM-dd} {ManualCheckIn}", out DateTime parsedCheckIn))
                    {
                        checkInDateTime = parsedCheckIn;
                    }
                    else
                    {
                        MessageBox.Show("Invalid check-in time format. Use HH:mm (e.g., 09:30)", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(ManualCheckOut))
                {
                    if (DateTime.TryParse($"{ManualDate:yyyy-MM-dd} {ManualCheckOut}", out DateTime parsedCheckOut))
                    {
                        checkOutDateTime = parsedCheckOut;
                    }
                    else
                    {
                        MessageBox.Show("Invalid check-out time format. Use HH:mm (e.g., 17:30)", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Validate times
                if (checkInDateTime != null && checkOutDateTime != null && checkOutDateTime < checkInDateTime)
                {
                    MessageBox.Show("Check-out time cannot be before check-in time.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsLoading = true;

                // Determine status if not provided
                string status = ManualStatus;
                if (string.IsNullOrWhiteSpace(status))
                {
                    if (checkInDateTime != null && checkOutDateTime != null)
                        status = "Present";
                    else if (checkInDateTime != null && checkOutDateTime == null)
                        status = "Half-day";
                    else
                        status = "Absent";
                }

                _attendanceService.ManualEntry(
                    SelectedEmployee.EmployeeId,
                    ManualDate,
                    checkInDateTime,
                    checkOutDateTime,
                    status);

                MessageBox.Show("Manual attendance entry saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear manual entry fields
                ManualCheckIn = string.Empty;
                ManualCheckOut = string.Empty;
                ManualStatus = string.Empty;
                ManualDate = DateTime.Today;

                ExecuteRefresh(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving manual entry: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteApplyFilter(object parameter)
        {
            try
            {
                IsLoading = true;

                if (SessionManager.IsAdmin)
                {
                    int? employeeId = null;

                    // Only set employeeId if a specific employee is selected (not "All Employees")
                    if (FilterEmployee != null && FilterEmployee.EmployeeId > 0)
                    {
                        employeeId = FilterEmployee.EmployeeId;
                    }

                    var records = _attendanceService.GetAttendanceReport(
                        employeeId, FilterStartDate, FilterEndDate);

                    AttendanceRecords.Clear();
                    foreach (var record in records)
                    {
                        AttendanceRecords.Add(record);
                    }
                }
                else
                {
                    // Employee view - existing code is correct
                    var records = _attendanceService.GetMyAttendance(
                        SessionManager.CurrentEmployeeId, FilterStartDate, FilterEndDate);

                    AttendanceRecords.Clear();
                    foreach (var record in records)
                    {
                        AttendanceRecords.Add(record);
                    }
                }

                CalculateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filter: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteExportToCsv(object parameter)
        {
            return AttendanceRecords != null && AttendanceRecords.Count > 0;
        }

        private void ExecuteExportToCsv(object parameter)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"AttendanceReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();

                    // Header
                    csv.AppendLine("Date,Employee Name,Check-In,Check-Out,Total Hours,Status");

                    // Data rows
                    foreach (var record in AttendanceRecords)
                    {
                        csv.AppendLine($"{record.AttendanceDate:yyyy-MM-dd}," +
                            $"{record.EmployeeName}," +
                            $"{record.CheckInDisplay}," +
                            $"{record.CheckOutDisplay}," +
                            $"{record.TotalHours}," +
                            $"{record.Status}");
                    }

                    File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                    MessageBox.Show("Attendance report exported successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRefresh(object parameter)
        {
            ExecuteApplyFilter(null);
            if (SessionManager.IsAdmin && SelectedEmployee != null)
            {
                LoadEmployeeTodayAttendance();
            }
        }

        #endregion

        #region Private Methods

        private void LoadInitialData()
        {
            try
            {
                IsLoading = true;

                // Initialize ManualStatus BEFORE the if statement
                ManualStatus = "Present";
                
                if (SessionManager.IsAdmin)
                {
                    // Load all employees for admin
                    var employees = _employeeService.GetAllEmployees();
                    Employees.Clear();

                    // Add "All Employees" option
                    Employees.Add(new Employee { EmployeeId = 0, FirstName = "All", LastName = "Employees" });
                    
                    foreach (var emp in employees)
                    {
                        Employees.Add(emp);
                    }

                    // Set default filter to "All Employees"
                    if (Employees.Count > 0)
                    {
                        FilterEmployee = Employees[0]; // Select "All Employees" by default
                    }
                }
                else
                {
                    // For employees, load only their own data
                    var currentEmployee = _employeeService.GetEmployeeById(SessionManager.CurrentEmployeeId);
                    if (currentEmployee != null)
                    {
                        Employees.Clear();
                        Employees.Add(currentEmployee);
                        SelectedEmployee = currentEmployee;
                        FilterEmployee = currentEmployee; // Add this line for employee view
                    }
                }

                // Load attendance records
                ExecuteApplyFilter(null);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadEmployeeTodayAttendance()
        {
            if (SelectedEmployee == null || SelectedEmployee.EmployeeId == 0) return;

            try
            {
                TodayAttendance = _attendanceService.GetEmployeeAttendanceForToday(SelectedEmployee.EmployeeId);
                UpdateAttendanceStatus();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading today's attendance: {ex.Message}";
            }
        }

        private void UpdateAttendanceStatus()
        {
            if (TodayAttendance == null || TodayAttendance.CheckIn == null)
            {
                CheckInStatus = "Not yet marked";
                CheckOutStatus = "Not yet marked";
            }
            else
            {
                CheckInStatus = TodayAttendance.CheckIn.Value.ToString("hh:mm tt") + " ✓";

                if (TodayAttendance.CheckOut == null)
                {
                    CheckOutStatus = "Not yet marked";
                }
                else
                {
                    CheckOutStatus = TodayAttendance.CheckOut.Value.ToString("hh:mm tt") + " ✓";
                }
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void CalculateSummary()
        {
            if (AttendanceRecords == null || AttendanceRecords.Count == 0)
            {
                TotalDays = 0;
                PresentDays = 0;
                AbsentDays = 0;
                AttendancePercentage = 0;
                AvgCheckInTime = "--:--";
                return;
            }

            TotalDays = AttendanceRecords.Count;
            PresentDays = AttendanceRecords.Count(r => r.Status == "Present" || r.Status == "Half-day");
            AbsentDays = AttendanceRecords.Count(r => r.Status == "Absent");

            if (TotalDays > 0)
            {
                AttendancePercentage = Math.Round((decimal)PresentDays / TotalDays * 100, 1);
            }

            // Calculate average check-in time
            var checkIns = AttendanceRecords.Where(r => r.CheckIn != null).Select(r => r.CheckIn.Value).ToList();

            if (checkIns.Any())
            {
                var avgTicks = (long)checkIns.Average(d => d.TimeOfDay.Ticks);
                var avgTime = new TimeSpan(avgTicks);
                AvgCheckInTime = DateTime.Today.Add(avgTime).ToString("hh:mm tt");
            }
            else
            {
                AvgCheckInTime = "--:--";
            }
        }

        #endregion
    }
}