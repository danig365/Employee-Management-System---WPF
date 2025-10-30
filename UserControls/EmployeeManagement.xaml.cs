using Employee_Management_System.ViewModels;
using System.Windows.Controls;

namespace Employee_Management_System.UserControls
{
    public partial class EmployeeManagement : UserControl
    {
        public EmployeeManagement()
        {
            InitializeComponent();
            DataContext = new EmployeeManagementViewModel();
        }
    }
}