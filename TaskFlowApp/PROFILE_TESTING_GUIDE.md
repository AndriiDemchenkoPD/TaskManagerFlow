# Testing User Profile Feature

## ✅ Fixed Issues

Changed column references from `UserId` to `Id` to match your AppUsers table schema:

```sql
AppUsers Table:
- Id (PK, int, not null)           ← Primary Key
- Username (nvarchar(100), not null)
- Email (nvarchar(150), not null)
- PasswordHash (nvarchar(255), not null)
- CreatedAt (datetime, null)
```

## 🚀 How to Test

### Step 1: Stop Running API
Press `Ctrl+C` in the terminal where API is running

### Step 2: Rebuild API
```bash
cd d:\Bhavya_Raval\TaskManager\TaskManagerApi
dotnet build
```

### Step 3: Run API
```bash
dotnet run
```

### Step 4: Run Frontend (in new terminal)
```bash
cd d:\Bhavya_Raval\TaskManager\task-manager-ui
npm run dev
```

### Step 5: Test Profile Feature

1. **Login** with your existing credentials
   - Example: username = "testuser", password = "test123"

2. **Check Top Right Corner**
   - You should see your avatar with first letter
   - Click on it to see dropdown

3. **Verify Data from Database**
   - Username shown = Your actual username from AppUsers table
   - Email shown = Your actual email from AppUsers table

4. **Edit Profile**
   - Click "Edit Profile" in dropdown
   - Modal opens with current username and email
   - Change username (e.g., "testuser" → "test_user")
   - Change email (e.g., "test@test.com" → "test.user@test.com")
   - Click "Save Changes"

5. **Verify Update**
   - Profile updates immediately
   - Avatar shows new username's first letter
   - Dropdown shows new username and email

6. **Verify Database Persistence**
   - Logout
   - Login again
   - Check profile - should show updated values
   - This confirms data is saved in database

## 🔍 Verify in Database

Open SQL Server Management Studio and run:

```sql
SELECT Id, Username, Email, CreatedAt 
FROM AppUsers 
WHERE Username = 'your_username'
```

You should see your updated username and email!

## 📊 API Endpoints

### Get Profile
```
GET http://localhost:5022/api/user/profile
Headers: Authorization: Bearer {your_token}
```

### Update Profile
```
PUT http://localhost:5022/api/user/profile
Headers: Authorization: Bearer {your_token}
Body: {
  "username": "new_username",
  "email": "new@email.com"
}
```

## ✨ Expected Behavior

✅ Profile loads from AppUsers table
✅ Shows correct username from database
✅ Shows correct email from database
✅ Can edit username
✅ Can edit email
✅ Saves to AppUsers table
✅ Changes persist after logout/login
✅ Theme works perfectly (dark/light)
✅ No console errors

## 🐛 If Issues Occur

1. **Profile not loading?**
   - Check browser console for errors
   - Verify JWT token is valid
   - Check API is running on port 5022

2. **Wrong email showing?**
   - Verify AppUsers table has correct data
   - Check SQL query returns correct columns

3. **Can't update?**
   - Check network tab for API errors
   - Verify UPDATE query has correct WHERE clause
   - Check user has permission to update

---

**Status**: ✅ Ready to test
**Database**: ✅ Using AppUsers table with Id column
**Columns**: ✅ Id, Username, Email, PasswordHash
