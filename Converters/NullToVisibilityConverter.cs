using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Employee_Management_System.Converters
{
    /// <summary>
    /// Converts null/non-null values to Visibility
    /// If value is null → Collapsed
    /// If value is not null → Visible
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        // This method is called when data flows from ViewModel to View
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If value is not null, show the control (Visible)
            // If value is null, hide the control (Collapsed)
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        // This method is for two-way binding (not needed here)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}