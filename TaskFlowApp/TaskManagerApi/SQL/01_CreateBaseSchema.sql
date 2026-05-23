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

-- 2. Create Tasks table (Main tasks table)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
BEGIN
    CREATE TABLE dbo.Tasks (
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
        DueTime TIME NULL,
        CompletedAt DATETIME NULL,
        IsDeleted BIT DEFAULT 0,
        FOREIGN KEY (UserId) REFERENCES AppUsers(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_Tasks_UserId ON Tasks(UserId);
    CREATE INDEX IX_Tasks_IsCompleted ON Tasks(IsCompleted);
END
GO

-- 3. Create PasswordResetTokens table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordResetTokens')
BEGIN
    CREATE TABLE dbo.PasswordResetTokens (
        Id INT PRIMARY KEY IDENTITY(1,1),
        AppUserId INT NOT NULL,
        TokenHash NVARCHAR(200) NOT NULL UNIQUE,
        ExpiresAtUtc DATETIME2 NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        UsedAtUtc DATETIME2 NULL,
        FOREIGN KEY (AppUserId) REFERENCES AppUsers(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_PasswordResetTokens_AppUserId_ExpiresAtUtc
        ON PasswordResetTokens(AppUserId, ExpiresAtUtc);
END
GO

PRINT 'Base schema created successfully!';