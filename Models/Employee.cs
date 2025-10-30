using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Management_System.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }   
        public string LastName { get; set; }
        public string Department    { get; set; }
        public string Designation  { get; set; }
        public DateTime? JoinDate { get; set; }
        public string Status { get; set; } // Active, Inactive
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
