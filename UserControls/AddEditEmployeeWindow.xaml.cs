using Employee_Management_System.Models;
using System.Windows;

namespace Employee_Management_System.UserControls
{
    public partial class AddEditEmployeeWindow : Window
    {
        private readonly AddEditEmployeeViewModel _viewModel;

        public AddEditEmployeeWindow(Employee employee)
        {
            InitializeComponent();
            _viewModel = new AddEditEmployeeViewModel(employee, Close);
            DataContext = _viewModel;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = _viewModel.DialogResult;
            base.OnClosing(e);
        }
    }
}