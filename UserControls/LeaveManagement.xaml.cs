using System.Windows.Controls;

namespace Employee_Management_System.UserControls
{
    public partial class LeaveManagement : UserControl
    {
        public LeaveManagement()
        {
            InitializeComponent();
            DataContext = new ViewModels.LeaveManagementViewModel();
        }
    }
}