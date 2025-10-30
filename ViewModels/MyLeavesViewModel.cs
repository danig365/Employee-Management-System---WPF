using Employee_Management_System.Commands;
using Employee_Management_System.Models;
using Employee_Management_System.Services;
using Employee_Management_System.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Employee_Management_System.ViewModels
{
    public class MyLeavesViewModel : BaseViewModel
    {
        private readonly LeaveService _leaveService;
        private ObservableCollection<LeaveRequest> _leaveRequests;
        private LeaveRequest _selectedLeaveRequest;
        private string _selectedStatusFilter;
        private List<LeaveRequest> _allLeaveRequests;
        private int _annualLeaveBalance;
        private int _sickLeaveBalance;
        private int _casualLeaveBalance;

        public MyLeavesViewModel()
        {
            _leaveService = new LeaveService();
            _selectedStatusFilter = "All";

            LoadLeaveRequests();
            LoadLeaveBalances();

            ApplyLeaveCommand = new RelayCommand(ExecuteApplyLeave);
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

        public int AnnualLeaveBalance
        {
            get => _annualLeaveBalance;
            set
            {
                _annualLeaveBalance = value;
                OnPropertyChanged();
            }
        }

        public int SickLeaveBalance
        {
            get => _sickLeaveBalance;
            set
            {
                _sickLeaveBalance = value;
                OnPropertyChanged();
            }
        }

        public int CasualLeaveBalance
        {
            get => _casualLeaveBalance;
            set
            {
                _casualLeaveBalance = value;
                OnPropertyChanged();
            }
        }

        public List<string> StatusFilters => new List<string> { "All", "Pending", "Approved", "Rejected" };

        public ICommand ApplyLeaveCommand { get; }

        private void LoadLeaveRequests()
        {
            _allLeaveRequests = _leaveService.GetLeaveHistory(SessionManager.CurrentEmployeeId);
            LeaveRequests = new ObservableCollection<LeaveRequest>(_allLeaveRequests);
        }

        private void LoadLeaveBalances()
        {
            AnnualLeaveBalance = _leaveService.GetLeaveBalance(SessionManager.CurrentEmployeeId, "Annual Leave");
            SickLeaveBalance = _leaveService.GetLeaveBalance(SessionManager.CurrentEmployeeId, "Sick Leave");
            CasualLeaveBalance = _leaveService.GetLeaveBalance(SessionManager.CurrentEmployeeId, "Casual Leave");
        }

        public void FilterLeaves()
        {
            if (SelectedStatusFilter == "All")
            {
                LeaveRequests = new ObservableCollection<LeaveRequest>(_allLeaveRequests);
            }
            else
            {
                var filtered = _allLeaveRequests.Where(l => l.Status == SelectedStatusFilter).ToList();
                LeaveRequests = new ObservableCollection<LeaveRequest>(filtered);
            }
        }

        private void ExecuteApplyLeave(object parameter)
        {
            var applyWindow = new ApplyLeaveWindow();
            if (applyWindow.ShowDialog() == true)
            {
                LoadLeaveRequests();
                LoadLeaveBalances();
            }
        }
    }
}