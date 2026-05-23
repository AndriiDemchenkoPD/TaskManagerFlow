-- STEP 1: Add new columns to existing Tasks table
-- Run this first to enhance your existing tasks

ALTER TABLE Tasks ADD 
    ParentTaskId INT NULL,
    ProjectId INT NULL,
    TimeSpentMinutes INT DEFAULT 0;

-- STEP 2: Create Projects table
CREATE TABLE Projects (
    ProjectId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ProjectName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Color NVARCHAR(7) DEFAULT '#3B82F6',
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES AppUsers(UserId)
);

-- STEP 3: Create Tags table
CREATE TABLE Tags (
    TagId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TagName NVARCHAR(100) NOT NULL,
    Color NVARCHAR(7) DEFAULT '#10B981',
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AppUsers(UserId)
);

-- STEP 4: Create TaskTags junction table
CREATE TABLE TaskTags (
    TaskId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (TaskId, TagId),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (TagId) REFERENCES Tags(TagId)
);

-- STEP 5: Create Comments table
CREATE TABLE TaskComments (
    CommentId INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (UserId) REFERENCES AppUsers(UserId)
);

-- STEP 6: Create Subtasks table
CREATE TABLE Subtasks (
    SubtaskId INT PRIMARY KEY IDENTITY(1,1),
    ParentTaskId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    IsCompleted BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CompletedAt DATETIME NULL,
    FOREIGN KEY (ParentTaskId) REFERENCES Tasks(TaskId)
);

-- STEP 7: Insert sample data for testing

-- Sample Projects
INSERT INTO Projects (UserId, ProjectName, Description, Color) VALUES
(1, 'Website Redesign', 'Q1 2024 project', '#3B82F6'),
(1, 'Mobile App', 'iOS and Android development', '#10B981'),
(1, 'Backend API', 'New features and endpoints', '#F59E0B');

-- Sample Tags
INSERT INTO Tags (UserId, TagName, Color) VALUES
(1, 'Urgent', '#EF4444'),
(1, 'High Priority', '#F59E0B'),
(1, 'Bug Fix', '#8B5CF6'),
(1, 'Feature', '#10B981');

-- Sample Subtasks (assuming TaskId 1 exists)
INSERT INTO Subtasks (ParentTaskId, Title, IsCompleted) VALUES
(1, 'Design mockups', 1),
(1, 'Create wireframes', 0),
(1, 'Implement frontend', 0);

-- Sample Comments (assuming TaskId 1 exists)
INSERT INTO TaskComments (TaskId, UserId, CommentText) VALUES
(1, 1, 'This is a test comment'),
(1, 1, 'Working on this task now');

-- Done! Now your features will work
SELECT 'Database setup complete!' AS Status;
