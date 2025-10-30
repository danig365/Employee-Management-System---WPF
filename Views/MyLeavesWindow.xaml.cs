using System.Windows;
using System.Windows.Controls;
using Employee_Management_System.ViewModels;

namespace Employee_Management_System.Views
{
    public partial class MyLeavesWindow : Window
    {
        public MyLeavesWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MyLeavesViewModel viewModel)
            {
                viewModel.FilterLeaves();
            }
        }
    }
}