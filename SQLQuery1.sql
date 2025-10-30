-- ============================================
-- Employees Table
-- ============================================
CREATE TABLE Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL, 
    LastName NVARCHAR(50) NOT NULL,
    Department NVARCHAR(50) NOT NULL,
    Designation NVARCHAR(50) NOT NULL,
    JoinDate DATE,
    Status NVARCHAR(50) CHECK (Status IN ('Active', 'Inactive')),
    Email NVARCHAR(50),
    Phone NVARCHAR(50)
);
GO

-- ============================================
-- Users Table
-- ============================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(50) NOT NULL UNIQUE, 
    Password NVARCHAR(255) NOT NULL,   -- Increased length for hashed password
    UserRole NVARCHAR(50) CHECK (UserRole IN ('Admin','Employee')),
    EmployeeId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1, 

    CONSTRAINT FK_Users_Employees
        FOREIGN KEY (EmployeeId)
        REFERENCES Employees(EmployeeId)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);
GO

-- ============================================
-- Attendance Table
-- ============================================
CREATE TABLE Attendance (
    AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    AttendanceDate DATE NOT NULL,
    CheckIn TIME,
    CheckOut TIME,
    Status NVARCHAR(20) CHECK (Status IN ('Present', 'Absent', 'Half-day')),

    CONSTRAINT FK_Attendance_Employees
        FOREIGN KEY (EmployeeId)
        REFERENCES Employees(EmployeeId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
GO

-- ============================================
-- LeaveRequests Table
-- ============================================
CREATE TABLE LeaveRequests (
    LeaveId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    LeaveType NVARCHAR(50) CHECK (LeaveType IN ('Sick Leave', 'Casual Leave', 'Annual Leave')),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    Remarks NVARCHAR(500),

    CONSTRAINT FK_LeaveRequests_Employees
        FOREIGN KEY (EmployeeId)
        REFERENCES Employees(EmployeeId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
GO


-- ============================================
-- Stored Procedure: Validate User
-- ============================================
CREATE OR ALTER PROCEDURE sp_ValidateUser
    @Username NVARCHAR(50),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.UserName,
        u.UserRole,
        u.EmployeeId,
        u.IsActive,
        e.FirstName,
        e.LastName,
        e.Email
    FROM Users u
    LEFT JOIN Employees e ON u.EmployeeId = e.EmployeeId
    WHERE u.UserName = @Username 
        AND u.Password = @Password 
        AND u.IsActive = 1;
END
GO

-- ============================================
-- Stored Procedure: Get User Details
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetUserDetails
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.UserName,
        u.UserRole,
        u.EmployeeId,
        u.IsActive,
        e.FirstName,
        e.LastName,
        e.Email,
        e.Department,
        e.Designation
    FROM Users u
    LEFT JOIN Employees e ON u.EmployeeId = e.EmployeeId
    WHERE u.UserId = @UserId AND u.IsActive = 1;
END
GO

-- ============================================
-- Insert Sample Data for Testing
-- ============================================

-- Insert Sample Employees
INSERT INTO Employees (FirstName, LastName, Department, Designation, JoinDate, Status, Email, Phone)
VALUES 
    ('John', 'Admin', 'IT', 'System Administrator', '2024-01-01', 'Active', 'admin@company.com', '1234567890'),
    ('Jane', 'Smith', 'HR', 'HR Manager', '2024-01-15', 'Active', 'jane.smith@company.com', '0987654321');

-- Insert Sample Users
-- Password: "admin123" and "emp123" (You should hash these in production)
INSERT INTO Users (UserName, Password, UserRole, EmployeeId, IsActive)
VALUES 
    ('admin', 'admin123', 'Admin', 1, 1),
    ('employee', 'emp123', 'Employee', 2, 1);
GO

-- ============================================
-- Stored Procedure: sp_AddEmployee
-- ============================================
CREATE OR ALTER PROCEDURE sp_AddEmployee
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Department NVARCHAR(50),
    @Designation NVARCHAR(50),
    @JoinDate DATE,
    @Status NVARCHAR(50),
    @Email NVARCHAR(50),
    @Phone NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Employees (FirstName, LastName, Department, Designation, JoinDate, Status, Email, Phone)
    VALUES (@FirstName, @LastName, @Department, @Designation, @JoinDate, @Status, @Email, @Phone);
END;
GO

-- ============================================
-- Stored Procedure: sp_AddUser
-- ============================================

CREATE OR ALTER PROCEDURE sp_AddUser
    @UserName NVARCHAR(50),
    @Password NVARCHAR(255),
    @UserRole NVARCHAR(50),
    @EmployeeId INT = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validate UserRole
        IF @UserRole NOT IN ('Admin', 'Employee')
        BEGIN
            RAISERROR('Invalid UserRole. Allowed values are Admin or Employee.', 16, 1);
            RETURN;
        END

        -- Check if username already exists
        IF EXISTS (SELECT 1 FROM Users WHERE UserName = @UserName)
        BEGIN
            RAISERROR('Username already exists. Please choose a different username.', 16, 1);
            RETURN;
        END

        -- Validate EmployeeId (if provided)
        IF @EmployeeId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeId = @EmployeeId)
        BEGIN
            RAISERROR('Invalid EmployeeId. Employee does not exist.', 16, 1);
            RETURN;
        END

        -- Insert new user record
        INSERT INTO Users (UserName, Password, UserRole, EmployeeId, IsActive)
        VALUES (@UserName, @Password, @UserRole, @EmployeeId, @IsActive);

        PRINT 'User added successfully.';
    END TRY

    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- ============================================
-- Stored Procedure: sp_UpdateEmployee
-- ============================================
CREATE OR ALTER PROCEDURE sp_UpdateEmployee
    @EmployeeId INT,
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Department NVARCHAR(50),
    @Designation NVARCHAR(50),
    @JoinDate DATE,
    @Status NVARCHAR(50),
    @Email NVARCHAR(50),
    @Phone NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Employees
    SET 
        FirstName = @FirstName,
        LastName = @LastName,
        Department = @Department,
        Designation = @Designation,
        JoinDate = @JoinDate,
        Status = @Status,
        Email = @Email,
        Phone = @Phone
    WHERE EmployeeId = @EmployeeId;
END;
GO

-- ============================================
-- Stored Procedure: sp_DeleteEmployee
-- ============================================

CREATE OR ALTER PROCEDURE sp_DeleteEmployee
    @EmployeeId INT,
    @SoftDelete BIT = 1  -- 1 = mark as Inactive, 0 = permanently delete
AS
BEGIN
    SET NOCOUNT ON;

    IF @SoftDelete = 1
    BEGIN
        UPDATE Employees
        SET Status = 'Inactive'
        WHERE EmployeeId = @EmployeeId;
    END
    ELSE
    BEGIN
        DELETE FROM Employees
        WHERE EmployeeId = @EmployeeId;
    END
END;
GO

-- ============================================
-- Stored Procedure: sp_GetAllEmployees
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetAllEmployees
    @Department NVARCHAR(50) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Employees
    WHERE 
        (@Department IS NULL OR Department = @Department)
        AND (@Status IS NULL OR Status = @Status)
    ORDER BY EmployeeId;
END;
GO

-- ============================================
-- Stored Procedure: sp_GetEmployeeById
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetEmployeeById
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Employees
    WHERE EmployeeId = @EmployeeId;
END;
GO

-- ============================================
-- Stored Procedure: sp_GetDepartments
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetDepartments
    @Status NVARCHAR(50) = NULL  -- Optional: 'Active' or 'Inactive'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT Department
    FROM Employees
    WHERE (@Status IS NULL OR Status = @Status)
    ORDER BY Department;
END;
GO


-- ============================================
-- Stored Procedure: sp_MarkAttendance
-- ============================================
CREATE PROCEDURE sp_MarkAttendance
    @EmployeeId INT,
    @Action NVARCHAR(10)  -- 'CheckIn' or 'CheckOut'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);

    IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeId = @EmployeeId AND Status = 'Active')
    BEGIN
        RAISERROR('Employee not found or inactive.', 16, 1);
        RETURN;
    END

    IF @Action = 'CheckIn'
    BEGIN
        IF EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today)
        BEGIN
            RAISERROR('Attendance already marked for today.', 16, 1);
            RETURN;
        END

        INSERT INTO Attendance (EmployeeId, AttendanceDate, CheckIn, Status)
        VALUES (@EmployeeId, @Today, CAST(GETDATE() AS TIME), 'Present');
    END
    ELSE IF @Action = 'CheckOut'
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today)
        BEGIN
            RAISERROR('No check-in record found for today.', 16, 1);
            RETURN;
        END

        UPDATE Attendance
        SET CheckOut = CAST(GETDATE() AS TIME)
        WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today;
    END
    ELSE
    BEGIN
        RAISERROR('Invalid action. Use CheckIn or CheckOut.', 16, 1);
    END
END;
GO

-- ============================================
-- Stored Procedure: sp_GetAttendanceReport
-- ============================================
CREATE PROCEDURE sp_GetAttendanceReport
    @StartDate DATE,
    @EndDate DATE,
    @EmployeeId INT = NULL,       -- Optional filter
    @Department NVARCHAR(50) = NULL  -- Optional filter
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.AttendanceId,
        e.EmployeeId,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        e.Department,
        e.Designation,
        a.AttendanceDate,
        a.CheckIn,
        a.CheckOut,
        a.Status
    FROM Attendance a
    INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
    WHERE a.AttendanceDate BETWEEN @StartDate AND @EndDate
      AND (@EmployeeId IS NULL OR e.EmployeeId = @EmployeeId)
      AND (@Department IS NULL OR e.Department = @Department)
    ORDER BY e.EmployeeId, a.AttendanceDate;
END;
GO



-- ============================================
-- Stored Procedure: sp_MarkCheckIn
-- Description: Marks check-in for the current day
-- ============================================
CREATE PROCEDURE sp_MarkCheckIn
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);

    -- Validate employee
    IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeId = @EmployeeId AND Status = 'Active')
    BEGIN
        RAISERROR('Employee not found or inactive.', 16, 1);
        RETURN;
    END

    -- Prevent double check-in
    IF EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today)
    BEGIN
        RAISERROR('Check-in already recorded for today.', 16, 1);
        RETURN;
    END

    INSERT INTO Attendance (EmployeeId, AttendanceDate, CheckIn, Status)
    VALUES (@EmployeeId, @Today, CAST(GETDATE() AS TIME), 'Present');

    SELECT 1 AS Success;
END;
GO


-- ============================================
-- Stored Procedure: sp_MarkCheckOut
-- Description: Marks check-out for the current day
-- ============================================
CREATE PROCEDURE sp_MarkCheckOut
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);

    IF NOT EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today)
    BEGIN
        RAISERROR('No check-in found for today.', 16, 1);
        RETURN;
    END

    UPDATE Attendance
    SET CheckOut = CAST(GETDATE() AS TIME)
    WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today;

    SELECT 1 AS Success;
END;
GO


-- ============================================
-- Stored Procedure: sp_ManualEntry
-- Description: Allows admin to manually insert or update attendance
-- ============================================
CREATE PROCEDURE sp_ManualEntry
    @EmployeeId INT,
    @AttendanceDate DATE,
    @CheckIn TIME = NULL,
    @CheckOut TIME = NULL,
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @AttendanceDate)
    BEGIN
        UPDATE Attendance
        SET CheckIn = @CheckIn,
            CheckOut = @CheckOut,
            Status = @Status
        WHERE EmployeeId = @EmployeeId AND AttendanceDate = @AttendanceDate;
    END
    ELSE
    BEGIN
        INSERT INTO Attendance (EmployeeId, AttendanceDate, CheckIn, CheckOut, Status)
        VALUES (@EmployeeId, @AttendanceDate, @CheckIn, @CheckOut, @Status);
    END

    SELECT 1 AS Success;
END;
GO

-- ============================================
-- Stored Procedure: sp_GetMyAttendance
-- Description: Returns attendance records for a specific employee and date range
-- ============================================
CREATE PROCEDURE sp_GetMyAttendance
    @EmployeeId INT,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        AttendanceDate,
        CheckIn,
        CheckOut,
        Status
    FROM Attendance
    WHERE EmployeeId = @EmployeeId
      AND AttendanceDate BETWEEN @StartDate AND @EndDate
    ORDER BY AttendanceDate;
END;
GO


-- ============================================
-- Stored Procedure: sp_CalculateHours
-- Description: Calculates total worked hours between check-in and check-out times
-- ============================================
CREATE PROCEDURE sp_CalculateHours
    @CheckIn TIME,
    @CheckOut TIME
AS
BEGIN
    SET NOCOUNT ON;

    IF @CheckIn IS NULL OR @CheckOut IS NULL
    BEGIN
        SELECT CAST(0 AS DECIMAL(5,2)) AS HoursWorked;
        RETURN;
    END

    DECLARE @Minutes INT = DATEDIFF(MINUTE, @CheckIn, @CheckOut);
    SELECT CAST(@Minutes / 60.0 AS DECIMAL(5,2)) AS HoursWorked;
END;
GO

-- ============================================
-- Stored Procedure: sp_GetTodayAttendance
-- Description: Calculates total worked hours between check-in and check-out times
-- ============================================
CREATE PROCEDURE sp_GetTodayAttendance
    @EmployeeId INT,
    @AttendanceDate DATE
AS
BEGIN
    SELECT 
        a.AttendanceId,
        a.EmployeeId,
        CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
        a.AttendanceDate,
        a.CheckIn,
        a.CheckOut,
        a.TotalHours,
        a.Status
    FROM Attendance a
    INNER JOIN Employee e ON a.EmployeeId = e.EmployeeId
    WHERE a.EmployeeId = @EmployeeId 
    AND CAST(a.AttendanceDate AS DATE) = @AttendanceDate
END


-- ============================================
-- ALTER Attendance Table to Add TotalHours Column
-- ============================================
ALTER TABLE Attendance
ADD TotalHours DECIMAL(5,2) NULL;
GO

-- ============================================
-- FIXED: sp_GetTodayAttendance
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetTodayAttendance
    @EmployeeId INT,
    @AttendanceDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.AttendanceId,
        a.EmployeeId,
        CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
        a.AttendanceDate,
        -- Convert TIME to DATETIME by combining with AttendanceDate
        CASE 
            WHEN a.CheckIn IS NOT NULL 
            THEN CAST(CAST(a.AttendanceDate AS DATETIME) + CAST(a.CheckIn AS DATETIME) AS DATETIME)
            ELSE NULL 
        END AS CheckIn,
        CASE 
            WHEN a.CheckOut IS NOT NULL 
            THEN CAST(CAST(a.AttendanceDate AS DATETIME) + CAST(a.CheckOut AS DATETIME) AS DATETIME)
            ELSE NULL 
        END AS CheckOut,
        ISNULL(a.TotalHours, 0) AS TotalHours,
        a.Status
    FROM Attendance a
    INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId  -- Fixed: Employee -> Employees
    WHERE a.EmployeeId = @EmployeeId 
    AND CAST(a.AttendanceDate AS DATE) = @AttendanceDate;
END;
GO

-- ============================================
-- FIXED: sp_GetAttendanceReport
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetAttendanceReport
    @StartDate DATE,
    @EndDate DATE,
    @EmployeeId INT = NULL,
    @Department NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.AttendanceId,
        e.EmployeeId,
        CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
        e.Department,
        e.Designation,
        a.AttendanceDate,
        -- Convert TIME to DATETIME
        CASE 
            WHEN a.CheckIn IS NOT NULL 
            THEN CAST(CAST(a.AttendanceDate AS DATETIME) + CAST(a.CheckIn AS DATETIME) AS DATETIME)
            ELSE NULL 
        END AS CheckIn,
        CASE 
            WHEN a.CheckOut IS NOT NULL 
            THEN CAST(CAST(a.AttendanceDate AS DATETIME) + CAST(a.CheckOut AS DATETIME) AS DATETIME)
            ELSE NULL 
        END AS CheckOut,
        ISNULL(a.TotalHours, 0) AS TotalHours,
        a.Status
    FROM Attendance a
    INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
    WHERE a.AttendanceDate BETWEEN @StartDate AND @EndDate
      AND (@EmployeeId IS NULL OR e.EmployeeId = @EmployeeId)
      AND (@Department IS NULL OR e.Department = @Department)
    ORDER BY e.EmployeeId, a.AttendanceDate;
END;
GO

-- ============================================
-- FIXED: sp_GetMyAttendance
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetMyAttendance
    @EmployeeId INT,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.AttendanceId,
        a.EmployeeId,
        CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
        a.AttendanceDate,
        -- Convert TIME to DATETIME
        CASE 
            WHEN a.CheckIn IS NOT NULL 
            THEN CAST(CAST(a.AttendanceDate AS DATETIME) + CAST(a.CheckIn AS DATETIME) AS DATETIME)
            ELSE NULL 
        END AS CheckIn,
        CASE 
            WHEN a.CheckOut IS NOT NULL 
            THEN CAST(CAST(a.AttendanceDate AS DATETIME) + CAST(a.CheckOut AS DATETIME) AS DATETIME)
            ELSE NULL 
        END AS CheckOut,
        ISNULL(a.TotalHours, 0) AS TotalHours,
        a.Status
    FROM Attendance a
    INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
    WHERE a.EmployeeId = @EmployeeId
      AND a.AttendanceDate BETWEEN @StartDate AND @EndDate
    ORDER BY a.AttendanceDate;
END;
GO

-- ============================================
-- FIXED: sp_MarkCheckIn - Now calculates hours
-- ============================================
CREATE OR ALTER PROCEDURE sp_MarkCheckIn
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);

    IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeId = @EmployeeId AND Status = 'Active')
    BEGIN
        RAISERROR('Employee not found or inactive.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today)
    BEGIN
        RAISERROR('Check-in already recorded for today.', 16, 1);
        RETURN;
    END

    INSERT INTO Attendance (EmployeeId, AttendanceDate, CheckIn, TotalHours, Status)
    VALUES (@EmployeeId, @Today, CAST(GETDATE() AS TIME), 0, 'Present');

    SELECT 1 AS Success;
END;
GO

-- ============================================
-- FIXED: sp_MarkCheckOut - Now calculates hours
-- ============================================
CREATE OR ALTER PROCEDURE sp_MarkCheckOut
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @CheckInTime TIME;
    DECLARE @CheckOutTime TIME = CAST(GETDATE() AS TIME);
    DECLARE @Hours DECIMAL(5,2);

    IF NOT EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today)
    BEGIN
        RAISERROR('No check-in found for today.', 16, 1);
        RETURN;
    END

    -- Get CheckIn time
    SELECT @CheckInTime = CheckIn 
    FROM Attendance 
    WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today;

    -- Calculate hours
    SET @Hours = CAST(DATEDIFF(MINUTE, @CheckInTime, @CheckOutTime) / 60.0 AS DECIMAL(5,2));

    UPDATE Attendance
    SET CheckOut = @CheckOutTime,
        TotalHours = @Hours
    WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Today;

    SELECT 1 AS Success;
END;
GO

-- ============================================
-- FIXED: sp_ManualEntry - Now calculates hours
-- ============================================
CREATE OR ALTER PROCEDURE sp_ManualEntry
    @EmployeeId INT,
    @AttendanceDate DATE,
    @CheckIn DATETIME = NULL,
    @CheckOut DATETIME = NULL,
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CheckInTime TIME = CAST(@CheckIn AS TIME);
    DECLARE @CheckOutTime TIME = CAST(@CheckOut AS TIME);
    DECLARE @Hours DECIMAL(5,2) = 0;

    -- Calculate hours if both times provided
    IF @CheckIn IS NOT NULL AND @CheckOut IS NOT NULL
    BEGIN
        SET @Hours = CAST(DATEDIFF(MINUTE, @CheckInTime, @CheckOutTime) / 60.0 AS DECIMAL(5,2));
    END

    IF EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @AttendanceDate)
    BEGIN
        UPDATE Attendance
        SET CheckIn = @CheckInTime,
            CheckOut = @CheckOutTime,
            TotalHours = @Hours,
            Status = @Status
        WHERE EmployeeId = @EmployeeId AND AttendanceDate = @AttendanceDate;
    END
    ELSE
    BEGIN
        INSERT INTO Attendance (EmployeeId, AttendanceDate, CheckIn, CheckOut, TotalHours, Status)
        VALUES (@EmployeeId, @AttendanceDate, @CheckInTime, @CheckOutTime, @Hours, @Status);
    END

    SELECT 1 AS Success;
END;
GO

-- ============================================
-- Test the fixed procedures
-- ============================================
-- Test GetTodayAttendance
EXEC sp_GetTodayAttendance @EmployeeId = 1, @AttendanceDate = '2025-10-23';

-- Test GetMyAttendance
EXEC sp_GetMyAttendance @EmployeeId = 1, @StartDate = '2025-10-01', @EndDate = '2025-10-31';

-- Test GetAttendanceReport
EXEC sp_GetAttendanceReport @StartDate = '2025-10-01', @EndDate = '2025-10-31', @EmployeeId = NULL;