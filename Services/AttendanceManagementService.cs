using Employee_Management_System.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Employee_Management_System.Services
{
    public class AttendanceManagementService
    {
        private readonly DatabaseService _databaseService;

        public AttendanceManagementService()
        {
            _databaseService = new DatabaseService();
        }

        public bool MarkCheckIn(int employeeId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employeeId)
                };

                _databaseService.ExecuteStoredProcedure("sp_MarkCheckIn", parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during check-in: {ex.Message}", ex);
            }
        }

        public bool MarkCheckOut(int employeeId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employeeId)
                };

                _databaseService.ExecuteStoredProcedure("sp_MarkCheckOut", parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during check-out: {ex.Message}", ex);
            }
        }

        public bool ManualEntry(int employeeId, DateTime attendanceDate, DateTime? checkIn, DateTime? checkOut, string status)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@AttendanceDate", attendanceDate),
                    new SqlParameter("@CheckIn", (object?)checkIn ?? DBNull.Value),
                    new SqlParameter("@CheckOut", (object?)checkOut ?? DBNull.Value),
                    new SqlParameter("@Status", status)
                };

                _databaseService.ExecuteStoredProcedure("sp_ManualEntry", parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during manual entry: {ex.Message}", ex);
            }
        }

        public List<Attendance> GetAttendanceReport(int? employeeId, DateTime startDate, DateTime endDate, string department = null)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@EmployeeId", (object?)employeeId ?? DBNull.Value),
                    new SqlParameter("@Department", (object?)department ?? DBNull.Value)
                };

                var dataTable = _databaseService.ExecuteStoredProcedure("sp_GetAttendanceReport", parameters);
                return ConvertDataTableToList(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving attendance report: {ex.Message}", ex);
            }
        }

        public List<Attendance> GetMyAttendance(int employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var dataTable = _databaseService.ExecuteStoredProcedure("sp_GetMyAttendance", parameters);
                return ConvertDataTableToList(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving employee attendance: {ex.Message}", ex);
            }
        }

        public decimal CalculateHours(DateTime? checkIn, DateTime? checkOut)
        {
            try
            {
                if (checkIn == null || checkOut == null)
                    return 0;

                var totalMinutes = (checkOut.Value - checkIn.Value).TotalMinutes;
                return (decimal)Math.Round(totalMinutes / 60.0, 2);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating worked hours: {ex.Message}", ex);
            }
        }

        public Attendance GetEmployeeAttendanceForToday(int employeeId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@AttendanceDate", DateTime.Today)
                };

                var dataTable = _databaseService.ExecuteStoredProcedure("sp_GetTodayAttendance", parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return MapRowToAttendance(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving today's attendance: {ex.Message}", ex);
            }
        }

        // Helper method to convert DataTable to List<Attendance>
        private List<Attendance> ConvertDataTableToList(DataTable dataTable)
        {
            var attendanceList = new List<Attendance>();

            foreach (DataRow row in dataTable.Rows)
            {
                attendanceList.Add(MapRowToAttendance(row));
            }

            return attendanceList;
        }

        // Helper method to map DataRow to Attendance object
        private Attendance MapRowToAttendance(DataRow row)
        {
            return new Attendance
            {
                AttendanceId = row["AttendanceId"] != DBNull.Value ? Convert.ToInt32(row["AttendanceId"]) : 0,
                EmployeeId = row["EmployeeId"] != DBNull.Value ? Convert.ToInt32(row["EmployeeId"]) : 0,
                EmployeeName = row["EmployeeName"]?.ToString() ?? "",
                AttendanceDate = row["AttendanceDate"] != DBNull.Value ? Convert.ToDateTime(row["AttendanceDate"]) : DateTime.MinValue,
                CheckIn = row["CheckIn"] != DBNull.Value ? Convert.ToDateTime(row["CheckIn"]) : (DateTime?)null,
                CheckOut = row["CheckOut"] != DBNull.Value ? Convert.ToDateTime(row["CheckOut"]) : (DateTime?)null,
                TotalHours = row["TotalHours"] != DBNull.Value ? Convert.ToDecimal(row["TotalHours"]) : 0,
                Status = row["Status"]?.ToString() ?? "Absent"
            };
        }
    }
}