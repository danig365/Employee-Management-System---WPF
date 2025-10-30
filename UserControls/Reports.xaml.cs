using System.Windows.Controls;

namespace Employee_Management_System.UserControls
{
    public partial class Reports : UserControl
    {
        public Reports()
        {
            InitializeComponent();
            DataContext = new ViewModels.ReportsViewModel();
        }
    }
}