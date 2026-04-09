-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BasicTraining')
BEGIN
    CREATE DATABASE BasicTraining;
END
GO

USE BasicTraining;
GO