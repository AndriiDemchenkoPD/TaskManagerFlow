-- =============================================
-- Task Manager - Audit Trail Setup Script
-- Using SS.DataCore.Save Package
-- =============================================

USE BasicTraining;
GO

-- 1. Create TimeZoneMst table (required by SS.DataCore.Save)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TimeZoneMst')
BEGIN
    CREATE TABLE dbo.TimeZoneMst (
        nTimeZoneNo INT PRIMARY KEY IDENTITY(1,1),
        vTimeZoneName NVARCHAR(100) NOT NULL,
        vOffSet NVARCHAR(10) NOT NULL,
        IsActive BIT DEFAULT 1
    );

    -- Insert default timezone
    INSERT INTO TimeZoneMst (vTimeZoneName, vOffSet) 
    VALUES ('UTC', '+00:00'), ('IST', '+05:30');
END
GO

-- 2. Update AppUsers table to add ProfileNo
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AppUsers') AND name = 'nProfileNo')
BEGIN
    ALTER TABLE AppUsers ADD nProfileNo INT DEFAULT 1;
END
GO

-- 3. Create ProfileMst table (required by SS.DataCore.Save)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProfileMst')
BEGIN
    CREATE TABLE dbo.ProfileMst (
        nProfileNo INT PRIMARY KEY IDENTITY(1,1),
        vProfileName NVARCHAR(50) NOT NULL,
        IsActive BIT DEFAULT 1
    );

    INSERT INTO ProfileMst (vProfileName) VALUES ('User'), ('Admin');
END
GO

-- 4. Update BT_Tasks table with audit fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BT_Tasks') AND name = 'nRecordedBy')
BEGIN
    ALTER TABLE BT_Tasks ADD 
        nRecordedBy INT NULL,
        dRecordedOnUTC DATETIME NULL,
        nRecordedAtTimeZone INT NULL,
        vRecordedAtOffSet NVARCHAR(10) NULL,
        vRecordedSign NVARCHAR(200) NULL,
        cReplicaFlag CHAR(1) DEFAULT 'N',
        nModifiedBy INT NULL,
        dModifiedOnUTC DATETIME NULL,
        vModifiedSign NVARCHAR(200) NULL;
END
GO

-- 5. Create AuditHdr table (Audit Header - tracks each save operation)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditHdr')
BEGIN
    CREATE TABLE dbo.AuditHdr (
        nAuditHdrNo BIGINT PRIMARY KEY IDENTITY(1,1),
        vTableName NVARCHAR(100) NOT NULL,
        vSchemaName NVARCHAR(50) DEFAULT 'dbo',
        nRecordPK NVARCHAR(50) NOT NULL,
        vOperationType NVARCHAR(20) NOT NULL, -- Add/Edit/Delete
        nRecordedBy INT NOT NULL,
        dRecordedOnUTC DATETIME NOT NULL,
        nRecordedAtTimeZone INT NOT NULL,
        vRecordedAtOffSet NVARCHAR(10) NOT NULL,
        vRecordedSign NVARCHAR(200) NOT NULL,
        cReplicaFlag CHAR(1) DEFAULT 'N'
    );

    CREATE INDEX IX_AuditHdr_Table ON AuditHdr(vTableName, nRecordPK);
    CREATE INDEX IX_AuditHdr_Date ON AuditHdr(dRecordedOnUTC DESC);
END
GO

-- 6. Create AuditDtl table (Audit Detail - tracks field-level changes)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditDtl')
BEGIN
    CREATE TABLE dbo.AuditDtl (
        nAuditDtlNo BIGINT PRIMARY KEY IDENTITY(1,1),
        nAuditHdrNo BIGINT NOT NULL,
        vFieldName NVARCHAR(100) NOT NULL,
        vOldValue NVARCHAR(MAX) NULL,
        vNewValue NVARCHAR(MAX) NULL,
        vAuditRemark NVARCHAR(500) NULL,
        FOREIGN KEY (nAuditHdrNo) REFERENCES AuditHdr(nAuditHdrNo)
    );

    CREATE INDEX IX_AuditDtl_Hdr ON AuditDtl(nAuditHdrNo);
END
GO

PRINT 'Audit Trail tables created successfully!';
