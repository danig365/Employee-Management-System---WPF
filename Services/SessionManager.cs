using Employee_Management_System.Models;

namespace Employee_Management_System.Services
{
    /// <summary>
    /// Manages the current user session
    /// </summary>
    public static class SessionManager
    {
        private static User _currentUser;

        public static User CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        public static int CurrentUserId => _currentUser?.UserId ?? 0;

        public static int CurrentEmployeeId => _currentUser?.EmployeeId ?? 0;

        public static string CurrentUserRole => _currentUser?.UserRole ?? "Employee";

        public static bool IsAdmin => CurrentUserRole == "Admin";

        public static bool IsEmployee => CurrentUserRole == "Employee";

        public static void ClearSession()
        {
            _currentUser = null;
        }

    }
}