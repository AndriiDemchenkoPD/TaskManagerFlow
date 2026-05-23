-- Add new columns to Tasks for new features
ALTER TABLE Tasks ADD
    ParentTaskId INT NULL,
    ProjectId INT NULL,
    TimeSpentMinutes INT DEFAULT 0,
    RecurrencePattern NVARCHAR(50) NULL,
    IsRecurring BIT DEFAULT 0,
    AssignedToUserId INT NULL,
    FOREIGN KEY (ParentTaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (AssignedToUserId) REFERENCES AppUsers(UserId);

-- Create Projects table
CREATE TABLE Projects (
    ProjectId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ProjectName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Color NVARCHAR(7) DEFAULT '#3B82F6',
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
);

-- Create Tags table
CREATE TABLE Tags (
    TagId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TagName NVARCHAR(100) NOT NULL,
    Color NVARCHAR(7) DEFAULT '#10B981',
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AppUsers(Id),
    UNIQUE(UserId, TagName)
);

-- Create TaskTags junction table
CREATE TABLE TaskTags (
    TaskId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (TaskId, TagId),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES Tags(TagId) ON DELETE CASCADE
);

-- Create Comments table
CREATE TABLE TaskComments (
    CommentId INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
);

-- Create Attachments table
CREATE TABLE TaskAttachments (
    AttachmentId INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(MAX) NOT NULL,
    FileSize INT,
    UploadedBy INT NOT NULL,
    UploadedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedBy) REFERENCES AppUsers(Id)
);

-- Create Subtasks table (alternative to ParentTaskId)
CREATE TABLE Subtasks (
    SubtaskId INT PRIMARY KEY IDENTITY(1,1),
    ParentTaskId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    IsCompleted BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CompletedAt DATETIME NULL,
    FOREIGN KEY (ParentTaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE
);

-- Create TaskReminders table
CREATE TABLE TaskReminders (
    ReminderId INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    ReminderTime DATETIME NOT NULL,
    ReminderType NVARCHAR(50) DEFAULT 'Email',
    IsSent BIT DEFAULT 0,
    SentAt DATETIME NULL,
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
);

-- Create UserPreferences table
CREATE TABLE UserPreferences (
    PreferenceId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL UNIQUE,
    DarkMode BIT DEFAULT 0,
    DefaultView NVARCHAR(50) DEFAULT 'List',
    NotificationsEnabled BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
);

-- Create TaskHistory for activity log
CREATE TABLE TaskHistory (
    HistoryId INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
);
