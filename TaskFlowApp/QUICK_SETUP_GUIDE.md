# 🚀 QUICK SETUP - Make Features Work

## Problem
The buttons don't work because database tables don't exist yet.

## Solution (3 Steps)

### STEP 1: Run SQL Script
1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your database
3. Open file: `D:\Bhavya_Raval\TaskManager\SETUP_DATABASE.sql`
4. Click **Execute** (or press F5)

### STEP 2: Start Backend
```bash
cd D:\Bhavya_Raval\TaskManager\TaskManagerApi
dotnet run
```

### STEP 3: Start Frontend
```bash
cd D:\Bhavya_Raval\TaskManager\task-manager-ui
npm run dev
```

---

## ✅ After Setup, You'll Have:

### Working Buttons:
- 📊 **Dashboard** - Shows 6 statistics cards
- 📁 **Projects** - Shows 3 sample projects
- 🏷️ **Tags** - Shows 4 sample tags
- **Details** button on tasks - Shows subtasks & comments

### Sample Data Created:
- 3 Projects (Website Redesign, Mobile App, Backend API)
- 4 Tags (Urgent, High Priority, Bug Fix, Feature)
- 3 Subtasks for your first task
- 2 Comments for your first task

---

## 🔧 If Buttons Still Don't Work:

### Check 1: Is API running?
- Open browser: `http://localhost:5022/api/projects`
- Should see JSON response (not error)

### Check 2: Is database connected?
- Check `appsettings.json` connection string
- Make sure SQL Server is running

### Check 3: Check browser console
- Press F12 in browser
- Look for errors in Console tab
- Common error: CORS or 401 Unauthorized

---

## 📝 What Each Button Does:

### 📊 Dashboard Button
- Calls: `GET /api/dashboard/stats`
- Shows: Total tasks, completed today, overdue, in progress, completion %, projects count

### 📁 Projects Button
- Calls: `GET /api/projects`
- Shows: All your projects with colors

### 🏷️ Tags Button
- Calls: `GET /api/tags`
- Shows: All your tags with colors

### Details Button (on task card)
- Calls: `GET /api/tasks/{id}/subtasks` and `GET /api/tasks/{id}/comments`
- Shows: Subtasks checklist and comments thread

---

## 🎯 Quick Test

After running SQL script, test in browser:

1. **Test Dashboard:**
   ```
   http://localhost:5022/api/dashboard/stats
   ```
   Should return JSON with statistics

2. **Test Projects:**
   ```
   http://localhost:5022/api/projects
   ```
   Should return 3 projects

3. **Test Tags:**
   ```
   http://localhost:5022/api/tags
   ```
   Should return 4 tags

---

## ✅ Success Checklist

- [ ] SQL script executed without errors
- [ ] Backend running on port 5022
- [ ] Frontend running on port 5173
- [ ] Can see tasks in "My Tasks"
- [ ] Dashboard button shows statistics
- [ ] Projects button shows 3 projects
- [ ] Tags button shows 4 tags
- [ ] Details button shows subtasks & comments

---

**If all checked, your features are working! 🎉**
