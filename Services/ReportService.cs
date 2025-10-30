using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Employee_Management_System.Models;
using Microsoft.Data.SqlClient;

namespace Employee_Management_System.Services
{
    public class ReportService
    {
        private readonly DatabaseService _dbService;

        public ReportService()
        {
            _dbService = new DatabaseService();
        }

        public List<MonthlyAttendanceReport> GetMonthlyAttendanceReport(int month, int year, int? employeeId = null)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Month", month),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@EmployeeId", employeeId ?? (object)DBNull.Value)
                };

                var result = _dbService.ExecuteStoredProcedure("sp_GetMonthlyAttendanceReport", parameters);
                return ConvertToMonthlyReport(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMonthlyAttendanceReport Error: {ex.Message}");
                return new List<MonthlyAttendanceReport>();
            }
        }

        public List<LeaveSummaryReport> GetLeaveSummaryByDept(DateTime? startDate = null, DateTime? endDate = null, string department = null)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@StartDate", startDate ?? (object)DBNull.Value),
                    new SqlParameter("@EndDate", endDate ?? (object)DBNull.Value),
                    new SqlParameter("@Department", department ?? (object)DBNull.Value)
                };

                var result = _dbService.ExecuteStoredProcedure("sp_GetLeaveSummaryByDept", parameters);
                return ConvertToLeaveSummary(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetLeaveSummaryByDept Error: {ex.Message}");
                return new List<LeaveSummaryReport>();
            }
        }

        private List<MonthlyAttendanceReport> ConvertToMonthlyReport(DataTable dt)
        {
            return dt.AsEnumerable().Select(row => new MonthlyAttendanceReport
            {
                EmployeeId = Convert.ToInt32(row["EmployeeId"]),
                EmployeeName = row["EmployeeName"].ToString(),
                Department = row["Department"].ToString(),
                Designation = row["Designation"].ToString(),
                TotalPresentDays = Convert.ToInt32(row["TotalPresentDays"]),
                TotalAbsentDays = Convert.ToInt32(row["TotalAbsentDays"]),
                TotalHalfDays = Convert.ToInt32(row["TotalHalfDays"]),
                LateCheckIns = Convert.ToInt32(row["LateCheckIns"]),
                AverageWorkingHours = Convert.ToDecimal(row["AverageWorkingHours"]),
                TotalRecords = Convert.ToInt32(row["TotalRecords"])
            }).ToList();
        }

        private List<LeaveSummaryReport> ConvertToLeaveSummary(DataTable dt)
        {
            return dt.AsEnumerable().Select(row => new LeaveSummaryReport
            {
                Department = row["Department"].ToString(),
                TotalLeaveRequests = Convert.ToInt32(row["TotalLeaveRequests"]),
                ApprovedLeaves = Convert.ToInt32(row["ApprovedLeaves"]),
                RejectedLeaves = Convert.ToInt32(row["RejectedLeaves"]),
                PendingLeaves = Convert.ToInt32(row["PendingLeaves"]),
                MostCommonLeaveType = row["MostCommonLeaveType"]?.ToString() ?? "N/A",
                TotalLeaveDays = Convert.ToInt32(row["TotalLeaveDays"])
            }).ToList();
        }
    }
}