using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Management_System.Models
{
    public class User
    {
        // Primary Key
        public int UserId { get; set; }

        // Username (unique, required)
        public string UserName { get; set; }

        // Hashed password
        public string Password { get; set; }

        // User Role: Admin or Employee
        public string UserRole { get; set; }

        // Foreign key (nullable for Admins)
        public int? EmployeeId { get; set; }

        // Whether the account is active
        public bool IsActive { get; set; }
        public Employee Employee { get; set; }
    }

}
