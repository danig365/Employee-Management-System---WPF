using Employee_Management_System.Commands;
using Employee_Management_System.Models;
using Employee_Management_System.Services;
using Employee_Management_System.ViewModels;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Employee_Management_System.UserControls
{
    public class AddEditEmployeeViewModel : BaseViewModel
    {
        private readonly EmployeeManagementService _employeeManagementService;
        private readonly Employee _existingEmployee;
        private readonly Action _closeAction;

        private string _windowTitle;
        private string _firstName;
        private string _lastName;
        private string _department;
        private string _designation;
        private DateTime? _joinDate;
        private bool _isActive;
        private bool _isInactive;
        private string _email;
        private string _phone;

        // Validation error properties
        private string _firstNameError;
        private string _lastNameError;
        private string _departmentError;
        private string _emailError;
        private string _phoneError;
        private Visibility _firstNameErrorVisibility = Visibility.Collapsed;
        private Visibility _lastNameErrorVisibility = Visibility.Collapsed;
        private Visibility _departmentErrorVisibility = Visibility.Collapsed;
        private Visibility _emailErrorVisibility = Visibility.Collapsed;
        private Visibility _phoneErrorVisibility = Visibility.Collapsed;

        public bool DialogResult { get; private set; }

        public AddEditEmployeeViewModel(Employee employee, Action closeAction)
        {
            _employeeManagementService = new EmployeeManagementService();
            _existingEmployee = employee;
            _closeAction = closeAction;

            // Initialize Commands
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);

            // Set window title and load data
            if (_existingEmployee == null)
            {
                WindowTitle = "Add New Employee";
                IsActive = true;
                JoinDate = DateTime.Now;
            }
            else
            {
                WindowTitle = "Edit Employee";
                LoadEmployeeData();
            }
        }

        #region Properties

        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                ValidateFirstName();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                ValidateLastName();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Department
        {
            get { return _department; }
            set
            {
                _department = value;
                OnPropertyChanged(nameof(Department));
                ValidateDepartment();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Designation
        {
            get { return _designation; }
            set
            {
                _designation = value;
                OnPropertyChanged(nameof(Designation));
            }
        }

        public DateTime? JoinDate
        {
            get { return _joinDate; }
            set
            {
                _joinDate = value;
                OnPropertyChanged(nameof(JoinDate));
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (value) _isInactive = false;
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(IsInactive));
            }
        }

        public bool IsInactive
        {
            get { return _isInactive; }
            set
            {
                _isInactive = value;
                if (value) _isActive = false;
                OnPropertyChanged(nameof(IsInactive));
                OnPropertyChanged(nameof(IsActive));
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                ValidateEmail();
            }
        }

        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
                ValidatePhone();
            }
        }

        #endregion

        #region Validation Error Properties

        public string FirstNameError
        {
            get { return _firstNameError; }
            set
            {
                _firstNameError = value;
                OnPropertyChanged(nameof(FirstNameError));
            }
        }

        public string LastNameError
        {
            get { return _lastNameError; }
            set
            {
                _lastNameError = value;
                OnPropertyChanged(nameof(LastNameError));
            }
        }

        public string DepartmentError
        {
            get { return _departmentError; }
            set
            {
                _departmentError = value;
                OnPropertyChanged(nameof(DepartmentError));
            }
        }

        public string EmailError
        {
            get { return _emailError; }
            set
            {
                _emailError = value;
                OnPropertyChanged(nameof(EmailError));
            }
        }

        public string PhoneError
        {
            get { return _phoneError; }
            set
            {
                _phoneError = value;
                OnPropertyChanged(nameof(PhoneError));
            }
        }

        public Visibility FirstNameErrorVisibility
        {
            get { return _firstNameErrorVisibility; }
            set
            {
                _firstNameErrorVisibility = value;
                OnPropertyChanged(nameof(FirstNameErrorVisibility));
            }
        }

        public Visibility LastNameErrorVisibility
        {
            get { return _lastNameErrorVisibility; }
            set
            {
                _lastNameErrorVisibility = value;
                OnPropertyChanged(nameof(LastNameErrorVisibility));
            }
        }

        public Visibility DepartmentErrorVisibility
        {
            get { return _departmentErrorVisibility; }
            set
            {
                _departmentErrorVisibility = value;
                OnPropertyChanged(nameof(DepartmentErrorVisibility));
            }
        }

        public Visibility EmailErrorVisibility
        {
            get { return _emailErrorVisibility; }
            set
            {
                _emailErrorVisibility = value;
                OnPropertyChanged(nameof(EmailErrorVisibility));
            }
        }

        public Visibility PhoneErrorVisibility
        {
            get { return _phoneErrorVisibility; }
            set
            {
                _phoneErrorVisibility = value;
                OnPropertyChanged(nameof(PhoneErrorVisibility));
            }
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        private void LoadEmployeeData()
        {
            FirstName = _existingEmployee.FirstName;
            LastName = _existingEmployee.LastName;
            Department = _existingEmployee.Department;
            Designation = _existingEmployee.Designation;
            JoinDate = _existingEmployee.JoinDate;
            Email = _existingEmployee.Email;
            Phone = _existingEmployee.Phone;

            if (_existingEmployee.Status == "Active")
                IsActive = true;
            else
                IsInactive = true;
        }

        private void ValidateFirstName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                FirstNameError = "First Name is required";
                FirstNameErrorVisibility = Visibility.Visible;
            }
            else
            {
                FirstNameError = string.Empty;
                FirstNameErrorVisibility = Visibility.Collapsed;
            }
        }

        private void ValidateLastName()
        {
            if (string.IsNullOrWhiteSpace(LastName))
            {
                LastNameError = "Last Name is required";
                LastNameErrorVisibility = Visibility.Visible;
            }
            else
            {
                LastNameError = string.Empty;
                LastNameErrorVisibility = Visibility.Collapsed;
            }
        }

        private void ValidateDepartment()
        {
            if (string.IsNullOrWhiteSpace(Department))
            {
                DepartmentError = "Department is required";
                DepartmentErrorVisibility = Visibility.Visible;
            }
            else
            {
                DepartmentError = string.Empty;
                DepartmentErrorVisibility = Visibility.Collapsed;
            }
        }

        private void ValidateEmail()
        {
            if (!string.IsNullOrWhiteSpace(Email))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(Email))
                {
                    EmailError = "Invalid email format";
                    EmailErrorVisibility = Visibility.Visible;
                }
                else
                {
                    EmailError = string.Empty;
                    EmailErrorVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                EmailError = string.Empty;
                EmailErrorVisibility = Visibility.Collapsed;
            }
        }

        private void ValidatePhone()
        {
            if (!string.IsNullOrWhiteSpace(Phone))
            {
                var phoneRegex = new Regex(@"^\+?[\d\s\-()]+$");
                if (!phoneRegex.IsMatch(Phone) || Phone.Length < 10)
                {
                    PhoneError = "Invalid phone number (minimum 10 digits)";
                    PhoneErrorVisibility = Visibility.Visible;
                }
                else
                {
                    PhoneError = string.Empty;
                    PhoneErrorVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                PhoneError = string.Empty;
                PhoneErrorVisibility = Visibility.Collapsed;
            }
        }

        private bool ValidateAll()
        {
            ValidateFirstName();
            ValidateLastName();
            ValidateDepartment();
            ValidateEmail();
            ValidatePhone();

            return FirstNameErrorVisibility == Visibility.Collapsed &&
                   LastNameErrorVisibility == Visibility.Collapsed &&
                   DepartmentErrorVisibility == Visibility.Collapsed &&
                   EmailErrorVisibility == Visibility.Collapsed &&
                   PhoneErrorVisibility == Visibility.Collapsed;
        }

        private bool CanExecuteSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Department);
        }

        private void ExecuteSave(object parameter)
        {
            try
            {
                if (!ValidateAll())
                {
                    MessageBox.Show("Please fix all validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var employee = new Employee
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Department = Department,
                    Designation = Designation,
                    JoinDate = JoinDate,
                    Status = IsActive ? "Active" : "Inactive",
                    Email = Email,
                    Phone = Phone
                };

                if (_existingEmployee == null)
                {
                    // Add new employee
                    _employeeManagementService.AddEmployee(employee);
                }
                else
                {
                    // Update existing employee
                    employee.EmployeeId = _existingEmployee.EmployeeId;
                    _employeeManagementService.UpdateEmployee(employee);
                }

                DialogResult = true;
                _closeAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel(object parameter)
        {
            DialogResult = false;
            _closeAction?.Invoke();
        }

        #endregion
    }
}