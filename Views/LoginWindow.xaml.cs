using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Employee_Management_System.ViewModels;

namespace Employee_Management_System.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel? ViewModel => DataContext as LoginViewModel;

        public LoginWindow()
        {
            InitializeComponent();

            // Set DataContext if not set in XAML
            DataContext = new LoginViewModel();

            // Set focus to username on load
            Loaded += (s, e) => txtUsername.Focus();

            // Handle Enter key navigation
            txtUsername.KeyDown += TxtUsername_KeyDown;
            txtPassword.KeyDown += TxtPassword_KeyDown;
        }

        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPassword.Focus();
                e.Handled = true; // Prevent beep sound
            }
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ViewModel?.LoginCommand.CanExecute(null) == true)
                {
                    ViewModel.LoginCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }
    }
}