

using Employee_Management_System.Models;
using Microsoft.Data.SqlClient;

namespace Employee_Management_System.Services
{
    public class AuthenticationService
    {
        private static User _currentUser;
        private readonly DatabaseService _databaseService;
        public AuthenticationService()
        {
            _databaseService = new DatabaseService();
        }   
        public User CurrentUser
        {
            get { return _currentUser; }
        }
        public User ValidateUser(string username, string password)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", password)
                };  
                var dataTable = _databaseService.ExecuteStoredProcedure("sp_ValidateUser", parameters);
                if (dataTable.Rows.Count == 1)
                {
                    var row = dataTable.Rows[0];
                    _currentUser = new User
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        UserName = row["UserName"].ToString(),
                        //Password = row["Password"].ToString(),
                        UserRole = row["UserRole"].ToString(),
                        EmployeeId = row["EmployeeId"] != DBNull.Value ? Convert.ToInt32(row["EmployeeId"]) : null,
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    };
                    return _currentUser;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating user: {ex.Message}", ex);
            }
        }
        public User GetUserDetails(int userId) {             try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId)
                };
                var dataTable = _databaseService.ExecuteStoredProcedure("sp_GetUserDetails", parameters);
                if (dataTable.Rows.Count == 1)
                {
                    var row = dataTable.Rows[0];
                    return new User
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        UserName = row["UserName"].ToString(),
                        Password = row["Password"].ToString(),
                        UserRole = row["UserRole"].ToString(),
                        EmployeeId = row["EmployeeId"] != DBNull.Value ? Convert.ToInt32(row["EmployeeId"]) : null,
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user details: {ex.Message}", ex);
            }
        }
        public User GetCurrentUser()
        {
                       return _currentUser;
        }
        public int Logout()
        {
            _currentUser = null;
            return 0;
        }   
        public bool IsUserLoggedIn()
        {
            return _currentUser != null;
        }   
        public bool HasRole(string role)
        {
            return _currentUser != null && _currentUser.UserRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }
        public bool isAdmin()
        {
            return HasRole("Admin");
        }
        public bool isEmployee()
        {
            return HasRole("Employee");
        }   

    }
}
