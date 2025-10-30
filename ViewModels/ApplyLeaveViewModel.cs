using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Employee_Management_System.Commands;
using Employee_Management_System.Models;
using Employee_Management_System.Services;

namespace Employee_Management_System.ViewModels
{
    public class ApplyLeaveViewModel : BaseViewModel
    {
        private readonly LeaveService _leaveService;
        private int _selectedEmployeeId;
        private string _selectedLeaveType;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _remarks;
        private int _duration;
        private int _leaveBalance;

        public ApplyLeaveViewModel()
        {
            _leaveService = new LeaveService();
            _selectedEmployeeId = SessionManager.CurrentEmployeeId;
            _startDate = DateTime.Today;
            _endDate = DateTime.Today;

            LeaveTypes = new List<string> { "Sick Leave", "Casual Leave", "Annual Leave" };

            // ✅ DEBUG: Check if data loads
            System.Diagnostics.Debug.WriteLine($"🔍 Session Employee ID: {SessionManager.CurrentEmployeeId}");
            System.Diagnostics.Debug.WriteLine($"🔍 Is Admin: {SessionManager.IsAdmin}");

            LoadEmployees();

            // ✅ DEBUG: Verify after loading
            System.Diagnostics.Debug.WriteLine($"✅ Employees loaded: {Employees?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"✅ Leave types: {LeaveTypes?.Count ?? 0}");

            SubmitCommand = new RelayCommand(ExecuteSubmit, CanExecuteSubmit);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        public Action CloseAction { get; set; }
        private List<Employee> _employees;
        private List<string> _leaveTypes;

        public List<string> LeaveTypes
        {
            get => _leaveTypes;
            set
            {
                _leaveTypes = value;
                OnPropertyChanged();
            }
        }
        public bool IsAdmin => SessionManager.IsAdmin;
        public List<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }
        public int SelectedEmployeeId
        {
            get => _selectedEmployeeId;
            set
            {
                _selectedEmployeeId = value;
                OnPropertyChanged();
                UpdateLeaveBalance();
            }
        }

        public string SelectedLeaveType
        {
            get => _selectedLeaveType;
            set
            {
                _selectedLeaveType = value;
                OnPropertyChanged();
                UpdateLeaveBalance();
                ((RelayCommand)SubmitCommand).RaiseCanExecuteChanged();
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
                CalculateDuration();
                ((RelayCommand)SubmitCommand).RaiseCanExecuteChanged();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
                CalculateDuration();
                ((RelayCommand)SubmitCommand).RaiseCanExecuteChanged();
            }
        }

        public string Remarks
        {
            get => _remarks;
            set
            {
                _remarks = value;
                OnPropertyChanged();
                ((RelayCommand)SubmitCommand).RaiseCanExecuteChanged();
            }
        }

        public int Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }

        public int LeaveBalance
        {
            get => _leaveBalance;
            set
            {
                _leaveBalance = value;
                OnPropertyChanged();
            }
        }

        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        private void LoadEmployees()
        {
            try
            {
                var employeeList = _leaveService.GetAllEmployees();
                System.Diagnostics.Debug.WriteLine($"📊 Loaded {employeeList.Count} employees");

                if (IsAdmin)
                {
                    Employees = employeeList;
                }
                else
                {
                    var currentEmployee = employeeList
                        .FirstOrDefault(e => e.EmployeeId == SessionManager.CurrentEmployeeId);
                    Employees = currentEmployee != null ? new List<Employee> { currentEmployee } : new List<Employee>();
                }

                // ✅ FIX: Set initial selection if employees exist
                if (Employees != null && Employees.Any())
                {
                    SelectedEmployeeId = Employees.First().EmployeeId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ LoadEmployees Error: {ex.Message}");
                Employees = new List<Employee>();
            }
        }

        private void CalculateDuration()
        {
            if (StartDate.HasValue && EndDate.HasValue && EndDate >= StartDate)
            {
                Duration = _leaveService.CalculateDuration(StartDate.Value, EndDate.Value);
            }
            else
            {
                Duration = 0;
            }
        }

        private void UpdateLeaveBalance()
        {
            if (SelectedEmployeeId > 0 && !string.IsNullOrEmpty(SelectedLeaveType))
            {
                LeaveBalance = _leaveService.GetLeaveBalance(SelectedEmployeeId, SelectedLeaveType);
            }
            else
            {
                LeaveBalance = 0;
            }
        }

        private void ExecuteSubmit(object parameter)
        {
            if (!ValidateInput())
                return;

            if (_leaveService.CheckOverlap(SelectedEmployeeId, StartDate.Value, EndDate.Value))
            {
                MessageBox.Show("You already have a leave request for the selected dates.", "Overlap Detected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = SelectedEmployeeId,
                LeaveType = SelectedLeaveType,
                StartDate = StartDate.Value,
                EndDate = EndDate.Value,
                Remarks = Remarks,
                Status = "Pending"
            };

            if (_leaveService.ApplyLeave(leaveRequest))
            {
                MessageBox.Show("Leave request submitted successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            else
            {
                MessageBox.Show("Failed to submit leave request. Please try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSubmit(object parameter)
        {
            return SelectedEmployeeId > 0 &&
                   !string.IsNullOrEmpty(SelectedLeaveType) &&
                   StartDate.HasValue &&
                   EndDate.HasValue &&
                   EndDate >= StartDate &&
                   Duration > 0;
        }

        private bool ValidateInput()
        {
            if (SelectedEmployeeId <= 0)
            {
                MessageBox.Show("Please select an employee.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(SelectedLeaveType))
            {
                MessageBox.Show("Please select a leave type.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!StartDate.HasValue || !EndDate.HasValue)
            {
                MessageBox.Show("Please select start and end dates.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EndDate < StartDate)
            {
                MessageBox.Show("End date must be greater than or equal to start date.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (StartDate.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Cannot apply for leave on past dates.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (SelectedLeaveType == "Sick Leave" && Duration > 2 && string.IsNullOrWhiteSpace(Remarks))
            {
                MessageBox.Show("Remarks are required for sick leave longer than 2 days.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ExecuteCancel(object parameter)
        {
            Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.Close();
        }
    }
}