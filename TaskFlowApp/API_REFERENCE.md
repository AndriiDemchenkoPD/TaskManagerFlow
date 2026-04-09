# 🔌 Task Manager - API Reference Guide

## 📚 Complete API Documentation

---

## 🏗️ Projects API

### Create Project
```http
POST /api/projects
Content-Type: application/json
Authorization: Bearer {token}

{
  "projectName": "Website Redesign",
  "description": "Q1 2024 project",
  "color": "#3B82F6"
}

Response: 200 OK
{
  "message": "Project created successfully"
}
```

### Get All Projects
```http
GET /api/projects
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "projectId": 1,
    "userId": 1,
    "projectName": "Website Redesign",
    "description": "Q1 2024 project",
    "color": "#3B82F6",
    "createdAt": "2024-01-15T10:30:00",
    "isDeleted": false
  }
]
```

### Update Project
```http
PUT /api/projects/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "projectName": "Website Redesign v2",
  "description": "Updated description",
  "color": "#10B981"
}

Response: 200 OK
{
  "message": "Project updated successfully"
}
```

### Delete Project
```http
DELETE /api/projects/1
Authorization: Bearer {token}

Response: 200 OK
{
  "message": "Project deleted successfully"
}
```

---

## 🏷️ Tags API

### Create Tag
```http
POST /api/tags
Content-Type: application/json
Authorization: Bearer {token}

{
  "tagName": "Urgent",
  "color": "#EF4444"
}

Response: 200 OK
{
  "message": "Tag created successfully"
}
```

### Get All Tags
```http
GET /api/tags
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "tagId": 1,
    "userId": 1,
    "tagName": "Urgent",
    "color": "#EF4444",
    "createdAt": "2024-01-15T10:30:00"
  }
]
```

### Get Task Tags
```http
GET /api/tags/task/5
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "tagId": 1,
    "userId": 1,
    "tagName": "Urgent",
    "color": "#EF4444",
    "createdAt": "2024-01-15T10:30:00"
  }
]
```

### Add Tag to Task
```http
POST /api/tags/task/5/tag/1
Authorization: Bearer {token}

Response: 200 OK
{
  "message": "Tag added to task"
}
```

### Remove Tag from Task
```http
DELETE /api/tags/task/5/tag/1
Authorization: Bearer {token}

Response: 200 OK
{
  "message": "Tag removed from task"
}
```

---

## 💬 Comments API

### Get Task Comments
```http
GET /api/tasks/5/comments
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "commentId": 1,
    "taskId": 5,
    "userId": 1,
    "commentText": "Need to review this with the team",
    "createdAt": "2024-01-15T10:30:00",
    "updatedAt": "2024-01-15T10:30:00",
    "isDeleted": false,
    "userName": "john_doe"
  }
]
```

### Add Comment
```http
POST /api/tasks/5/comments
Content-Type: application/json
Authorization: Bearer {token}

{
  "commentText": "Need to review this with the team"
}

Response: 200 OK
{
  "message": "Comment added successfully"
}
```

### Update Comment
```http
PUT /api/tasks/5/comments/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "commentText": "Updated comment text"
}

Response: 200 OK
{
  "message": "Comment updated successfully"
}
```

### Delete Comment
```http
DELETE /api/tasks/5/comments/1
Authorization: Bearer {token}

Response: 200 OK
{
  "message": "Comment deleted successfully"
}
```

---

## ✅ Subtasks API

### Get Subtasks
```http
GET /api/tasks/5/subtasks
Authorization: Bearer {token}

Response: 200 OK
{
  "subtasks": [
    {
      "subtaskId": 1,
      "parentTaskId": 5,
      "title": "Design mockups",
      "isCompleted": false,
      "createdAt": "2024-01-15T10:30:00",
      "completedAt": null
    }
  ],
  "completionPercentage": 33.33
}
```

### Create Subtask
```http
POST /api/tasks/5/subtasks
Content-Type: application/json
Authorization: Bearer {token}

{
  "title": "Design mockups"
}

Response: 200 OK
{
  "message": "Subtask created successfully"
}
```

### Update Subtask Status
```http
PUT /api/tasks/5/subtasks/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "isCompleted": true
}

Response: 200 OK
{
  "message": "Subtask updated successfully"
}
```

### Delete Subtask
```http
DELETE /api/tasks/5/subtasks/1
Authorization: Bearer {token}

Response: 200 OK
{
  "message": "Subtask deleted successfully"
}
```

---

## 📊 Dashboard API

### Get Dashboard Stats
```http
GET /api/dashboard/stats
Authorization: Bearer {token}

Response: 200 OK
{
  "totalTasks": 25,
  "completedToday": 3,
  "overdueTasks": 2,
  "inProgressTasks": 8,
  "completionPercentage": 48.0,
  "totalProjects": 4
}
```

### Get Task Activity Log
```http
GET /api/dashboard/activity/5
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "historyId": 1,
    "taskId": 5,
    "userId": 1,
    "action": "Status Changed",
    "oldValue": "To Do",
    "newValue": "In Progress",
    "createdAt": "2024-01-15T10:30:00",
    "userName": "john_doe"
  }
]
```

---

## 🔐 Authentication

### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}

Response: 200 OK
{
  "message": "User registered successfully"
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "SecurePassword123!"
}

Response: 200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Login successful"
}
```

---

## 📋 Tasks API (Existing)

### Get All Tasks
```http
GET /api/tasks
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "taskId": 1,
    "title": "Complete project",
    "description": "Finish the website redesign",
    "status": "In Progress",
    "priority": "High",
    "category": "Development",
    "dueDate": "2024-02-15T00:00:00",
    "createdAt": "2024-01-15T10:30:00",
    "completedAt": null,
    "userId": 1,
    "isDeleted": false
  }
]
```

### Create Task
```http
POST /api/tasks
Content-Type: application/json
Authorization: Bearer {token}

{
  "title": "Complete project",
  "description": "Finish the website redesign",
  "status": "To Do",
  "priority": "High",
  "category": "Development",
  "dueDate": "2024-02-15T00:00:00"
}

Response: 200 OK
{
  "message": "Task created successfully"
}
```

### Update Task
```http
PUT /api/tasks/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "title": "Complete project",
  "description": "Finish the website redesign",
  "status": "In Progress",
  "priority": "High",
  "category": "Development",
  "dueDate": "2024-02-15T00:00:00"
}

Response: 200 OK
{
  "message": "Task updated successfully"
}
```

### Delete Task
```http
DELETE /api/tasks/1
Authorization: Bearer {token}

Response: 200 OK
{
  "message": "Task deleted successfully"
}
```

---

## 🎨 Color Reference

Use these colors in your frontend:

```json
{
  "primary": "#3B82F6",      // Blue
  "success": "#10B981",      // Green
  "danger": "#EF4444",       // Red
  "warning": "#F59E0B",      // Amber
  "info": "#06B6D4",         // Cyan
  "gray": "#6B7280"          // Gray
}
```

---

## ⚠️ Error Responses

### 400 Bad Request
```json
{
  "message": "Failed to create project"
}
```

### 401 Unauthorized
```json
{
  "message": "Invalid token"
}
```

### 404 Not Found
```json
{
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "message": "An error occurred"
}
```

---

## 🧪 Testing with Postman

1. **Register User**
   - POST /api/auth/register
   - Save response token

2. **Create Project**
   - POST /api/projects
   - Use token in Authorization header

3. **Create Task**
   - POST /api/tasks
   - Assign to project

4. **Add Comments**
   - POST /api/tasks/{id}/comments

5. **Create Subtasks**
   - POST /api/tasks/{id}/subtasks

6. **View Dashboard**
   - GET /api/dashboard/stats

---

## 📱 Frontend Integration Example

```javascript
// React example
const getProjects = async (token) => {
  const response = await fetch('/api/projects', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  return response.json();
};

const createProject = async (token, projectData) => {
  const response = await fetch('/api/projects', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(projectData)
  });
  return response.json();
};
```

---

**API Version**: 2.0
**Status**: ✅ Production Ready
**Last Updated**: 2024
