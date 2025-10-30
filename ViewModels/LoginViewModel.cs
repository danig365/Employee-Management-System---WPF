using Azure.Core;
using Employee_Management_System.Commands;
using Employee_Management_System.Models;
using Employee_Management_System.Services;
using System.Windows;
using System.Windows.Input;
namespace Employee_Management_System.ViewModels
{
    public class LoginViewModel:BaseViewModel
    {
		private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isLoggingIn;
        private bool _isErrorVisible;
        private bool _rememberMe;
		private readonly AuthenticationService _authService;

        public LoginViewModel()
        {
			_authService = new AuthenticationService();
			LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }
        public string Username
		{
			get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
                // Also notify LoginCommand to update CanExecute
                CommandManager.InvalidateRequerySuggested();
            }
        }

		public string Password
		{
			get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                // Also notify LoginCommand to update CanExecute
                CommandManager.InvalidateRequerySuggested();
            }
        }

		public string ErrorMessage
		{
			get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                // Also notify LoginCommand to update CanExecute
                CommandManager.InvalidateRequerySuggested();
            }
        }

		public bool IsLoggingIn
		{
			get { return _isLoggingIn; }
            set
            {
                _isLoggingIn = value;
                OnPropertyChanged(nameof(IsLoggingIn));
                // Also notify LoginCommand to update CanExecute
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsErrorVisible
        {
            get { return _isErrorVisible; }
            set
            {
                if (_isErrorVisible != value)
                {
                    _isErrorVisible = value;
                    OnPropertyChanged(nameof(IsErrorVisible));
                }
            }
        }

        public bool RememberMe
        {
            get { return _rememberMe; }
            set
            {
                if (_rememberMe != value)
                {
                    _rememberMe = value;
                    OnPropertyChanged(nameof(RememberMe));
                }
            }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));
                }
            }
        }

        public string LoginButtonContent => IsLoggingIn ? "Logging in..." : "LOGIN";
		public ICommand LoginCommand { get; }
		public bool CanExecuteLogin(object parameter)
		{
			if (IsLoggingIn)
			{
				return false;
			}
			if(Username == null || Password == null)
			{
				return false;
            }
			return true;
        }
		public void ExecuteLogin(object parameter)
		{
			IsErrorVisible = false;
			ErrorMessage = string.Empty;
			if (!ValidateInput())
			{
				return;

			}
			_isLoggingIn = true;
			var user = _authService.ValidateUser(Username.Trim(), Password);

			_isLoggingIn = false;
            if (user != null)
            {
                CurrentUser = user;
                SessionManager.CurrentUser = user;
                // 1. OPEN NEW WINDOW FIRST
                if (CurrentUser.UserRole == "Admin")
                {
                    var adminDashboard = new Views.AdminDashboard();
                    adminDashboard.Show();
                }
                else if (CurrentUser.UserRole == "Employee")
                {
                    var userDashboard = new Views.EmployeeProfile();
                    userDashboard.Show();
                }

                // 2. THEN CLOSE LOGIN WINDOW
                // NEW (works):
                Application.Current.Windows.OfType<Views.LoginWindow>()
                    .FirstOrDefault()?.Close();
            }
            else
			{
                ShowError("Invalid username or password.");
			}
		}
		public bool ValidateInput()
		{
			if(string.IsNullOrWhiteSpace(Username))
			{
				ErrorMessage= "Username is required.";
				IsErrorVisible= true;
				return false;
            }
			if (string.IsNullOrWhiteSpace(Password))
			{
				ErrorMessage = "Password is required.";
				IsErrorVisible = true;
				return false;
            }
            // Check username length
            if (Username.Trim().Length < 3)
            {
                ShowError("Username must be at least 3 characters long.");
                return false;
            }

            // Check password length
            if (Password.Length < 3)
            {
                ShowError("Password must be at least 3 characters long.");
                return false;
            }

            return true;
        }
		public void ShowError(string message)
		{
			ErrorMessage = message;
			IsErrorVisible = true;
        }
    }
}
