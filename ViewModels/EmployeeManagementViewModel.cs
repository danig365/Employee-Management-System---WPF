using Employee_Management_System.Services;
using System.Windows.Input;
using Employee_Management_System.Commands;
using Employee_Management_System.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Employee_Management_System.UserControls;

namespace Employee_Management_System.ViewModels
{
    public class EmployeeManagementViewModel : BaseViewModel
    {
        private readonly EmployeeManagementService _employeeManagementService;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<Employee> _filteredEmployees;
        private Employee _selectedEmployee;
        private string _searchText;
        private string _selectedDepartment;

        public EmployeeManagementViewModel()
        {
            _employeeManagementService = new EmployeeManagementService();

            // Initialize Commands
            SearchCommand = new RelayCommand(ExecuteSearch, CanExecuteSearch);
            Add_New_Employee = new RelayCommand(ExecuteAddEmployee);
            Refresh = new RelayCommand(ExecuteRefresh);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ViewDetailsCommand = new RelayCommand(ExecuteViewDetails, CanExecuteViewDetails);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteDelete);

            // Load initial data
            LoadEmployees();
        }

        #region Properties

        public ObservableCollection<Employee> Employees
        {
            get { return _employees; }
            set
            {
                _employees = value;
                OnPropertyChanged(nameof(Employees));
            }
        }

        public ObservableCollection<Employee> FilteredEmployees
        {
            get { return _filteredEmployees; }
            set
            {
                _filteredEmployees = value;
                OnPropertyChanged(nameof(FilteredEmployees));
            }
        }

        public Employee SelectedEmployee
        {
            get { return _selectedEmployee; }
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged(nameof(SelectedEmployee));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectedDepartment
        {
            get { return _selectedDepartment; }
            set
            {
                _selectedDepartment = value;
                OnPropertyChanged(nameof(SelectedDepartment));
            }
        }

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand Add_New_Employee { get; }
        public ICommand Refresh { get; }
        public ICommand EditCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand DeleteCommand { get; }

        #endregion

        #region Command Methods

        private void LoadEmployees()
        {
            try
            {
                var employeeList = _employeeManagementService.GetAllEmployees();
                Employees = new ObservableCollection<Employee>(employeeList);
                FilteredEmployees = new ObservableCollection<Employee>(employeeList);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSearch(object parameter)
        {
            return !string.IsNullOrWhiteSpace(SearchText) || !string.IsNullOrEmpty(SelectedDepartment);
        }

        private void ExecuteSearch(object parameter)
        {
            try
            {
                if (Employees == null || Employees.Count == 0)
                {
                    LoadEmployees();
                }

                var filtered = Employees.AsEnumerable();

                // Filter by search text (search in FirstName, LastName, Email, Phone)
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLower();
                    filtered = filtered.Where(e =>
                        (e.FirstName?.ToLower().Contains(searchLower) ?? false) ||
                        (e.LastName?.ToLower().Contains(searchLower) ?? false) ||
                        (e.Email?.ToLower().Contains(searchLower) ?? false) ||
                        (e.Phone?.Contains(SearchText) ?? false) ||
                        e.EmployeeId.ToString().Contains(SearchText)
                    );
                }

                // Filter by department
                if (!string.IsNullOrEmpty(SelectedDepartment) && SelectedDepartment != "All Departments")
                {
                    filtered = filtered.Where(e => e.Department == SelectedDepartment);
                }

                FilteredEmployees = new ObservableCollection<Employee>(filtered);

                if (FilteredEmployees.Count == 0)
                {
                    MessageBox.Show("No employees found matching the search criteria.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddEmployee(object parameter)
        {
            try
            {
                var addEmployeeWindow = new AddEditEmployeeWindow(null);
                addEmployeeWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                if (addEmployeeWindow.ShowDialog() == true)
                {
                    LoadEmployees();
                    MessageBox.Show("Employee added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error opening add employee window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRefresh(object parameter)
        {
            SearchText = string.Empty;
            SelectedDepartment = "All Departments";
            LoadEmployees();
            MessageBox.Show("Employee list refreshed!", "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanExecuteEdit(object parameter)
        {
            return SelectedEmployee != null;
        }

        private void ExecuteEdit(object parameter)
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Please select an employee to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var editEmployeeWindow = new AddEditEmployeeWindow(SelectedEmployee);
                editEmployeeWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                if (editEmployeeWindow.ShowDialog() == true)
                {
                    LoadEmployees();
                    MessageBox.Show("Employee updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error opening edit employee window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteViewDetails(object parameter)
        {
            return SelectedEmployee != null;
        }

        private void ExecuteViewDetails(object parameter)
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Please select an employee to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var details = $"Employee Details:\n\n" +
                             $"Employee ID: {SelectedEmployee.EmployeeId}\n" +
                             $"Name: {SelectedEmployee.FullName}\n" +
                             $"Department: {SelectedEmployee.Department}\n" +
                             $"Designation: {SelectedEmployee.Designation}\n" +
                             $"Join Date: {SelectedEmployee.JoinDate?.ToString("dd/MM/yyyy") ?? "N/A"}\n" +
                             $"Status: {SelectedEmployee.Status}\n" +
                             $"Email: {SelectedEmployee.Email}\n" +
                             $"Phone: {SelectedEmployee.Phone}";

                MessageBox.Show(details, "Employee Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error viewing employee details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteDelete(object parameter)
        {
            return SelectedEmployee != null;
        }

        private void ExecuteDelete(object parameter)
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Please select an employee to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete employee '{SelectedEmployee.FullName}'?\n\n" +
                    $"This will mark the employee as Inactive.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Soft delete - mark as Inactive
                    SelectedEmployee.Status = "Inactive";
                    _employeeManagementService.UpdateEmployee(SelectedEmployee);

                    LoadEmployees();
                    MessageBox.Show("Employee marked as Inactive successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error deleting employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}