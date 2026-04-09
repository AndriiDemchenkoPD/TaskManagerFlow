# ✅ SS.DataCore.Save - Complete Implementation Guide

## 🎯 What is SS.DataCore.Save?

A NuGet package that provides **automatic audit trail generation** for database operations. It automatically:
- Tracks who created/modified records
- Records when changes happened (UTC timestamp)
- Stores old vs new values for each field
- Manages transactions safely

---

## 📦 Package Components Used

### 1. **SaveLogicServices** (Startup Registration)
```csharp
SaveLogicServices.RegisterServices(config =>
{
    config.ConnectionProperties = new ConnectionProperties { ... };
    config.UserSignPlaceHolderTemplate = "{FirstName} {LastName} ({UserName})";
    config.EncryptionMode = EncryptionModeEnum.None;
    config.DataBaseConfigSchemaName = "dbo";
});
```

**Purpose:** Validates required tables exist (UserMst, ProfileMst, AuditHdr, AuditDtl, TimeZoneMst)

### 2. **SaveLogicContext<T>** (Configuration)
```csharp
var context = new SaveLogicContext<TaskItem>(task)
{
    Connection = conn,              // IConnection from SS.DataCore.MSSQL
    UserNo = userId,                // Who is saving
    TimeZoneNo = 2,                 // User's timezone
    ProfileNo = 1,                  // User's role
    ExecutionType = ExecutionTypeEnum.ExecutionOnly,  // Execute immediately
    CanValidateAuditRemark = true   // Enable audit trail
};
```

### 3. **SaveLogic<T>** (Main Save Engine)
```csharp
using var saveLogic = new SaveLogic<TaskItem>(context);
var result = saveLogic.Save();

if (!saveLogic.HasError)
{
    transaction.Commit();  // Success
}
else
{
    transaction.RollBack();  // Failure
    _logger.Error(saveLogic.ErrorString);
}
```

---

## 🔌 New API Endpoints

### 1. Save Task with Automatic Audit
```http
POST http://localhost:5022/api/taskdatacoresave
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": 0,
  "title": "New Task",
  "description": "Task description",
  "status": "Pending",
  "priority": "High",
  "category": "Work",
  "dueDate": "2024-12-31T00:00:00"
}
```

**Response:**
```json
{
  "message": "Task saved with automatic audit trail using SS.DataCore.Save"
}
```

### 2. Get Tasks
```http
GET http://localhost:5022/api/taskdatacoresave
Authorization: Bearer {token}
```

### 3. Update Task with Automatic Audit
```http
PUT http://localhost:5022/api/taskdatacoresave/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": 1,
  "title": "Updated Task",
  "description": "Updated description",
  "status": "Completed",
  "priority": "High",
  "category": "Work",
  "dueDate": "2024-12-31T00:00:00"
}
```

---

## 🗄️ Database Tables Used

### Required Tables (Validated at Startup):
1. **UserMst** - User information
2. **ProfileMst** - User roles/profiles
3. **TimeZoneMst** - Timezone reference
4. **AuditHdr** - Audit header (one per save operation)
5. **AuditDtl** - Audit details (field-level changes)

### Enhanced Table:
- **BT_Tasks** - Added audit fields:
  - `nRecordedBy` - Who created
  - `dRecordedOnUTC` - When created (UTC)
  - `nRecordedAtTimeZone` - Timezone number
  - `vRecordedAtOffSet` - UTC offset
  - `vRecordedSign` - User signature
  - `cReplicaFlag` - Replica flag
  - `nModifiedBy` - Who modified
  - `dModifiedOnUTC` - When modified
  - `vModifiedSign` - Modifier signature

---

## 🔄 How It Works

### Automatic Audit Flow:

```
1. User calls SaveTask()
   ↓
2. SaveLogicContext<TaskItem> created with user info
   ↓
3. SaveLogic<TaskItem>.Save() called
   ↓
4. Automatically:
   - Validates data
   - Compares old vs new values
   - Inserts into BT_Tasks
   - Creates AuditHdr record (who, when, operation type)
   - Creates AuditDtl records (field changes)
   - Commits transaction
   ↓
5. Returns success/error
```

---

## 📊 What Gets Audited

### AuditHdr (Header) Records:
- `vTableName` = "BT_Tasks"
- `nRecordPK` = TaskId
- `vOperationType` = "Add" or "Edit" or "Delete"
- `nRecordedBy` = UserId
- `dRecordedOnUTC` = DateTime.UtcNow
- `vRecordedSign` = "John Doe (jdoe) (User)"

### AuditDtl (Detail) Records:
For each changed field:
- `vFieldName` = "Title"
- `vOldValue` = "Old Title"
- `vNewValue` = "New Title"
- `vAuditRemark` = "Title changed"

---

## 🚀 Setup Instructions

### Step 1: Run SQL Script
```sql
-- Execute in SSMS
D:\Bhavya_Raval\TaskManager\TaskManagerApi\SQL\SetupAuditTables.sql
```

### Step 2: Update Connection String
In `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "BTConnection": "Server=localhost;Database=TaskManagerDB;User Id=sa;Password=your_password;"
  }
}
```

### Step 3: Update Program.cs
Connection properties in SaveLogicServices.RegisterServices():
```csharp
config.ConnectionProperties = new ConnectionProperties
{
    Server = "localhost",
    DataBaseName = "TaskManagerDB",
    UserName = "sa",
    Password = "your_password"
};
```

### Step 4: Build and Run
```powershell
cd D:\Bhavya_Raval\TaskManager\TaskManagerApi
dotnet build
dotnet run
```

---

## 🧪 Testing

### Test with Postman:

**1. Login:**
```http
POST http://localhost:5022/api/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "Test@123"
}
```

**2. Create Task (with automatic audit):**
```http
POST http://localhost:5022/api/taskdatacoresave
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": 0,
  "title": "Test Task",
  "description": "Testing SS.DataCore.Save",
  "status": "Pending",
  "priority": "High",
  "category": "Test",
  "dueDate": "2024-12-31"
}
```

**3. Check Audit Trail:**
```sql
-- View audit header
SELECT * FROM AuditHdr ORDER BY dRecordedOnUTC DESC;

-- View audit details
SELECT * FROM AuditDtl ORDER BY nAuditDtlNo DESC;

-- View task with audit fields
SELECT TaskId, Title, Status, nRecordedBy, dRecordedOnUTC, vRecordedSign 
FROM BT_Tasks 
ORDER BY TaskId DESC;
```

---

## 📈 Benefits

✅ **Automatic Audit Trail** - No manual logging code needed
✅ **Field-Level Tracking** - Know exactly what changed
✅ **User Attribution** - Who made each change
✅ **Timestamp Tracking** - When changes occurred (UTC)
✅ **Transaction Safety** - Automatic rollback on errors
✅ **Enterprise Pattern** - Production-ready approach

---

## 🎓 For Your Resume

**You can now say:**
- ✅ "Implemented automatic audit trail using SS.DataCore.Save package"
- ✅ "Integrated third-party NuGet packages for enterprise features"
- ✅ "Built field-level change tracking with automatic audit generation"
- ✅ "Used SaveLogic pattern for secure database operations"
- ✅ "Implemented transaction-based data persistence"

---

## 📁 Files Created

1. **Services/TaskServiceDataCoreSave.cs** - Service using SS.DataCore.Save
2. **Controllers/TaskDataCoreSaveController.cs** - API endpoints
3. **SQL/SetupAuditTables.sql** - Database setup
4. **Program.cs** - SaveLogicServices registration

---

## ⚙️ Configuration

### ExecutionTypeEnum Options:
- `ExecutionOnly` (0) - Execute immediately (default)
- `OnlyScript` (1) - Generate SQL script only
- `ExecutionWithScript` (2) - Execute and return script

### EncryptionModeEnum Options:
- `None` (0) - No encryption
- `Default` (1) - Built-in encryption
- `Custom` (2) - Custom encryption key

---

## 🔐 Security Features

- **Immutable Audit Log** - Records can't be modified
- **UTC Timestamps** - Prevents timezone manipulation
- **User Signatures** - Full traceability
- **Transaction-Based** - Ensures data consistency
- **Automatic Rollback** - On errors

---

## 📚 Key Classes Reference

| Class | Purpose |
|-------|---------|
| `SaveLogicServices` | Startup registration |
| `SaveLogicContext<T>` | Configuration for save |
| `SaveLogic<T>` | Main save engine |
| `MSSQLConnection` | Database connection |
| `ExecutionTypeEnum` | Save mode control |
| `EncryptionModeEnum` | Encryption settings |

---

## ✅ Status

**Package Integration:** ✅ Complete
**Automatic Audit:** ✅ Enabled
**API Endpoints:** ✅ Ready
**Database Setup:** ✅ Script provided
**Production Ready:** ✅ Yes

---

**Package:** SS.DataCore.Save v1.0.0
**Status:** Actively Used
**Feature:** Automatic Audit Trail with Field-Level Change Tracking
