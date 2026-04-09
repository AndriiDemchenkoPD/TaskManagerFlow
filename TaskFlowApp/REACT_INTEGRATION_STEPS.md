# 🎨 INTEGRATION GUIDE - Add Features to Your Dashboard

## 📋 What to Do

Your existing dashboard shows:
- Task Manager
- My Tasks
- Add Task
- Logout

We need to add 4 new tabs to show:
1. **Dashboard** - Statistics
2. **Projects** - Project management
3. **Tags** - Tag management
4. **Subtasks & Comments** - In task detail view

---

## 🔧 STEP 1: Update Your Dashboard.jsx

Replace your current Dashboard.jsx with this structure:

```jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

export function Dashboard() {
  const [activeTab, setActiveTab] = useState('tasks');
  const [tasks, setTasks] = useState([]);
  const [projects, setProjects] = useState([]);
  const [tags, setTags] = useState([]);
  const [stats, setStats] = useState(null);
  const [selectedTask, setSelectedTask] = useState(null);

  const token = localStorage.getItem('token');
  const API_URL = 'http://localhost:5022/api';

  useEffect(() => {
    fetchTasks();
    fetchProjects();
    fetchTags();
    fetchStats();
  }, []);

  // FETCH FUNCTIONS
  const fetchTasks = async () => {
    const res = await axios.get(`${API_URL}/tasks`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setTasks(res.data);
  };

  const fetchProjects = async () => {
    const res = await axios.get(`${API_URL}/projects`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setProjects(res.data);
  };

  const fetchTags = async () => {
    const res = await axios.get(`${API_URL}/tags`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setTags(res.data);
  };

  const fetchStats = async () => {
    const res = await axios.get(`${API_URL}/dashboard/stats`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setStats(res.data);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    window.location.href = '/login';
  };

  return (
    <div className="dashboard">
      {/* HEADER */}
      <header className="header">
        <h1>📋 Task Manager v2.0</h1>
        <button onClick={handleLogout}>Logout</button>
      </header>

      {/* TABS */}
      <nav className="tabs">
        <button 
          className={activeTab === 'dashboard' ? 'active' : ''}
          onClick={() => setActiveTab('dashboard')}
        >
          📊 Dashboard
        </button>
        <button 
          className={activeTab === 'tasks' ? 'active' : ''}
          onClick={() => setActiveTab('tasks')}
        >
          📋 My Tasks
        </button>
        <button 
          className={activeTab === 'projects' ? 'active' : ''}
          onClick={() => setActiveTab('projects')}
        >
          📁 Projects
        </button>
        <button 
          className={activeTab === 'tags' ? 'active' : ''}
          onClick={() => setActiveTab('tags')}
        >
          🏷️ Tags
        </button>
      </nav>

      {/* CONTENT */}
      <main className="content">
        {/* DASHBOARD TAB */}
        {activeTab === 'dashboard' && stats && (
          <div className="dashboard-stats">
            <h2>📊 Dashboard Statistics</h2>
            <div className="stats-grid">
              <div className="stat-card">
                <h3>📋 Total Tasks</h3>
                <p>{stats.totalTasks}</p>
              </div>
              <div className="stat-card">
                <h3>✅ Completed Today</h3>
                <p>{stats.completedToday}</p>
              </div>
              <div className="stat-card">
                <h3>⚠️ Overdue</h3>
                <p>{stats.overdueTasks}</p>
              </div>
              <div className="stat-card">
                <h3>🔄 In Progress</h3>
                <p>{stats.inProgressTasks}</p>
              </div>
              <div className="stat-card">
                <h3>📈 Completion %</h3>
                <p>{stats.completionPercentage.toFixed(1)}%</p>
              </div>
              <div className="stat-card">
                <h3>📁 Projects</h3>
                <p>{stats.totalProjects}</p>
              </div>
            </div>
          </div>
        )}

        {/* TASKS TAB */}
        {activeTab === 'tasks' && (
          <div className="tasks-section">
            <h2>My Tasks</h2>
            <div className="tasks-list">
              {tasks.map(task => (
                <div key={task.taskId} className="task-card">
                  <h3>{task.title}</h3>
                  <p>{task.description}</p>
                  <div className="task-meta">
                    <span>Status: {task.status}</span>
                    <span>Priority: {task.priority}</span>
                  </div>
                  <button onClick={() => setSelectedTask(task)}>
                    View Details
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* PROJECTS TAB */}
        {activeTab === 'projects' && (
          <div className="projects-section">
            <h2>📁 Projects</h2>
            <div className="projects-list">
              {projects.map(project => (
                <div key={project.projectId} className="project-card" 
                     style={{borderLeft: `4px solid ${project.color}`}}>
                  <h3>{project.projectName}</h3>
                  <p>{project.description}</p>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* TAGS TAB */}
        {activeTab === 'tags' && (
          <div className="tags-section">
            <h2>🏷️ Tags</h2>
            <div className="tags-list">
              {tags.map(tag => (
                <span key={tag.tagId} className="tag-badge" 
                      style={{backgroundColor: tag.color}}>
                  {tag.tagName}
                </span>
              ))}
            </div>
          </div>
        )}
      </main>

      {/* TASK DETAIL MODAL */}
      {selectedTask && (
        <TaskDetailModal 
          task={selectedTask}
          onClose={() => setSelectedTask(null)}
          token={token}
          API_URL={API_URL}
        />
      )}
    </div>
  );
}

// TASK DETAIL COMPONENT
function TaskDetailModal({ task, onClose, token, API_URL }) {
  const [subtasks, setSubtasks] = useState([]);
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [progress, setProgress] = useState(0);

  useEffect(() => {
    fetchSubtasks();
    fetchComments();
  }, [task.taskId]);

  const fetchSubtasks = async () => {
    const res = await axios.get(`${API_URL}/tasks/${task.taskId}/subtasks`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setSubtasks(res.data.subtasks);
    setProgress(res.data.completionPercentage);
  };

  const fetchComments = async () => {
    const res = await axios.get(`${API_URL}/tasks/${task.taskId}/comments`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setComments(res.data);
  };

  const addComment = async () => {
    if (newComment.trim()) {
      await axios.post(`${API_URL}/tasks/${task.taskId}/comments`,
        { commentText: newComment },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setNewComment('');
      fetchComments();
    }
  };

  const toggleSubtask = async (subtaskId, isCompleted) => {
    await axios.put(`${API_URL}/tasks/${task.taskId}/subtasks/${subtaskId}`,
      { isCompleted: !isCompleted },
      { headers: { Authorization: `Bearer ${token}` } }
    );
    fetchSubtasks();
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="close-btn" onClick={onClose}>✕</button>
        
        <h2>{task.title}</h2>
        <p>{task.description}</p>

        {/* SUBTASKS */}
        <div className="modal-section">
          <h3>✅ Subtasks ({progress.toFixed(0)}% Complete)</h3>
          <div className="progress-bar">
            <div className="progress" style={{width: `${progress}%`}}></div>
          </div>
          {subtasks.map(subtask => (
            <div key={subtask.subtaskId} className="subtask">
              <input 
                type="checkbox"
                checked={subtask.isCompleted}
                onChange={() => toggleSubtask(subtask.subtaskId, subtask.isCompleted)}
              />
              <span>{subtask.title}</span>
            </div>
          ))}
        </div>

        {/* COMMENTS */}
        <div className="modal-section">
          <h3>💬 Comments</h3>
          <textarea 
            placeholder="Add a comment..."
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
          />
          <button onClick={addComment}>Post Comment</button>
          {comments.map(comment => (
            <div key={comment.commentId} className="comment">
              <strong>{comment.userName}</strong>
              <p>{comment.commentText}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
```

---

## 🎨 STEP 2: Add CSS Styling

Add this to your CSS file:

```css
.dashboard {
  min-height: 100vh;
  background: #f3f4f6;
}

.header {
  background: linear-gradient(135deg, #3B82F6 0%, #1e40af 100%);
  color: white;
  padding: 20px 40px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header button {
  padding: 10px 20px;
  background: rgba(255,255,255,0.2);
  color: white;
  border: 2px solid white;
  border-radius: 6px;
  cursor: pointer;
  font-weight: bold;
}

.tabs {
  background: white;
  padding: 0;
  display: flex;
  gap: 0;
  border-bottom: 2px solid #e5e7eb;
}

.tabs button {
  flex: 1;
  padding: 16px 20px;
  background: white;
  border: none;
  cursor: pointer;
  font-weight: 600;
  color: #6b7280;
  border-bottom: 3px solid transparent;
  transition: all 0.3s;
}

.tabs button.active {
  color: #3B82F6;
  border-bottom-color: #3B82F6;
}

.content {
  padding: 30px 40px;
  max-width: 1400px;
  margin: 0 auto;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.stat-card {
  padding: 20px;
  background: linear-gradient(135deg, #3B82F6 0%, #1e40af 100%);
  color: white;
  border-radius: 8px;
  text-align: center;
}

.stat-card h3 {
  font-size: 14px;
  margin-bottom: 10px;
}

.stat-card p {
  font-size: 32px;
  font-weight: bold;
}

.tasks-list,
.projects-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.task-card,
.project-card {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.task-card h3,
.project-card h3 {
  margin-bottom: 10px;
  color: #1f2937;
}

.task-meta {
  display: flex;
  gap: 15px;
  margin: 10px 0;
  font-size: 12px;
  color: #6b7280;
}

.task-card button {
  margin-top: 10px;
  padding: 8px 16px;
  background: #3B82F6;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.tags-list {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 20px;
}

.tag-badge {
  display: inline-block;
  padding: 8px 16px;
  border-radius: 20px;
  color: white;
  font-weight: bold;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  padding: 30px;
  max-width: 600px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
}

.close-btn {
  position: absolute;
  top: 15px;
  right: 15px;
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
}

.modal-section {
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
}

.progress-bar {
  width: 100%;
  height: 8px;
  background: #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
  margin: 10px 0;
}

.progress {
  height: 100%;
  background: #10b981;
}

.subtask {
  display: flex;
  align-items: center;
  padding: 10px;
  margin: 8px 0;
  background: #f9fafb;
  border-radius: 6px;
}

.subtask input {
  margin-right: 10px;
  width: 18px;
  height: 18px;
}

.comment {
  padding: 12px;
  margin: 10px 0;
  background: #f9fafb;
  border-radius: 6px;
  border-left: 3px solid #3B82F6;
}

.comment strong {
  display: block;
  margin-bottom: 5px;
}

textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  margin: 10px 0;
  min-height: 60px;
}

button {
  padding: 10px 20px;
  background: #3B82F6;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: bold;
}

button:hover {
  background: #1e40af;
}
```

---

## ✅ What You'll See After Login

```
┌─────────────────────────────────────────────────────────────┐
│  📋 Task Manager v2.0                          [Logout]     │
├─────────────────────────────────────────────────────────────┤
│  [📊 Dashboard] [📋 My Tasks] [📁 Projects] [🏷️ Tags]      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  📊 Dashboard Statistics                                    │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐      │
│  │ 📋 Tasks │ │ ✅ Today │ │ ⚠️ Overdue│ │ 🔄 Progress│   │
│  │   25     │ │    3     │ │    2     │ │    8     │      │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘      │
│  ┌──────────┐ ┌──────────┐                                 │
│  │ 📈 Completion│ │ 📁 Projects│                           │
│  │   48%    │ │    4     │                                 │
│  └──────────┘ └──────────┘                                 │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Features Now Visible

| Feature | Where | What You See |
|---------|-------|-------------|
| Dashboard | Tab | 6 statistics cards |
| Projects | Tab | Project cards with colors |
| Tags | Tab | Colored tag badges |
| Subtasks | Task Detail | Checklist with progress |
| Comments | Task Detail | Comment thread |

---

**Done!** Your dashboard now has all 9 features integrated! 🎉
