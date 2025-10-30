using Employee_Management_System.ViewModels;
using System.Windows.Controls;

namespace Employee_Management_System.UserControls
{
    /// <summary>
    /// Interaction logic for AttendanceManagement.xaml
    /// </summary>
    public partial class AttendanceManagement : UserControl
    {
        public AttendanceManagement()
        {
            InitializeComponent();
            DataContext = new AttendanceManagementViewModel();
        }
    }
}