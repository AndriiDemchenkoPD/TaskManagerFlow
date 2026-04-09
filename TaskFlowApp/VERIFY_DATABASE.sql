-- Verify TaskManagerDB Database Setup
-- Run this in SQL Server Management Studio (SSMS)

USE TaskManagerDB;
GO

-- Check if AppUsers table exists and has data
SELECT 'AppUsers Table' AS TableName, COUNT(*) AS RecordCount FROM AppUsers;
SELECT TOP 5 Id, Username, Email, CreatedAt FROM AppUsers ORDER BY CreatedAt DESC;

-- Check if BT_Tasks table exists
SELECT 'BT_Tasks Table' AS TableName, COUNT(*) AS RecordCount FROM BT_Tasks;

-- Check if Projects table exists
IF OBJECT_ID('Projects', 'U') IS NOT NULL
    SELECT 'Projects Table' AS TableName, COUNT(*) AS RecordCount FROM Projects;
ELSE
    SELECT 'Projects table does not exist' AS Status;

-- Check if Tags table exists
IF OBJECT_ID('Tags', 'U') IS NOT NULL
    SELECT 'Tags Table' AS TableName, COUNT(*) AS RecordCount FROM Tags;
ELSE
    SELECT 'Tags table does not exist' AS Status;

-- Check if TaskComments table exists
IF OBJECT_ID('TaskComments', 'U') IS NOT NULL
    SELECT 'TaskComments Table' AS TableName, COUNT(*) AS RecordCount FROM TaskComments;
ELSE
    SELECT 'TaskComments table does not exist' AS Status;

-- Verify connection string matches
SELECT 
    @@SERVERNAME AS ServerName,
    DB_NAME() AS DatabaseName,
    SUSER_NAME() AS CurrentUser;
