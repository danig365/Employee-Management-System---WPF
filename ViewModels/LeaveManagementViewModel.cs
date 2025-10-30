using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Employee_Management_System.Commands;
using Employee_Management_System.Models;
using Employee_Management_System.Services;

namespace Employee_Management_System.ViewModels
{
    public class LeaveManagementViewModel : BaseViewModel
    {
        private readonly LeaveService _leaveService;
        private ObservableCollection<LeaveRequest> _leaveRequests;
        private LeaveRequest _selectedLeaveRequest;
        private string _selectedStatusFilter;
        private int? _selectedEmployeeFilter;
        private bool _isApproveSelected;
        private bool _isRejectSelected;
        private string _adminRemarks;
        private int _pendingCount;
        private ObservableCollection<Employee> _employees;
        public LeaveManagementViewModel()
        {
            _leaveService = new LeaveService();
            _selectedStatusFilter = "All";
            _isApproveSelected = true;

            LoadLeaveRequests();
            LoadFilters();

            ApplyLeaveCommand = new RelayCommand(ExecuteApplyLeave);
            ApproveCommand = new RelayCommand(ExecuteApprove, CanExecuteAction);
            RejectCommand = new RelayCommand(ExecuteReject, CanExecuteAction);
            FilterCommand = new RelayCommand(ExecuteFilter);
            ViewPendingCommand = new RelayCommand(ExecuteViewPending);
            ConfirmActionCommand = new RelayCommand(ExecuteConfirmAction, CanExecuteConfirmAction);
        }

        public ObservableCollection<LeaveRequest> LeaveRequests
        {
            get => _leaveRequests;
            set
            {
                _leaveRequests = value;
                OnPropertyChanged();
            }
        }

        public LeaveRequest SelectedLeaveRequest
        {
            get => _selectedLeaveRequest;
            set
            {
                _selectedLeaveRequest = value;
                OnPropertyChanged();
                ((RelayCommand)ApproveCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RejectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ConfirmActionCommand).RaiseCanExecuteChanged();
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged();
            }
        }

        public int? SelectedEmployeeFilter
        {
            get => _selectedEmployeeFilter;
            set
            {
                _selectedEmployeeFilter = value;
                OnPropertyChanged();
            }
        }

        public bool IsApproveSelected
        {
            get => _isApproveSelected;
            set
            {
                _isApproveSelected = value;
                OnPropertyChanged();
                if (value) IsRejectSelected = false;
                ((RelayCommand)ConfirmActionCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsRejectSelected
        {
            get => _isRejectSelected;
            set
            {
                _isRejectSelected = value;
                OnPropertyChanged();
                if (value) IsApproveSelected = false;
                ((RelayCommand)ConfirmActionCommand).RaiseCanExecuteChanged();
            }
        }

        public string AdminRemarks
        {
            get => _adminRemarks;
            set
            {
                _adminRemarks = value;
                OnPropertyChanged();
                ((RelayCommand)ConfirmActionCommand).RaiseCanExecuteChanged();
            }
        }

        public int PendingCount
        {
            get => _pendingCount;
            set
            {
                _pendingCount = value;
                OnPropertyChanged();
            }
        }

        public List<string> StatusFilters => new List<string> { "All", "Pending", "Approved", "Rejected" };
        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }

        public ICommand ApplyLeaveCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ViewPendingCommand { get; }
        public ICommand ConfirmActionCommand { get; }

        private void LoadLeaveRequests()
        {
            var leaves = _leaveService.GetAllLeaveRequests();
            LeaveRequests = new ObservableCollection<LeaveRequest>(leaves);
            PendingCount = leaves.Count(l => l.Status == "Pending");
        }

        private void LoadFilters()
        {
            var employees = _leaveService.GetAllEmployees();

            // Add "All" option at the beginning
            var allEmployee = new Employee
            {
                EmployeeId = 0,
                FirstName = "All",
                LastName = ""
            };

            var employeeList = new List<Employee> { allEmployee };
            employeeList.AddRange(employees);

            Employees = new ObservableCollection<Employee>(employeeList);
        }

        private void ExecuteApplyLeave(object parameter)
        {
            var applyWindow = new Views.ApplyLeaveWindow();
            if (applyWindow.ShowDialog() == true)
            {
                LoadLeaveRequests();
            }
        }

        private void ExecuteApprove(object parameter)
        {
            IsApproveSelected = true;
            IsRejectSelected = false;
            AdminRemarks = string.Empty;
        }

        private void ExecuteReject(object parameter)
        {
            IsRejectSelected = true;
            IsApproveSelected = false;
            AdminRemarks = string.Empty;
        }

        private bool CanExecuteAction(object parameter)
        {
            return SelectedLeaveRequest != null && SelectedLeaveRequest.Status == "Pending";
        }

        private void ExecuteFilter(object parameter)
        {
            var status = SelectedStatusFilter == "All" ? null : SelectedStatusFilter;
            var employeeId = SelectedEmployeeFilter == 0 ? (int?)null : SelectedEmployeeFilter;
            var leaves = _leaveService.GetAllLeaveRequests(status, employeeId);
            LeaveRequests = new ObservableCollection<LeaveRequest>(leaves);
        }

        private void ExecuteViewPending(object parameter)
        {
            SelectedStatusFilter = "Pending";
            ExecuteFilter(null);
        }

        private void ExecuteConfirmAction(object parameter)
        {
            if (SelectedLeaveRequest == null) return;

            bool success;
            if (IsApproveSelected)
            {
                success = _leaveService.ApproveLeave(SelectedLeaveRequest.LeaveId, SessionManager.CurrentEmployeeId, AdminRemarks);
                if (success)
                {
                    MessageBox.Show("Leave approved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (IsRejectSelected)
            {
                if (string.IsNullOrWhiteSpace(AdminRemarks))
                {
                    MessageBox.Show("Please provide a reason for rejection.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                success = _leaveService.RejectLeave(SelectedLeaveRequest.LeaveId, SessionManager.CurrentEmployeeId, AdminRemarks);
                if (success)
                {
                    MessageBox.Show("Leave rejected successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select Approve or Reject.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (success)
            {
                LoadLeaveRequests();
                SelectedLeaveRequest = null;
                AdminRemarks = string.Empty;
                IsApproveSelected = true;
                IsRejectSelected = false;
            }
            else
            {
                MessageBox.Show("Failed to process leave request.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteConfirmAction(object parameter)
        {
            return SelectedLeaveRequest != null &&
                   SelectedLeaveRequest.Status == "Pending" &&
                   (IsApproveSelected || (IsRejectSelected && !string.IsNullOrWhiteSpace(AdminRemarks)));
        }
    }
}