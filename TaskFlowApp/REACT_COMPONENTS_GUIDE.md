// 🎨 REACT COMPONENTS - UI IMPLEMENTATION GUIDE

// ============================================
// 1. PROJECTS COMPONENT
// ============================================

// File: task-manager-ui/src/components/Projects.jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

export function Projects() {
  const [projects, setProjects] = useState([]);
  const [newProject, setNewProject] = useState({ projectName: '', color: '#3B82F6' });

  useEffect(() => {
    fetchProjects();
  }, []);

  const fetchProjects = async () => {
    const token = localStorage.getItem('token');
    const res = await axios.get('http://localhost:5022/api/projects', {
      headers: { Authorization: `Bearer ${token}` }
    });
    setProjects(res.data);
  };

  const createProject = async () => {
    const token = localStorage.getItem('token');
    await axios.post('http://localhost:5022/api/projects', newProject, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setNewProject({ projectName: '', color: '#3B82F6' });
    fetchProjects();
  };

  return (
    <div className="projects-container">
      <h2>📁 Projects</h2>
      
      {/* Create Project Form */}
      <div className="create-project">
        <input 
          placeholder="Project name"
          value={newProject.projectName}
          onChange={(e) => setNewProject({...newProject, projectName: e.target.value})}
        />
        <input 
          type="color"
          value={newProject.color}
          onChange={(e) => setNewProject({...newProject, color: e.target.value})}
        />
        <button onClick={createProject}>Create Project</button>
      </div>

      {/* Projects List */}
      <div className="projects-list">
        {projects.map(project => (
          <div key={project.projectId} className="project-card" style={{borderLeft: `4px solid ${project.color}`}}>
            <h3>{project.projectName}</h3>
            <p>{project.description}</p>
          </div>
        ))}
      </div>
    </div>
  );
}

// ============================================
// 2. TAGS COMPONENT
// ============================================

// File: task-manager-ui/src/components/Tags.jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

export function Tags() {
  const [tags, setTags] = useState([]);
  const [newTag, setNewTag] = useState({ tagName: '', color: '#10B981' });

  useEffect(() => {
    fetchTags();
  }, []);

  const fetchTags = async () => {
    const token = localStorage.getItem('token');
    const res = await axios.get('http://localhost:5022/api/tags', {
      headers: { Authorization: `Bearer ${token}` }
    });
    setTags(res.data);
  };

  const createTag = async () => {
    const token = localStorage.getItem('token');
    await axios.post('http://localhost:5022/api/tags', newTag, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setNewTag({ tagName: '', color: '#10B981' });
    fetchTags();
  };

  return (
    <div className="tags-container">
      <h2>🏷️ Tags</h2>
      
      {/* Create Tag Form */}
      <div className="create-tag">
        <input 
          placeholder="Tag name"
          value={newTag.tagName}
          onChange={(e) => setNewTag({...newTag, tagName: e.target.value})}
        />
        <input 
          type="color"
          value={newTag.color}
          onChange={(e) => setNewTag({...newTag, color: e.target.value})}
        />
        <button onClick={createTag}>Create Tag</button>
      </div>

      {/* Tags List */}
      <div className="tags-list">
        {tags.map(tag => (
          <span key={tag.tagId} className="tag-badge" style={{backgroundColor: tag.color}}>
            {tag.tagName}
          </span>
        ))}
      </div>
    </div>
  );
}

// ============================================
// 3. COMMENTS COMPONENT
// ============================================

// File: task-manager-ui/src/components/TaskComments.jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

export function TaskComments({ taskId }) {
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');

  useEffect(() => {
    fetchComments();
  }, [taskId]);

  const fetchComments = async () => {
    const token = localStorage.getItem('token');
    const res = await axios.get(`http://localhost:5022/api/tasks/${taskId}/comments`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setComments(res.data);
  };

  const addComment = async () => {
    const token = localStorage.getItem('token');
    await axios.post(`http://localhost:5022/api/tasks/${taskId}/comments`, 
      { commentText: newComment },
      { headers: { Authorization: `Bearer ${token}` } }
    );
    setNewComment('');
    fetchComments();
  };

  return (
    <div className="comments-container">
      <h3>💬 Comments</h3>
      
      {/* Add Comment */}
      <div className="add-comment">
        <textarea 
          placeholder="Add a comment..."
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
        />
        <button onClick={addComment}>Post Comment</button>
      </div>

      {/* Comments List */}
      <div className="comments-list">
        {comments.map(comment => (
          <div key={comment.commentId} className="comment">
            <strong>{comment.userName}</strong>
            <p>{comment.commentText}</p>
            <small>{new Date(comment.createdAt).toLocaleString()}</small>
          </div>
        ))}
      </div>
    </div>
  );
}

// ============================================
// 4. SUBTASKS COMPONENT
// ============================================

// File: task-manager-ui/src/components/Subtasks.jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

export function Subtasks({ taskId }) {
  const [subtasks, setSubtasks] = useState([]);
  const [progress, setProgress] = useState(0);
  const [newSubtask, setNewSubtask] = useState('');

  useEffect(() => {
    fetchSubtasks();
  }, [taskId]);

  const fetchSubtasks = async () => {
    const token = localStorage.getItem('token');
    const res = await axios.get(`http://localhost:5022/api/tasks/${taskId}/subtasks`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setSubtasks(res.data.subtasks);
    setProgress(res.data.completionPercentage);
  };

  const createSubtask = async () => {
    const token = localStorage.getItem('token');
    await axios.post(`http://localhost:5022/api/tasks/${taskId}/subtasks`,
      { title: newSubtask },
      { headers: { Authorization: `Bearer ${token}` } }
    );
    setNewSubtask('');
    fetchSubtasks();
  };

  const toggleSubtask = async (subtaskId, isCompleted) => {
    const token = localStorage.getItem('token');
    await axios.put(`http://localhost:5022/api/tasks/${taskId}/subtasks/${subtaskId}`,
      { isCompleted: !isCompleted },
      { headers: { Authorization: `Bearer ${token}` } }
    );
    fetchSubtasks();
  };

  return (
    <div className="subtasks-container">
      <h3>✅ Subtasks</h3>
      
      {/* Progress Bar */}
      <div className="progress-bar">
        <div className="progress" style={{width: `${progress}%`}}></div>
      </div>
      <p>{progress.toFixed(0)}% Complete</p>

      {/* Add Subtask */}
      <div className="add-subtask">
        <input 
          placeholder="Add subtask..."
          value={newSubtask}
          onChange={(e) => setNewSubtask(e.target.value)}
        />
        <button onClick={createSubtask}>Add</button>
      </div>

      {/* Subtasks List */}
      <div className="subtasks-list">
        {subtasks.map(subtask => (
          <div key={subtask.subtaskId} className="subtask">
            <input 
              type="checkbox"
              checked={subtask.isCompleted}
              onChange={() => toggleSubtask(subtask.subtaskId, subtask.isCompleted)}
            />
            <span style={{textDecoration: subtask.isCompleted ? 'line-through' : 'none'}}>
              {subtask.title}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}

// ============================================
// 5. DASHBOARD COMPONENT
// ============================================

// File: task-manager-ui/src/components/Dashboard.jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

export function Dashboard() {
  const [stats, setStats] = useState(null);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async () => {
    const token = localStorage.getItem('token');
    const res = await axios.get('http://localhost:5022/api/dashboard/stats', {
      headers: { Authorization: `Bearer ${token}` }
    });
    setStats(res.data);
  };

  if (!stats) return <div>Loading...</div>;

  return (
    <div className="dashboard-container">
      <h2>📊 Dashboard</h2>
      
      <div className="stats-grid">
        <div className="stat-card">
          <h3>📋 Total Tasks</h3>
          <p className="stat-value">{stats.totalTasks}</p>
        </div>

        <div className="stat-card">
          <h3>✅ Completed Today</h3>
          <p className="stat-value">{stats.completedToday}</p>
        </div>

        <div className="stat-card">
          <h3>⚠️ Overdue</h3>
          <p className="stat-value">{stats.overdueTasks}</p>
        </div>

        <div className="stat-card">
          <h3>🔄 In Progress</h3>
          <p className="stat-value">{stats.inProgressTasks}</p>
        </div>

        <div className="stat-card">
          <h3>📈 Completion %</h3>
          <p className="stat-value">{stats.completionPercentage.toFixed(1)}%</p>
        </div>

        <div className="stat-card">
          <h3>📁 Projects</h3>
          <p className="stat-value">{stats.totalProjects}</p>
        </div>
      </div>
    </div>
  );
}

// ============================================
// 6. ACTIVITY LOG COMPONENT
// ============================================

// File: task-manager-ui/src/components/ActivityLog.jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

export function ActivityLog({ taskId }) {
  const [activity, setActivity] = useState([]);

  useEffect(() => {
    fetchActivity();
  }, [taskId]);

  const fetchActivity = async () => {
    const token = localStorage.getItem('token');
    const res = await axios.get(`http://localhost:5022/api/dashboard/activity/${taskId}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    setActivity(res.data);
  };

  return (
    <div className="activity-log">
      <h3>📜 Activity Log</h3>
      
      <div className="activity-list">
        {activity.map(entry => (
          <div key={entry.historyId} className="activity-entry">
            <strong>{entry.userName}</strong> - {entry.action}
            <p>Old: {entry.oldValue} → New: {entry.newValue}</p>
            <small>{new Date(entry.createdAt).toLocaleString()}</small>
          </div>
        ))}
      </div>
    </div>
  );
}

// ============================================
// CSS STYLING
// ============================================

/* File: task-manager-ui/src/styles/features.css */

.projects-container, .tags-container, .comments-container, 
.subtasks-container, .dashboard-container, .activity-log {
  padding: 20px;
  margin: 20px 0;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.project-card {
  padding: 15px;
  margin: 10px 0;
  background: #f9fafb;
  border-radius: 6px;
  border-left: 4px solid #3B82F6;
}

.tag-badge {
  display: inline-block;
  padding: 6px 12px;
  margin: 5px;
  border-radius: 20px;
  color: white;
  font-size: 12px;
  font-weight: bold;
}

.comment {
  padding: 12px;
  margin: 10px 0;
  background: #f3f4f6;
  border-radius: 6px;
  border-left: 3px solid #3B82F6;
}

.subtask {
  display: flex;
  align-items: center;
  padding: 10px;
  margin: 8px 0;
  background: #f9fafb;
  border-radius: 6px;
}

.subtask input[type="checkbox"] {
  margin-right: 10px;
  width: 18px;
  height: 18px;
  cursor: pointer;
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
  background: #10B981;
  transition: width 0.3s ease;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin: 20px 0;
}

.stat-card {
  padding: 20px;
  background: linear-gradient(135deg, #3B82F6 0%, #1e40af 100%);
  color: white;
  border-radius: 8px;
  text-align: center;
}

.stat-value {
  font-size: 32px;
  font-weight: bold;
  margin: 10px 0;
}

.activity-entry {
  padding: 12px;
  margin: 10px 0;
  background: #f3f4f6;
  border-radius: 6px;
  border-left: 3px solid #f59e0b;
}

button {
  padding: 10px 20px;
  background: #3B82F6;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: bold;
  transition: background 0.3s;
}

button:hover {
  background: #1e40af;
}

input, textarea {
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-family: inherit;
  margin: 5px;
}

textarea {
  width: 100%;
  min-height: 80px;
  resize: vertical;
}

// ============================================
// INTEGRATION IN MAIN DASHBOARD
// ============================================

// File: task-manager-ui/src/pages/Dashboard.jsx
import { Projects } from '../components/Projects';
import { Tags } from '../components/Tags';
import { Dashboard as DashboardStats } from '../components/Dashboard';
import { TaskComments } from '../components/TaskComments';
import { Subtasks } from '../components/Subtasks';
import { ActivityLog } from '../components/ActivityLog';

export function MainDashboard() {
  const [selectedTaskId, setSelectedTaskId] = useState(1);

  return (
    <div className="main-dashboard">
      <DashboardStats />
      <Projects />
      <Tags />
      
      {/* Task Details Section */}
      <div className="task-details">
        <Subtasks taskId={selectedTaskId} />
        <TaskComments taskId={selectedTaskId} />
        <ActivityLog taskId={selectedTaskId} />
      </div>
    </div>
  );
}
