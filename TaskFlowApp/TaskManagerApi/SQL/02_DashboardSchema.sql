USE BasicTraining;
GO

-- Ensure Tasks has columns used by dashboard queries
IF COL_LENGTH('dbo.Tasks', 'Status') IS NULL
BEGIN
    ALTER TABLE dbo.Tasks ADD Status NVARCHAR(20) DEFAULT 'Pending';
END
GO

IF COL_LENGTH('dbo.Tasks', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE dbo.Tasks ADD IsDeleted BIT DEFAULT 0;
END
GO

IF COL_LENGTH('dbo.Tasks', 'CompletedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Tasks ADD CompletedAt DATETIME NULL;
END
GO

-- Projects table is required by dashboard stats
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Projects' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Projects (
        ProjectId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        ProjectName NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        Color NVARCHAR(7) DEFAULT '#3B82F6',
        CreatedAt DATETIME DEFAULT GETDATE(),
        IsDeleted BIT DEFAULT 0,
        FOREIGN KEY (UserId) REFERENCES dbo.AppUsers(Id)
    );
END
GO

-- TaskHistory is required by dashboard activity log
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskHistory' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TaskHistory (
        HistoryId INT PRIMARY KEY IDENTITY(1,1),
        TaskId INT NOT NULL,
        UserId INT NOT NULL,
        Action NVARCHAR(100) NOT NULL,
        OldValue NVARCHAR(MAX),
        NewValue NVARCHAR(MAX),
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (TaskId) REFERENCES dbo.Tasks(TaskId) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES dbo.AppUsers(Id)
    );
END
GO

PRINT 'Dashboard schema ensured successfully!';
