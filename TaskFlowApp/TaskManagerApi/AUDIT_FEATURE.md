# 🔍 Audit Trail Feature - SS.DataCore.Save Integration

## Overview
This feature adds **complete audit tracking** to your Task Manager using the **SS.DataCore.Save** package. Every task creation, modification, and deletion is automatically logged with:
- Who made the change
- When it was made
- What fields changed
- Old vs New values

---

## 📋 Setup Instructions

### 1. Run SQL Script
Execute the audit setup script in SQL Server Management Studio:

```sql
-- File: SQL/SetupAuditTables.sql
```

This creates:
- `TimeZoneMst` - Timezone reference table
- `ProfileMst` - User profile/role table
- `AuditHdr` - Audit header (tracks each save operation)
- `AuditDtl` - Audit detail (tracks field-level changes)
- Adds audit fields to `BT_Tasks` table

### 2. Restore NuGet Packages
```bash
cd TaskManagerApi
dotnet restore
```

### 3. Run the Application
```bash
dotnet run
```

---

## 🎯 New API Endpoint

### Get Task Audit History
```http
GET /api/audit/task/{taskId}
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "auditId": 1,
    "operationType": "Edit",
    "changedOn": "2024-03-15T10:30:00Z",
    "changedBy": "John Doe (jdoe)",
    "fieldName": "Status",
    "oldValue": "Pending",
    "newValue": "Completed",
    "remark": "Status updated"
  }
]
```

---

## 🔧 How It Works

### AuditService
The `AuditService` uses **SS.DataCore.Save** to:
1. Automatically capture field changes
2. Store audit trail in `AuditHdr` and `AuditDtl` tables
3. Track user signature and timestamp

### Database Schema

**AuditHdr** (Header - one per save operation)
- `nAuditHdrNo` - Unique audit ID
- `vTableName` - Table name (BT_Tasks)
- `nRecordPK` - Primary key of changed record
- `vOperationType` - Add/Edit/Delete
- `dRecordedOnUTC` - When changed
- `vRecordedSign` - Who changed (user signature)

**AuditDtl** (Detail - one per field change)
- `nAuditDtlNo` - Detail ID
- `nAuditHdrNo` - Links to header
- `vFieldName` - Which field changed
- `vOldValue` - Previous value
- `vNewValue` - New value
- `vAuditRemark` - Optional remark

---

## 💡 Usage Example

### Frontend Integration (React)
```javascript
// Fetch audit history for a task
const fetchAuditHistory = async (taskId) => {
  const token = localStorage.getItem('token');
  const response = await fetch(`http://localhost:5022/api/audit/task/${taskId}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const history = await response.json();
  console.log(history);
};
```

---

## 🎨 UI Enhancement Ideas

1. **Audit History Modal**
   - Show change history when user clicks "View History" on a task
   - Display timeline of all changes

2. **Activity Feed**
   - Dashboard widget showing recent task changes
   - "Who changed what" feed

3. **Change Comparison**
   - Side-by-side view of old vs new values
   - Highlight changed fields

---

## 📊 Benefits

✅ **Compliance** - Track all data changes for auditing
✅ **Accountability** - Know who made each change
✅ **Debugging** - Trace when issues were introduced
✅ **Transparency** - Users can see task history
✅ **Automatic** - No manual logging code needed

---

## 🔐 Security Features

- User signature includes username and profile
- UTC timestamps prevent timezone manipulation
- Audit records are immutable (insert-only)
- Transaction-based saves ensure data consistency

---

## 🚀 Next Steps

1. Run the SQL setup script
2. Test the audit endpoint with Postman
3. Add UI components to display audit history
4. Extend to other entities (Users, Categories, etc.)

---

**Package Used:** SS.DataCore.Save v1.0.0
**Documentation:** See SS.DataCore.Save guide for advanced features
