-- =============================================
-- Task Manager - Audit Trail Setup Script
-- Creates audit tables used by AuditService
-- =============================================

USE BasicTraining;
GO

-- 1. Update Tasks table with audit fields
IF COL_LENGTH('dbo.Tasks', 'nRecordedBy') IS NULL
BEGIN
    ALTER TABLE dbo.Tasks ADD 
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

-- 2. Create AuditHdr table (Audit Header - tracks each save operation)
IF OBJECT_ID(N'dbo.AuditHdr', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditHdr (
        nAuditHdrNo BIGINT PRIMARY KEY IDENTITY(1,1),
        vTableName NVARCHAR(100) NOT NULL,
        vSchemaName NVARCHAR(50) NOT NULL CONSTRAINT DF_AuditHdr_vSchemaName DEFAULT ('dbo'),
        nRecordPK NVARCHAR(50) NOT NULL,
        vOperationType NVARCHAR(20) NOT NULL, -- Add/Edit/Delete
        nRecordedBy INT NOT NULL,
        dRecordedOnUTC DATETIME NOT NULL,
        nRecordedAtTimeZone INT NOT NULL,
        vRecordedAtOffSet NVARCHAR(10) NOT NULL,
        vRecordedSign NVARCHAR(200) NOT NULL,
        cReplicaFlag CHAR(1) DEFAULT 'N'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditHdr_Table' AND object_id = OBJECT_ID(N'dbo.AuditHdr'))
    CREATE INDEX IX_AuditHdr_Table ON dbo.AuditHdr(vTableName, nRecordPK);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditHdr_Date' AND object_id = OBJECT_ID(N'dbo.AuditHdr'))
    CREATE INDEX IX_AuditHdr_Date ON dbo.AuditHdr(dRecordedOnUTC DESC);
GO

-- 3. Create AuditDtl table (Audit Detail - tracks field-level changes)
IF OBJECT_ID(N'dbo.AuditDtl', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditDtl (
        nAuditDtlNo BIGINT PRIMARY KEY IDENTITY(1,1),
        nAuditHdrNo BIGINT NOT NULL,
        vFieldName NVARCHAR(100) NOT NULL,
        vOldValue NVARCHAR(MAX) NULL,
        vNewValue NVARCHAR(MAX) NULL,
        vAuditRemark NVARCHAR(500) NULL,
        CONSTRAINT FK_AuditDtl_AuditHdr FOREIGN KEY (nAuditHdrNo) REFERENCES dbo.AuditHdr(nAuditHdrNo)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditDtl_Hdr' AND object_id = OBJECT_ID(N'dbo.AuditDtl'))
    CREATE INDEX IX_AuditDtl_Hdr ON dbo.AuditDtl(nAuditHdrNo);
GO

PRINT 'Audit Trail tables created successfully!';
