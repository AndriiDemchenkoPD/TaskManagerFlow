USE BasicTraining;
GO

-- Ensure extra task fields exist
IF COL_LENGTH('dbo.BT_Tasks', 'ParentTaskId') IS NULL
    ALTER TABLE dbo.BT_Tasks ADD ParentTaskId INT NULL;
GO

IF COL_LENGTH('dbo.BT_Tasks', 'ProjectId') IS NULL
    ALTER TABLE dbo.BT_Tasks ADD ProjectId INT NULL;
GO

IF COL_LENGTH('dbo.BT_Tasks', 'TimeSpentMinutes') IS NULL
    ALTER TABLE dbo.BT_Tasks ADD TimeSpentMinutes INT DEFAULT 0;
GO

IF COL_LENGTH('dbo.BT_Tasks', 'RecurrencePattern') IS NULL
    ALTER TABLE dbo.BT_Tasks ADD RecurrencePattern NVARCHAR(50) NULL;
GO

IF COL_LENGTH('dbo.BT_Tasks', 'IsRecurring') IS NULL
    ALTER TABLE dbo.BT_Tasks ADD IsRecurring BIT DEFAULT 0;
GO

IF COL_LENGTH('dbo.BT_Tasks', 'AssignedToUserId') IS NULL
    ALTER TABLE dbo.BT_Tasks ADD AssignedToUserId INT NULL;
GO

-- Add missing foreign keys only if absent
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BT_Tasks_ParentTaskId')
    ALTER TABLE dbo.BT_Tasks
    ADD CONSTRAINT FK_BT_Tasks_ParentTaskId FOREIGN KEY (ParentTaskId) REFERENCES dbo.BT_Tasks(TaskId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BT_Tasks_AssignedToUserId')
    ALTER TABLE dbo.BT_Tasks
    ADD CONSTRAINT FK_BT_Tasks_AssignedToUserId FOREIGN KEY (AssignedToUserId) REFERENCES dbo.AppUsers(Id);
GO

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

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tags' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Tags (
        TagId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        TagName NVARCHAR(100) NOT NULL,
        Color NVARCHAR(7) DEFAULT '#10B981',
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (UserId) REFERENCES dbo.AppUsers(Id),
        CONSTRAINT UQ_Tags_UserTag UNIQUE(UserId, TagName)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskTags' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TaskTags (
        TaskId INT NOT NULL,
        TagId INT NOT NULL,
        PRIMARY KEY (TaskId, TagId),
        FOREIGN KEY (TaskId) REFERENCES dbo.BT_Tasks(TaskId) ON DELETE CASCADE,
        FOREIGN KEY (TagId) REFERENCES dbo.Tags(TagId) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskComments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TaskComments (
        CommentId INT PRIMARY KEY IDENTITY(1,1),
        TaskId INT NOT NULL,
        UserId INT NOT NULL,
        CommentText NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE(),
        IsDeleted BIT DEFAULT 0,
        FOREIGN KEY (TaskId) REFERENCES dbo.BT_Tasks(TaskId) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES dbo.AppUsers(Id)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Subtasks' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Subtasks (
        SubtaskId INT PRIMARY KEY IDENTITY(1,1),
        ParentTaskId INT NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        IsCompleted BIT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CompletedAt DATETIME NULL,
        FOREIGN KEY (ParentTaskId) REFERENCES dbo.BT_Tasks(TaskId) ON DELETE CASCADE
    );
END
GO

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
        FOREIGN KEY (TaskId) REFERENCES dbo.BT_Tasks(TaskId) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES dbo.AppUsers(Id)
    );
END
GO

PRINT 'Feature tables ensured successfully!';
