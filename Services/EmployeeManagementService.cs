using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Employee_Management_System.Models;
using Microsoft.Data.SqlClient;

namespace Employee_Management_System.Services
{
    public class EmployeeManagementService
    {
        private readonly DatabaseService _databaseService;

        public EmployeeManagementService()
         {
              _databaseService = new DatabaseService();
        }
        public List<Employee> GetAllEmployees()
        {
            try
            {
                var dataTable = _databaseService.ExecuteStoredProcedure("sp_GetAllEmployees", null);
                var employees = new List<Employee>();
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var employee = new Employee
                    {
                        EmployeeId = Convert.ToInt32(row["EmployeeId"]),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        Department = row["Department"].ToString(),
                        Designation = row["Designation"].ToString(),
                        JoinDate = row["JoinDate"] != DBNull.Value ? Convert.ToDateTime(row["JoinDate"]) : (DateTime?)null,
                        Status = row["Status"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Phone"].ToString()
                    };
                    employees.Add(employee);
                }
                return employees;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving employees: {ex.Message}", ex);
            }
        }
        public Employee GetEmployeeById(int employeeId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employeeId)
                };
                var dataTable = _databaseService.ExecuteStoredProcedure("sp_GetEmployeeById", parameters);
                if (dataTable.Rows.Count == 1)
                {
                    var row = dataTable.Rows[0];
                    var employee = new Employee
                    {
                        EmployeeId = Convert.ToInt32(row["EmployeeId"]),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        Department = row["Department"].ToString(),
                        Designation = row["Designation"].ToString(),
                        JoinDate = row["JoinDate"] != DBNull.Value ? Convert.ToDateTime(row["JoinDate"]) : (DateTime?)null,
                        Status = row["Status"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Phone"].ToString()
                    };
                    return employee;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving employee by ID: {ex.Message}", ex);
            }
        }
        public void AddEmployee(Employee employee)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@FirstName", employee.FirstName),
                    new SqlParameter("@LastName", employee.LastName),
                    new SqlParameter("@Department", employee.Department),
                    new SqlParameter("@Designation", employee.Designation),
                    new SqlParameter("@JoinDate", (object)employee.JoinDate ?? DBNull.Value),
                    new SqlParameter("@Status", employee.Status),
                    new SqlParameter("@Email", employee.Email),
                    new SqlParameter("@Phone", employee.Phone)
                };
                _databaseService.ExecuteStoredProcedure("sp_AddEmployee", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding employee: {ex.Message}", ex);
            }
        }
        public void AddUser(int employeeId, string username, string password, string role)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@UserRole", role)
                };
                _databaseService.ExecuteStoredProcedure("sp_AddUser", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding user for employee: {ex.Message}", ex);
            }
        }
        public void DeleteEmployee(Employee employee) {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employee.EmployeeId)
                };
                _databaseService.ExecuteStoredProcedure("sp_DeleteEmployee", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting employee: {ex.Message}", ex);
            }
        }
        public void UpdateEmployee(Employee employee)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employee.EmployeeId),
                    new SqlParameter("@FirstName", employee.FirstName),
                    new SqlParameter("@LastName", employee.LastName),
                    new SqlParameter("@Department", employee.Department),
                    new SqlParameter("@Designation", employee.Designation),
                    new SqlParameter("@JoinDate", (object)employee.JoinDate ?? DBNull.Value),
                    new SqlParameter("@Status", employee.Status),
                    new SqlParameter("@Email", employee.Email),
                    new SqlParameter("@Phone", employee.Phone)
                };
                _databaseService.ExecuteStoredProcedure("sp_UpdateEmployee", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating employee: {ex.Message}", ex);
            }
        }
       public Employee SearchEmployees (string keyword)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Keyword", keyword)
                };
                var dataTable = _databaseService.ExecuteStoredProcedure("sp_SearchEmployees", parameters);
                if (dataTable.Rows.Count >= 1)
                {
                    var row = dataTable.Rows[0];
                    var employee = new Employee
                    {
                        EmployeeId = Convert.ToInt32(row["EmployeeId"]),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        Department = row["Department"].ToString(),
                        Designation = row["Designation"].ToString(),
                        JoinDate = row["JoinDate"] != DBNull.Value ? Convert.ToDateTime(row["JoinDate"]) : (DateTime?)null,
                        Status = row["Status"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Phone"].ToString()
                    };
                    return employee;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching employees: {ex.Message}", ex);
            }
        }
       public Employee GetDepartments ()
        {
            try
            {
                var dataTable = _databaseService.ExecuteStoredProcedure("sp_GetDepartments", null);
                if (dataTable.Rows.Count >= 1)
                {
                    var row = dataTable.Rows[0];
                    var employee = new Employee
                    {
                        Department = row["Department"].ToString()
                    };
                    return employee;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving departments: {ex.Message}", ex);
            }
        }
    }
}
