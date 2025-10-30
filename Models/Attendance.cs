using System;
using System.ComponentModel;

namespace Employee_Management_System.Models
{
    public class Attendance : INotifyPropertyChanged
    {
        private int _attendanceId;
        private int _employeeId;
        private string _employeeName;
        private DateTime _attendanceDate;
        private DateTime? _checkIn;
        private DateTime? _checkOut;
        private decimal _totalHours;
        private string _status;

        public int AttendanceId
        {
            get => _attendanceId;
            set
            {
                _attendanceId = value;
                OnPropertyChanged(nameof(AttendanceId));
            }
        }

        public int EmployeeId
        {
            get => _employeeId;
            set
            {
                _employeeId = value;
                OnPropertyChanged(nameof(EmployeeId));
            }
        }

        public string EmployeeName
        {
            get => _employeeName;
            set
            {
                _employeeName = value;
                OnPropertyChanged(nameof(EmployeeName));
            }
        }

        public DateTime AttendanceDate
        {
            get => _attendanceDate;
            set
            {
                _attendanceDate = value;
                OnPropertyChanged(nameof(AttendanceDate));
            }
        }

        public DateTime? CheckIn
        {
            get => _checkIn;
            set
            {
                _checkIn = value;
                OnPropertyChanged(nameof(CheckIn));
                OnPropertyChanged(nameof(CheckInDisplay));
            }
        }

        public DateTime? CheckOut
        {
            get => _checkOut;
            set
            {
                _checkOut = value;
                OnPropertyChanged(nameof(CheckOut));
                OnPropertyChanged(nameof(CheckOutDisplay));
            }
        }

        public decimal TotalHours
        {
            get => _totalHours;
            set
            {
                _totalHours = value;
                OnPropertyChanged(nameof(TotalHours));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusSymbol));
            }
        }

        // Display properties for UI
        public string CheckInDisplay => CheckIn?.ToString("hh:mm tt") ?? "--:--";
        public string CheckOutDisplay => CheckOut?.ToString("hh:mm tt") ?? "--:--";

        public string StatusSymbol
        {
            get
            {
                return Status switch
                {
                    "Present" => "✓",
                    "Absent" => "✗",
                    "Half-day" => "◐",
                    _ => "-"
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}