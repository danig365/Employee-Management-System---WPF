using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Employee_Management_System.Models;
using Microsoft.Data.SqlClient;

namespace Employee_Management_System.Services
{
    public class LeaveService
    {
        private readonly DatabaseService _dbService;

        public LeaveService()
        {
            _dbService = new DatabaseService();
        }

        public bool ApplyLeave(LeaveRequest leaveRequest)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@EmployeeID", leaveRequest.EmployeeId),
                    new SqlParameter("@LeaveType", leaveRequest.LeaveType),
                    new SqlParameter("@StartDate", leaveRequest.StartDate),
                    new SqlParameter("@EndDate", leaveRequest.EndDate),
                    new SqlParameter("@Remarks", leaveRequest.Remarks ?? (object)DBNull.Value)
                };

                _dbService.ExecuteNonQuery("sp_AddLeaveRequest", parameters);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyLeave Error: {ex.Message}");
                return false;
            }
        }

        public bool CheckOverlap(int employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@EmployeeID", employeeId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var result = _dbService.ExecuteStoredProcedure("sp_CheckLeaveOverlap", parameters);
                if (result.Rows.Count > 0)
                {
                    return Convert.ToInt32(result.Rows[0][0]) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckOverlap Error: {ex.Message}");
                return false;
            }
        }

        public List<LeaveRequest> GetPendingLeaves()
        {
            try
            {
                var result = _dbService.ExecuteStoredProcedure("sp_GetPendingLeaveRequests");
                return ConvertDataTableToList(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetPendingLeaves Error: {ex.Message}");
                return new List<LeaveRequest>();
            }
        }

        public List<LeaveRequest> GetAllLeaveRequests(string status = null, int? employeeId = null)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Status", status ?? (object)DBNull.Value),
                    new SqlParameter("@EmployeeID", employeeId ?? (object)DBNull.Value)
                };

                var result = _dbService.ExecuteStoredProcedure("sp_GetAllLeaveRequests", parameters);
                var leaveList = ConvertDataTableToList(result);

                Debug.WriteLine($"GetAllLeaveRequests returned {leaveList.Count} records");
                return leaveList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetAllLeaveRequests Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<LeaveRequest>();
            }
        }

        public List<LeaveRequest> GetLeaveHistory(int employeeId)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@EmployeeID", employeeId)
                };

                var result = _dbService.ExecuteStoredProcedure("sp_GetLeaveHistory", parameters);
                return ConvertDataTableToList(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetLeaveHistory Error: {ex.Message}");
                return new List<LeaveRequest>();
            }
        }

        public bool ApproveLeave(int leaveId, int approvedBy, string adminRemarks = null)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@LeaveID", leaveId),
                    new SqlParameter("@ApprovedBy", approvedBy),
                    new SqlParameter("@AdminRemarks", adminRemarks ?? (object)DBNull.Value)
                };

                _dbService.ExecuteNonQuery("sp_ApproveLeave", parameters);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApproveLeave Error: {ex.Message}");
                return false;
            }
        }

        public bool RejectLeave(int leaveId, int rejectedBy, string reason)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@LeaveID", leaveId),
                    new SqlParameter("@RejectedBy", rejectedBy),
                    new SqlParameter("@Reason", reason ?? (object)DBNull.Value)
                };

                _dbService.ExecuteNonQuery("sp_RejectLeave", parameters);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RejectLeave Error: {ex.Message}");
                return false;
            }
        }

        public int CalculateDuration(DateTime startDate, DateTime endDate)
        {
            return (int)(endDate - startDate).TotalDays + 1;
        }

        public int GetLeaveBalance(int employeeId, string leaveType)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@EmployeeID", employeeId),
                    new SqlParameter("@LeaveType", leaveType)
                };

                var result = _dbService.ExecuteStoredProcedure("sp_GetLeaveBalance", parameters);
                if (result.Rows.Count > 0 && result.Rows[0][0] != DBNull.Value)
                {
                    return Convert.ToInt32(result.Rows[0][0]);
                }

                // If no balance found, initialize it
                InitializeLeaveBalance(employeeId, leaveType);
                return GetDefaultLeaveBalance(leaveType);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetLeaveBalance Error: {ex.Message}");
                return 0;
            }
        }

        private void InitializeLeaveBalance(int employeeId, string leaveType)
        {
            try
            {
                int defaultBalance = GetDefaultLeaveBalance(leaveType);
                var query = @"
                    IF NOT EXISTS (SELECT 1 FROM LeaveBalance WHERE EmployeeId = @EmployeeId AND LeaveType = @LeaveType)
                    BEGIN
                        INSERT INTO LeaveBalance (EmployeeId, LeaveType, TotalLeaves, UsedLeaves)
                        VALUES (@EmployeeId, @LeaveType, @TotalLeaves, 0)
                    END";

                var parameters = new[]
                {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@LeaveType", leaveType),
                    new SqlParameter("@TotalLeaves", defaultBalance)
                };

                _dbService.ExecuteQuery(query, parameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeLeaveBalance Error: {ex.Message}");
            }
        }

        private int GetDefaultLeaveBalance(string leaveType)
        {
            return leaveType switch
            {
                "Annual Leave" => 10,
                "Sick Leave" => 5,
                "Casual Leave" => 7,
                _ => 0
            };
        }

        // 🔧 FIXED: Changed 'Employee' to 'Employees' (correct table name)
        public List<Employee> GetAllEmployees()
        {
            try
            {
                // FIXED: Employees (plural) instead of Employee (singular)
                var query = "SELECT EmployeeId, FirstName, LastName FROM Employees WHERE Status = 'Active'";
                var result = _dbService.ExecuteQuery(query);

                var employees = result.AsEnumerable().Select(row => new Employee
                {
                    EmployeeId = Convert.ToInt32(row["EmployeeId"]),
                    FirstName = row["FirstName"].ToString(),
                    LastName = row["LastName"].ToString()
                }).ToList();

                Debug.WriteLine($"GetAllEmployees returned {employees.Count} employees");
                return employees;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetAllEmployees Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<Employee>();
            }
        }

        private List<LeaveRequest> ConvertDataTableToList(DataTable dt)
        {
            var leaveRequests = new List<LeaveRequest>();

            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    leaveRequests.Add(new LeaveRequest
                    {
                        LeaveId = Convert.ToInt32(row["LeaveId"]),
                        EmployeeId = Convert.ToInt32(row["EmployeeId"]),
                        EmployeeName = row["EmployeeName"].ToString(),
                        LeaveType = row["LeaveType"].ToString(),
                        StartDate = Convert.ToDateTime(row["StartDate"]),
                        EndDate = Convert.ToDateTime(row["EndDate"]),
                        Duration = Convert.ToInt32(row["Duration"]),
                        Status = row["Status"].ToString(),
                        Remarks = row["Remarks"]?.ToString(),
                        RequestDate = Convert.ToDateTime(row["RequestDate"]),
                        ApprovedBy = row["ApprovedBy"] != DBNull.Value ? Convert.ToInt32(row["ApprovedBy"]) : (int?)null,
                        ApprovedByName = row["ApprovedByName"]?.ToString(),
                        AdminRemarks = row["AdminRemarks"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConvertDataTableToList Error: {ex.Message}");
            }

            return leaveRequests;
        }
    }
}