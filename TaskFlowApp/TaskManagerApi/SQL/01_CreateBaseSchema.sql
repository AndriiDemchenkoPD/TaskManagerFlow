-- =============================================
-- Task Manager - Base Schema Setup
-- Creates essential tables for the application
-- =============================================

USE BasicTraining;
GO

-- 1. Create AppUsers table (User authentication)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppUsers')
BEGIN
    CREATE TABLE dbo.AppUsers (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        IsActive BIT DEFAULT 1
    );

    CREATE INDEX IX_AppUsers_Username ON AppUsers(Username);
    CREATE INDEX IX_AppUsers_Email ON AppUsers(Email);
END
GO

-- 2. Create BT_Tasks table (Main tasks table)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BT_Tasks')
BEGIN
    CREATE TABLE dbo.BT_Tasks (
        TaskId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        Status NVARCHAR(20) DEFAULT 'Pending',
        IsCompleted BIT DEFAULT 0,
        Priority NVARCHAR(20) DEFAULT 'Medium',
        Category NVARCHAR(50) DEFAULT 'General',
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE(),
        DueDate DATETIME NULL,
        CompletedAt DATETIME NULL,
        IsDeleted BIT DEFAULT 0,
        FOREIGN KEY (UserId) REFERENCES AppUsers(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_BT_Tasks_UserId ON BT_Tasks(UserId);
    CREATE INDEX IX_BT_Tasks_IsCompleted ON BT_Tasks(IsCompleted);
END
GO

PRINT 'Base schema created successfully!';