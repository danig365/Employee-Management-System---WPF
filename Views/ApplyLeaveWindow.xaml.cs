using System.Windows;

namespace Employee_Management_System.Views
{
    public partial class ApplyLeaveWindow : Window
    {
        public ApplyLeaveWindow()
        {
            InitializeComponent();

            // ✅ FIX: Create and set DataContext BEFORE checking
            var viewModel = new ViewModels.ApplyLeaveViewModel();
            DataContext = viewModel;
            viewModel.CloseAction = () => DialogResult = true;
        }
    }
}