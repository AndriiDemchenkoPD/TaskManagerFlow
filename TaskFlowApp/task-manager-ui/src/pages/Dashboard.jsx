import { useEffect, useRef, useState } from 'react'
import {
	getTasks,
	addTask,
	deleteTask,
	getUserProfile,
	updateUserProfile
} from '../services/api'
import axios from 'axios'

/* 
   THEME TOKENS  (updated live via CSS vars  no re-render needed
   for colour transitions; only boolean `dark` drives style objects)
 */
const dark = {
	bg: '#080810',
	surface: '#0e0d1c',
	card: '#13122a',
	cardHover: '#1b1938',
	border: 'rgba(255,255,255,0.075)',
	accent: '#7c6af7',
	accentLt: '#a89cf8',
	text: '#eeeaff',
	muted: 'rgba(238,234,255,0.52)',
	faint: 'rgba(238,234,255,0.28)',
	success: '#22d3a0',
	warning: '#fbbf24',
	danger: '#f87171',
	info: '#60a5fa',
	sidebar: '#0b0a1a',
	inputBg: 'rgba(255,255,255,0.045)',
	statBg: 'rgba(255,255,255,0.025)',
	statBorder: 'rgba(255,255,255,0.04)',
	logoutBorder: 'rgba(248,113,113,0.2)',
	logoutBg: 'rgba(248,113,113,0.06)',
	logoutColor: '#f87171',
	overlay: 'rgba(0,0,0,0.6)'
}

const light = {
	bg: '#f3f7fb',
	surface: '#ffffff',
	card: '#ffffff',
	cardHover: '#eef4ff',
	border: 'rgba(15,23,42,0.11)',
	accent: '#0f6fff',
	accentLt: '#3f8cff',
	text: '#0f172a',
	muted: '#475569',
	faint: '#64748b',
	success: '#0f9d74',
	warning: '#c27a08',
	danger: '#cf2f2f',
	info: '#155eef',
	sidebar: '#f8fbff',
	inputBg: '#f7fafe',
	statBg: 'rgba(15,111,255,0.05)',
	statBorder: 'rgba(15,23,42,0.06)',
	logoutBorder: 'rgba(207,47,47,0.22)',
	logoutBg: 'rgba(207,47,47,0.06)',
	logoutColor: '#b42318',
	overlay: 'rgba(15,23,42,0.3)'
}

/*  status palette  */
const statusPalette = C => ({
	Completed: { bg: `${C.success}1e`, text: C.success },
	'In Progress': { bg: `${C.warning}1e`, text: C.warning },
	Pending: { bg: `${C.accent}1e`, text: C.accentLt }
})

/*  NEW: Priority palette  */
const priorityPalette = C => ({
	High: { bg: `${C.danger}1e`, text: C.danger, icon: '' },
	Medium: { bg: `${C.warning}1e`, text: C.warning, icon: '' },
	Low: { bg: `${C.info}1e`, text: C.info, icon: '' }
})

/*  nav items  */
const NAV = [
	{ id: 'list', icon: '', label: 'My Tasks' },
	{ id: 'kanban', icon: '', label: 'Kanban' },
	{ id: 'add', icon: '+', label: 'New Task' },
	{ id: 'dashboard', icon: '', label: 'Overview' },
	{ id: 'projects', icon: '', label: 'Projects' },
	{ id: 'tags', icon: '', label: 'Tags' }
]

const FONT_DISPLAY = "'Outfit', sans-serif"
const FONT_BODY = "'Plus Jakarta Sans', sans-serif"
const TASK_DRAFT_KEY = 'taskflow.taskDraft'
const DUE_SOON_MINUTES = 30

export default function Dashboard() {
	/*  all original state (untouched)  */
	const [tasks, setTasks] = useState([])
	const [loading, setLoading] = useState(true)
	const [view, setView] = useState('list')
	const [selectedTask, setSelectedTask] = useState(null)
	const [title, setTitle] = useState('')
	const [description, setDescription] = useState('')
	const [status, setStatus] = useState('Pending')
	const [projects, setProjects] = useState([])
	const [tags, setTags] = useState([])
	const [stats, setStats] = useState(null)
	const [subtasks, setSubtasks] = useState([])
	const [newSubtaskTitle, setNewSubtaskTitle] = useState('')
	const [comments, setComments] = useState([])
	const [newComment, setNewComment] = useState('')
	const [showProfileMenu, setShowProfileMenu] = useState(false)
	const [showProfileModal, setShowProfileModal] = useState(false)
	const [userProfile, setUserProfile] = useState(null)
	const [profileForm, setProfileForm] = useState({ username: '', email: '' })
	const [dragTaskId, setDragTaskId] = useState(null)
	const [dragOverStatus, setDragOverStatus] = useState(null)

	/*  NEW: Search & Filter State  */
	const [searchQuery, setSearchQuery] = useState('')
	const [filterStatus, setFilterStatus] = useState('All')
	const [filterPriority, setFilterPriority] = useState('All')
	const [sortBy, setSortBy] = useState('created')

	/*  NEW: Form fields for priority and due date  */
	const [priority, setPriority] = useState('Medium')
	const [dueDate, setDueDate] = useState('')
	const [dueTime, setDueTime] = useState('')
	const [newProjectName, setNewProjectName] = useState('')
	const [newProjectDescription, setNewProjectDescription] = useState('')
	const [newProjectColor, setNewProjectColor] = useState('#3B82F6')
	const [newTagName, setNewTagName] = useState('')
	const [newTagColor, setNewTagColor] = useState('#10B981')
	const [projectId, setProjectId] = useState('')
	const [activeProjectId, setActiveProjectId] = useState('all')
	const [taskTags, setTaskTags] = useState([])
	const [selectedTagIds, setSelectedTagIds] = useState([])
	const [selectedTagId, setSelectedTagId] = useState('')
	const notifiedRemindersRef = useRef(new Set())

	/*  theme (UI only)  */
	const [isDark, setIsDark] = useState(
		() => (localStorage.getItem('theme') || 'dark') === 'dark'
	)
	const C = isDark ? dark : light
	const SP = statusPalette(C)
	const PP = priorityPalette(C)

	/*  NEW: Helper functions  */
	const isOverdue = dueDate => {
		if (!dueDate) return false
		return (
			new Date(dueDate) < new Date() &&
			new Date(dueDate).toDateString() !== new Date().toDateString()
		)
	}

	const formatDueTime = dueTimeValue => {
		if (!dueTimeValue) return ''
		const asString = String(dueTimeValue)
		return asString.length >= 5 ? asString.slice(0, 5) : asString
	}

	const normalizeDueTimeForApi = dueTimeValue => {
		if (!dueTimeValue) return null
		const asString = String(dueTimeValue).trim()
		if (!asString) return null
		return /^\d{2}:\d{2}$/.test(asString) ? `${asString}:00` : asString
	}

	const getTaskDueAt = task => {
		if (!task?.dueDate) return null
		const due = new Date(task.dueDate)
		if (Number.isNaN(due.getTime())) return null

		const dueTimeValue = formatDueTime(task.dueTime)
		if (/^\d{2}:\d{2}$/.test(dueTimeValue)) {
			const [hours, minutes] = dueTimeValue.split(':').map(Number)
			due.setHours(hours, minutes, 0, 0)
		}

		return due
	}

	const canCompleteTask = async taskId => {
		try {
			const res = await axios.get(`${API_URL}/tasks/${taskId}/subtasks`, {
				headers
			})
			const list = Array.isArray(res?.data?.subtasks) ? res.data.subtasks : []
			if (list.length === 0) {
				return true
			}
			const completion = Number(res?.data?.completionPercentage ?? 100)
			if (completion < 100) {
				alert('Спочатку завершіть усі підзадачі.')
				return false
			}
			return true
		} catch {
			// If subtasks are unavailable, do not block completion.
			return true
		}
	}

	const notifyDueSoonTasks = currentTasks => {
		if (typeof window === 'undefined' || typeof Notification === 'undefined') {
			return
		}

		if (Notification.permission !== 'granted') {
			return
		}

		const now = new Date()
		const limit = new Date(now.getTime() + DUE_SOON_MINUTES * 60 * 1000)

		currentTasks.forEach(task => {
			if (task.status === 'Completed') return
			const dueAt = getTaskDueAt(task)
			if (!dueAt) return
			if (dueAt < now || dueAt > limit) return

			const dedupeKey = `${task.taskId}-${dueAt.toISOString()}`
			if (notifiedRemindersRef.current.has(dedupeKey)) return

			notifiedRemindersRef.current.add(dedupeKey)
			new Notification('Нагадування по задачі', {
				body: `"${task.title}" має дедлайн о ${dueAt.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`
			})
		})
	}

	const getFilteredAndSortedTasks = () => {
		let filtered = tasks.filter(task => {
			const projectMatch =
				activeProjectId === 'all'
					? true
					: Number(task.projectId || 0) === Number(activeProjectId)
			const matchesSearch =
				!searchQuery ||
				task.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
				(task.description &&
					task.description.toLowerCase().includes(searchQuery.toLowerCase()))

			const matchesStatus =
				filterStatus === 'All' || task.status === filterStatus
			const matchesPriority =
				filterPriority === 'All' || task.priority === filterPriority

			return projectMatch && matchesSearch && matchesStatus && matchesPriority
		})

		// Sort tasks
		filtered.sort((a, b) => {
			switch (sortBy) {
				case 'priority': {
					const priorityOrder = { High: 3, Medium: 2, Low: 1 }
					return (
						(priorityOrder[b.priority] || 0) - (priorityOrder[a.priority] || 0)
					)
				}
				case 'dueDate':
					if (!a.dueDate && !b.dueDate) return 0
					if (!a.dueDate) return 1
					if (!b.dueDate) return -1
					return new Date(a.dueDate) - new Date(b.dueDate)
				case 'created':
				default:
					return new Date(b.createdAt || 0) - new Date(a.createdAt || 0)
			}
		})

		return filtered
	}

	const getProjectScopedTasks = inputTasks => {
		if (activeProjectId === 'all') return inputTasks
		return inputTasks.filter(
			t => Number(t.projectId || 0) === Number(activeProjectId)
		)
	}

	const getTaskTagObjects = task => {
		if (!task?.tagIds?.length || tags.length === 0) return []
		const tagIdSet = new Set(task.tagIds.map(tagId => String(tagId)))
		return tags.filter(tag => tagIdSet.has(String(tag.tagId)))
	}

	const toggleTheme = () => {
		setIsDark(d => {
			const next = !d
			const val = next ? 'dark' : 'light'
			document.documentElement.setAttribute('data-theme', val)
			localStorage.setItem('theme', val)
			return next
		})
	}

	/* keep <html> in sync on mount */
	useEffect(() => {
		document.documentElement.setAttribute(
			'data-theme',
			isDark ? 'dark' : 'light'
		)
	}, [isDark])

	/* Close profile menu when clicking outside */
	useEffect(() => {
		const handleClickOutside = e => {
			if (showProfileMenu && !e.target.closest('[data-profile-menu]')) {
				setShowProfileMenu(false)
			}
		}
		document.addEventListener('click', handleClickOutside)
		return () => document.removeEventListener('click', handleClickOutside)
	}, [showProfileMenu])

	/*  original API logic (untouched)  */
	const token = localStorage.getItem('token')
	const API_URL = 'http://localhost:5022/api'
	const headers = { Authorization: `Bearer ${token}` }

	async function loadTasks() {
		try {
			const data = await getTasks()
			setTasks(data)
		} catch (err) {
			console.error(err)
		} finally {
			setLoading(false)
		}
	}

	useEffect(() => {
		loadTasks()
		loadUserProfile()
		loadProjectsOnly()
		loadTagsOnly()
		if (
			typeof Notification !== 'undefined' &&
			Notification.permission === 'default'
		) {
			Notification.requestPermission().catch(() => {})
		}
	}, [])

	useEffect(() => {
		if (view === 'add' || view === 'edit') {
			const draft = {
				title,
				description,
				status,
				priority,
				dueDate,
				dueTime,
				projectId,
				selectedTagIds
			}
			localStorage.setItem(TASK_DRAFT_KEY, JSON.stringify(draft))
		}
	}, [
		view,
		title,
		description,
		status,
		priority,
		dueDate,
		dueTime,
		projectId,
		selectedTagIds
	])

	useEffect(() => {
		if (view !== 'add') return
		if (
			title ||
			description ||
			dueDate ||
			dueTime ||
			projectId ||
			selectedTagIds.length > 0
		) {
			return
		}

		const raw = localStorage.getItem(TASK_DRAFT_KEY)
		if (!raw) return

		try {
			const draft = JSON.parse(raw)
			setTitle(draft.title || '')
			setDescription(draft.description || '')
			setStatus(draft.status || 'Pending')
			setPriority(draft.priority || 'Medium')
			setDueDate(draft.dueDate || '')
			setDueTime(draft.dueTime || '')
			setProjectId(draft.projectId || '')
			setSelectedTagIds(
				Array.isArray(draft.selectedTagIds) ? draft.selectedTagIds : []
			)
		} catch {
			localStorage.removeItem(TASK_DRAFT_KEY)
		}
	}, [view])

	useEffect(() => {
		notifyDueSoonTasks(tasks)
	}, [tasks])

	useEffect(() => {
		const intervalId = setInterval(async () => {
			await loadTasks()
			if (view === 'dashboard') {
				await loadDashboardStatsOnly()
			}
		}, 15000)

		return () => clearInterval(intervalId)
	}, [view])

	const loadProjectsOnly = async () => {
		try {
			const res = await axios.get(`${API_URL}/projects`, { headers })
			setProjects(res.data)
			return res.data
		} catch {
			return []
		}
	}

	const loadTagsOnly = async () => {
		try {
			const res = await axios.get(`${API_URL}/tags`, { headers })
			setTags(res.data)
			return res.data
		} catch {
			return []
		}
	}

	const loadUserProfile = async () => {
		try {
			const profile = await getUserProfile()
			console.log('Profile loaded:', profile)
			setUserProfile(profile)
			setProfileForm({ username: profile.username, email: profile.email })
		} catch (err) {
			console.error('Failed to load profile:', err)
			// Set default values if API fails
			setUserProfile({ username: 'User', email: 'user@example.com' })
			setProfileForm({ username: 'User', email: 'user@example.com' })
		}
	}

	const handleUpdateProfile = async e => {
		e.preventDefault()
		try {
			await updateUserProfile(profileForm)
			await loadUserProfile()
			setShowProfileModal(false)
			alert('Profile updated successfully!')
		} catch (err) {
			console.error('Failed to update profile:', err)
			alert('Failed to update profile: ' + err.message)
		}
	}

	const handleAddTask = async e => {
		e.preventDefault()
		try {
			await addTask({
				title,
				description,
				status,
				priority,
				category: 'General',
				dueDate: dueDate || new Date().toISOString(),
				dueTime: normalizeDueTimeForApi(dueTime),
				projectId: projectId ? Number(projectId) : null,
				tagIds: selectedTagIds.map(Number)
			})
			await loadTasks()
			setActiveProjectId('all')
			setSearchQuery('')
			setFilterStatus('All')
			setFilterPriority('All')
			setSortBy('created')
			resetTaskForm()
			localStorage.removeItem(TASK_DRAFT_KEY)
			setView('list')
		} catch (err) {
			console.error('Add task error:', err.response?.data || err.message)
			alert(
				'Failed to add task: ' + (err.response?.data?.message || err.message)
			)
		}
	}

	const resetTaskForm = () => {
		setTitle('')
		setDescription('')
		setStatus('Pending')
		setPriority('Medium')
		setDueDate('')
		setDueTime('')
		setProjectId('')
		setSelectedTagIds([])
	}

	const startEdit = async task => {
		setSelectedTask(task)
		setTitle(task.title)
		setDescription(task.description)
		setStatus(task.status)
		setPriority(task.priority || 'Medium')
		setDueDate(
			task.dueDate ? new Date(task.dueDate).toISOString().split('T')[0] : ''
		)
		setDueTime(formatDueTime(task.dueTime))
		setProjectId(task.projectId ? String(task.projectId) : '')
		const tagsForTask = await fetchTaskTags(task.taskId)
		setSelectedTagIds(tagsForTask.map(tag => String(tag.tagId)))
		setView('edit')
	}

	const handleEditTask = async e => {
		e.preventDefault()
		try {
			await axios.put(
				`${API_URL}/task/${selectedTask.taskId}`,
				{
					...selectedTask,
					title,
					description,
					status,
					priority,
					category: selectedTask.category || 'General',
					dueDate: dueDate || new Date().toISOString(),
					dueTime: normalizeDueTimeForApi(dueTime),
					projectId: projectId ? Number(projectId) : null,
					tagIds: selectedTagIds.map(Number)
				},
				{ headers }
			)
			await loadTasks()
			resetTaskForm()
			localStorage.removeItem(TASK_DRAFT_KEY)
			setView('list')
		} catch (err) {
			console.error('Edit task error:', err.response?.data || err.message)
			alert('Failed to update task.')
		}
	}

	const removeTask = async id => {
		try {
			await deleteTask(id)
			await loadTasks()
		} catch (err) {
			console.error(err)
		}
	}

	const markTaskCompleted = async task => {
		if (task.status === 'Completed') return
		const allowed = await canCompleteTask(task.taskId)
		if (!allowed) return

		try {
			await axios.put(
				`${API_URL}/task/${task.taskId}`,
				{
					...task,
					status: 'Completed',
					dueDate: task.dueDate || new Date().toISOString(),
					dueTime: task.dueTime || null,
					priority: task.priority || 'Medium',
					category: task.category || 'General',
					projectId: task.projectId ?? null,
					tagIds: Array.isArray(task.tagIds) ? task.tagIds : []
				},
				{ headers }
			)

			await loadTasks()
			if (view === 'dashboard') {
				await loadDashboardStatsOnly()
			}
		} catch (err) {
			console.error('Complete task error:', err.response?.data || err.message)
			alert('Не вдалося позначити задачу як виконану.')
		}
	}

	const fetchProjects = async () => {
		try {
			await loadProjectsOnly()
			setView('projects')
		} catch (err) {
			console.error('Projects error:', err.response?.data || err.message)
			alert('Failed to load projects. Make sure database tables exist.')
		}
	}

	const fetchTags = async () => {
		try {
			await loadTagsOnly()
			setView('tags')
		} catch (err) {
			console.error('Tags error:', err.response?.data || err.message)
			alert('Failed to load tags. Make sure database tables exist.')
		}
	}

	const createProject = async e => {
		e.preventDefault()
		if (!newProjectName.trim()) return
		try {
			await axios.post(
				`${API_URL}/projects`,
				{
					projectName: newProjectName.trim(),
					description: newProjectDescription.trim(),
					color: newProjectColor
				},
				{ headers }
			)
			setNewProjectName('')
			setNewProjectDescription('')
			setNewProjectColor('#3B82F6')
			await loadProjectsOnly()
			setView('projects')
		} catch (err) {
			console.error('Create project error:', err.response?.data || err.message)
			alert(err.response?.data?.message || 'Failed to create project.')
		}
	}

	const createTag = async e => {
		e.preventDefault()
		if (!newTagName.trim()) return
		try {
			await axios.post(
				`${API_URL}/tags`,
				{
					tagName: newTagName.trim(),
					color: newTagColor
				},
				{ headers }
			)
			setNewTagName('')
			setNewTagColor('#10B981')
			await loadTagsOnly()
			setView('tags')
		} catch (err) {
			console.error('Create tag error:', err.response?.data || err.message)
			alert(err.response?.data?.message || 'Failed to create tag.')
		}
	}

	const loadDashboardStatsOnly = async () => {
		try {
			const res = await axios.get(`${API_URL}/dashboard/stats`, { headers })
			setStats(res.data)
		} catch (err) {
			console.error('Dashboard error:', err.response?.data || err.message)
		}
	}

	const fetchStats = async () => {
		await loadDashboardStatsOnly()
		setView('dashboard')
	}

	const fetchSubtasks = async taskId => {
		try {
			const res = await axios.get(`${API_URL}/tasks/${taskId}/subtasks`, {
				headers
			})
			setSubtasks(res.data.subtasks)
		} catch (err) {
			console.error(err)
		}
	}

	const addSubtaskToTask = async taskId => {
		const titleValue = newSubtaskTitle.trim()
		if (!titleValue) return

		try {
			await axios.post(
				`${API_URL}/tasks/${taskId}/subtasks`,
				{ title: titleValue },
				{ headers }
			)
			setNewSubtaskTitle('')
			await fetchSubtasks(taskId)
		} catch (err) {
			console.error('Add subtask error:', err.response?.data || err.message)
			alert(err.response?.data?.message || 'Не вдалося додати підзадачу.')
		}
	}

	const toggleSubtaskStatus = async (taskId, subtask) => {
		try {
			await axios.put(
				`${API_URL}/tasks/${taskId}/subtasks/${subtask.subtaskId}`,
				{ isCompleted: !subtask.isCompleted },
				{ headers }
			)

			await fetchSubtasks(taskId)
			await loadTasks()
			if (view === 'dashboard') {
				await loadDashboardStatsOnly()
			}
		} catch (err) {
			console.error('Toggle subtask error:', err.response?.data || err.message)
			alert(err.response?.data?.message || 'Не вдалося оновити підзадачу.')
		}
	}

	const deleteSubtaskFromTask = async (taskId, subtaskId) => {
		try {
			await axios.delete(`${API_URL}/tasks/${taskId}/subtasks/${subtaskId}`, {
				headers
			})
			await fetchSubtasks(taskId)
		} catch (err) {
			console.error('Delete subtask error:', err.response?.data || err.message)
			alert(err.response?.data?.message || 'Не вдалося видалити підзадачу.')
		}
	}

	const fetchComments = async taskId => {
		try {
			const res = await axios.get(`${API_URL}/tasks/${taskId}/comments`, {
				headers
			})
			setComments(res.data)
		} catch (err) {
			console.error(err)
		}
	}

	const addCommentToTask = async taskId => {
		if (!newComment.trim()) return
		try {
			await axios.post(
				`${API_URL}/tasks/${taskId}/comments`,
				{ commentText: newComment },
				{ headers }
			)
			setNewComment('')
			fetchComments(taskId)
		} catch (err) {
			console.error(err)
		}
	}

	const viewTaskDetails = task => {
		setSelectedTask(task)
		setNewSubtaskTitle('')
		fetchSubtasks(task.taskId)
		fetchComments(task.taskId)
		fetchTaskTags(task.taskId)
		setView('details')
	}

	const fetchTaskTags = async taskId => {
		try {
			const res = await axios.get(`${API_URL}/tags/task/${taskId}`, { headers })
			setTaskTags(res.data)
			return res.data
		} catch {
			setTaskTags([])
			return []
		}
	}

	const toggleTaskTagSelection = tagId => {
		setSelectedTagIds(current =>
			current.includes(String(tagId))
				? current.filter(id => id !== String(tagId))
				: [...current, String(tagId)]
		)
	}

	const addTagToSelectedTask = async () => {
		if (!selectedTask || !selectedTagId) return
		try {
			await axios.post(
				`${API_URL}/tags/task/${selectedTask.taskId}/tag/${selectedTagId}`,
				{},
				{ headers }
			)
			setSelectedTagId('')
			await fetchTaskTags(selectedTask.taskId)
		} catch (err) {
			alert(err.response?.data?.message || 'Failed to add tag to task.')
		}
	}

	const removeTagFromSelectedTask = async tagId => {
		if (!selectedTask) return
		try {
			await axios.delete(
				`${API_URL}/tags/task/${selectedTask.taskId}/tag/${tagId}`,
				{ headers }
			)
			await fetchTaskTags(selectedTask.taskId)
		} catch (err) {
			alert(err.response?.data?.message || 'Failed to remove tag from task.')
		}
	}

	const editProject = async project => {
		const name = prompt('Project name', project.projectName)
		if (!name || !name.trim()) return
		const description = prompt('Description', project.description || '')
		const color = prompt('Color hex (#RRGGBB)', project.color || '#3B82F6')
		try {
			await axios.put(
				`${API_URL}/projects/${project.projectId}`,
				{
					...project,
					projectName: name.trim(),
					description: description || '',
					color: color || '#3B82F6'
				},
				{ headers }
			)
			await loadProjectsOnly()
		} catch (err) {
			alert(err.response?.data?.message || 'Failed to update project.')
		}
	}

	const deleteProject = async project => {
		if (!confirm(`Delete project "${project.projectName}"?`)) return
		try {
			await axios.delete(`${API_URL}/projects/${project.projectId}`, {
				headers
			})
			if (String(activeProjectId) === String(project.projectId)) {
				setActiveProjectId('all')
			}
			await loadProjectsOnly()
		} catch (err) {
			alert(err.response?.data?.message || 'Failed to delete project.')
		}
	}

	const editTag = async tag => {
		const name = prompt('Tag name', tag.tagName)
		if (!name || !name.trim()) return
		const color = prompt('Color hex (#RRGGBB)', tag.color || '#10B981')
		try {
			await axios.put(
				`${API_URL}/tags/${tag.tagId}`,
				{ ...tag, tagName: name.trim(), color: color || '#10B981' },
				{ headers }
			)
			await loadTagsOnly()
		} catch (err) {
			alert(err.response?.data?.message || 'Failed to update tag.')
		}
	}

	const deleteTag = async tag => {
		if (!confirm(`Delete tag #${tag.tagName}?`)) return
		try {
			await axios.delete(`${API_URL}/tags/${tag.tagId}`, { headers })
			await loadTagsOnly()
			if (selectedTask) await fetchTaskTags(selectedTask.taskId)
		} catch (err) {
			alert(err.response?.data?.message || 'Failed to delete tag.')
		}
	}

	const onKanbanDragStart = taskId => {
		setDragTaskId(taskId)
	}

	const onKanbanDragEnd = () => {
		setDragTaskId(null)
		setDragOverStatus(null)
	}

	const onKanbanDragOver = (e, statusTarget) => {
		e.preventDefault()
		if (dragOverStatus !== statusTarget) {
			setDragOverStatus(statusTarget)
		}
	}

	const onKanbanDrop = async (e, statusTarget) => {
		e.preventDefault()
		setDragOverStatus(null)

		if (!dragTaskId) {
			return
		}

		const task = tasks.find(t => t.taskId === dragTaskId)
		if (!task || task.status === statusTarget) {
			setDragTaskId(null)
			return
		}

		if (statusTarget === 'Completed') {
			const allowed = await canCompleteTask(task.taskId)
			if (!allowed) {
				setDragTaskId(null)
				return
			}
		}

		const previousTasks = tasks
		const optimisticTasks = tasks.map(t =>
			t.taskId === dragTaskId ? { ...t, status: statusTarget } : t
		)
		setTasks(optimisticTasks)

		try {
			await axios.put(
				`${API_URL}/task/${dragTaskId}`,
				{
					...task,
					status: statusTarget,
					dueDate: task.dueDate || new Date().toISOString(),
					dueTime: task.dueTime || null,
					priority: task.priority || 'Medium',
					category: task.category || 'General',
					projectId: task.projectId ?? null
				},
				{ headers }
			)
		} catch (err) {
			setTasks(previousTasks)
			console.error(
				'Failed to move task in Kanban:',
				err.response?.data || err.message
			)
			alert('Failed to move task. Please try again.')
		} finally {
			setDragTaskId(null)
		}
	}

	const PAGE_TITLE = {
		list: 'My Tasks',
		add: 'New Task',
		edit: 'Edit Task',
		kanban: 'Kanban Board',
		dashboard: 'Overview',
		projects: 'Projects',
		tags: 'Tags',
		details: 'Task Details'
	}

	const activeProjectName =
		activeProjectId === 'all'
			? 'All projects'
			: activeProjectId === '0'
				? 'No project'
				: projects.find(p => String(p.projectId) === String(activeProjectId))
						?.projectName || 'Project'

	/*  STYLES (derived from C)  */
	const S = makeStyles(C)

	/*  RENDER  */
	return (
		<div style={S.shell}>
			{/*  SIDEBAR  */}
			<aside style={S.sidebar}>
				{/* Logo */}
				<div style={S.sidebarLogo}>
					<div style={S.logoIcon}>✦</div>
					<span style={S.logoText}>Taskflow</span>
				</div>

				<div style={S.divider} />

				{/* Nav */}
				<nav style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
					{NAV.map(n => {
						const active = view === n.id || (n.id === 'add' && view === 'edit')
						return (
							<button
								key={n.id}
								style={S.navBtn(active)}
								onClick={() =>
									n.id === 'dashboard'
										? fetchStats()
										: n.id === 'projects'
											? fetchProjects()
											: n.id === 'tags'
												? fetchTags()
												: n.id === 'add'
													? (resetTaskForm(), setView('add'))
													: setView(n.id)
								}
							>
								<span style={S.navIcon(active)}>{n.icon}</span>
								<span style={{ flex: 1 }}>{n.label}</span>
								{active && <span style={S.navPip} />}
							</button>
						)
					})}
				</nav>

				<div style={{ flex: 1 }} />

				{/* Mini stats */}
				<div style={S.miniStats}>
					{(() => {
						const scopedTasks = getProjectScopedTasks(tasks)
						return (
							<>
								<div style={S.miniStatRow}>
									<span style={{ color: C.muted, fontSize: 12 }}>Total</span>
									<span
										style={{ fontWeight: 700, color: C.text, fontSize: 13 }}
									>
										{scopedTasks.length}
									</span>
								</div>
								<div style={S.miniStatRow}>
									<span style={{ color: C.muted, fontSize: 12 }}>Done</span>
									<span
										style={{ fontWeight: 700, color: C.success, fontSize: 13 }}
									>
										{scopedTasks.filter(t => t.status === 'Completed').length}
									</span>
								</div>
								<div style={S.miniStatRow}>
									<span style={{ color: C.muted, fontSize: 12 }}>Overdue</span>
									<span
										style={{ fontWeight: 700, color: C.danger, fontSize: 13 }}
									>
										{
											scopedTasks.filter(
												t => isOverdue(t.dueDate) && t.status !== 'Completed'
											).length
										}
									</span>
								</div>
								<div style={S.miniStatRow}>
									<span style={{ color: C.muted, fontSize: 12 }}>Pending</span>
									<span
										style={{ fontWeight: 700, color: C.warning, fontSize: 13 }}
									>
										{scopedTasks.filter(t => t.status === 'Pending').length}
									</span>
								</div>
							</>
						)
					})()}
				</div>

				<div style={S.divider} />

				{/* Logout */}
				<button
					style={S.logoutBtn}
					onClick={() => {
						localStorage.removeItem('token')
						window.location.href = '/'
					}}
				>
					<span></span> Sign out
				</button>
			</aside>

			{/*  MAIN  */}
			<main style={S.main}>
				{/* Top bar */}
				<header style={S.topbar}>
					<div>
						<h1 style={S.pageTitle}>{PAGE_TITLE[view]}</h1>
						<p style={S.pageSubtitle}>
							Manage your daily work efficiently · {activeProjectName}
						</p>
					</div>
					<div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
						{/* Theme toggle */}
						<button
							style={S.themeBtn}
							onClick={toggleTheme}
							title={isDark ? 'Light mode' : 'Dark mode'}
						>
							{isDark ? '☀' : '☾'}
						</button>

						{/* User Profile */}
						<div style={{ position: 'relative' }} data-profile-menu>
							<button
								style={S.profileBtn}
								onClick={() => setShowProfileMenu(!showProfileMenu)}
							>
								<div style={S.profileAvatar}>
									{(userProfile?.username || 'U')[0].toUpperCase()}
								</div>
								<div style={{ textAlign: 'left' }}>
									<div
										style={{
											fontSize: 13,
											fontWeight: 600,
											color: C.text,
											fontFamily: FONT_DISPLAY
										}}
									>
										{userProfile?.username || 'User'}
									</div>
									<div
										style={{
											fontSize: 11,
											color: C.faint,
											fontFamily: FONT_BODY
										}}
									>
										{userProfile?.email || 'user@example.com'}
									</div>
								</div>
								<span style={{ fontSize: 10, color: C.muted }}></span>
							</button>

							{showProfileMenu && (
								<div style={S.profileMenu}>
									<button
										style={S.profileMenuItem}
										onMouseEnter={e =>
											(e.currentTarget.style.background = C.inputBg)
										}
										onMouseLeave={e =>
											(e.currentTarget.style.background = 'transparent')
										}
										onClick={() => {
											setShowProfileModal(true)
											setShowProfileMenu(false)
										}}
									>
										<span></span> Edit Profile
									</button>
									<div
										style={{ height: 1, background: C.border, margin: '4px 0' }}
									/>
									<button
										style={{ ...S.profileMenuItem, color: C.danger }}
										onMouseEnter={e =>
											(e.currentTarget.style.background = C.logoutBg)
										}
										onMouseLeave={e =>
											(e.currentTarget.style.background = 'transparent')
										}
										onClick={() => {
											localStorage.removeItem('token')
											window.location.href = '/'
										}}
									>
										<span></span> Sign out
									</button>
								</div>
							)}
						</div>

						{(view === 'list' || view === 'kanban') && (
							<select
								style={{ ...S.input, margin: 0, minWidth: 190 }}
								value={activeProjectId}
								onChange={e => setActiveProjectId(e.target.value)}
							>
								<option value="all">All projects</option>
								<option value="0">No project</option>
								{projects.map(p => (
									<option key={p.projectId} value={String(p.projectId)}>
										{p.projectName}
									</option>
								))}
							</select>
						)}

						{view === 'list' && (
							<button style={S.primaryBtn} onClick={() => setView('add')}>
								+ New Task
							</button>
						)}
						{view === 'kanban' && (
							<button style={S.primaryBtn} onClick={() => setView('add')}>
								+ New Task
							</button>
						)}
					</div>
				</header>

				{/*  TASK LIST  */}
				{view === 'list' && (
					<div>
						{/* NEW: Search and Filter Bar */}
						<div
							style={{
								background: C.card,
								border: `1px solid ${C.border}`,
								borderRadius: 12,
								padding: '20px',
								marginBottom: 24,
								display: 'flex',
								flexWrap: 'wrap',
								gap: 16,
								alignItems: 'center'
							}}
						>
							{/* Search */}
							<div style={{ flex: '1 1 300px', minWidth: 200 }}>
								<input
									style={{
										...S.input,
										margin: 0,
										background: C.inputBg,
										border: `1px solid ${C.border}`
									}}
									placeholder=" Search tasks..."
									value={searchQuery}
									onChange={e => setSearchQuery(e.target.value)}
								/>
							</div>

							{/* Status Filter */}
							<select
								style={{
									...S.input,
									margin: 0,
									minWidth: 120,
									flex: '0 0 auto'
								}}
								value={filterStatus}
								onChange={e => setFilterStatus(e.target.value)}
							>
								<option value="All">All Status</option>
								<option value="Pending">Pending</option>
								<option value="In Progress">In Progress</option>
								<option value="Completed">Completed</option>
							</select>

							{/* Priority Filter */}
							<select
								style={{
									...S.input,
									margin: 0,
									minWidth: 120,
									flex: '0 0 auto'
								}}
								value={filterPriority}
								onChange={e => setFilterPriority(e.target.value)}
							>
								<option value="All">All Priority</option>
								<option value="High">High Priority</option>
								<option value="Medium">Medium Priority</option>
								<option value="Low">Low Priority</option>
							</select>

							{/* Sort By */}
							<select
								style={{
									...S.input,
									margin: 0,
									minWidth: 120,
									flex: '0 0 auto'
								}}
								value={sortBy}
								onChange={e => setSortBy(e.target.value)}
							>
								<option value="created">Sort by Created</option>
								<option value="dueDate">Sort by Due Date</option>
								<option value="priority">Sort by Priority</option>
							</select>

							{/* Clear Filters */}
							{(searchQuery ||
								filterStatus !== 'All' ||
								filterPriority !== 'All' ||
								sortBy !== 'created') && (
								<button
									style={{
										...S.ghostBtn,
										padding: '8px 16px',
										fontSize: 12,
										margin: 0
									}}
									onClick={() => {
										setSearchQuery('')
										setFilterStatus('All')
										setFilterPriority('All')
										setSortBy('created')
									}}
								>
									Clear Filters
								</button>
							)}
						</div>

						{loading && (
							<div style={S.emptyState}>
								<div style={S.spinner} />
								<p
									style={{
										color: C.muted,
										marginTop: 16,
										fontFamily: FONT_BODY
									}}
								>
									Loading tasks
								</p>
							</div>
						)}
						{!loading && tasks.length === 0 && (
							<div style={S.emptyState}>
								<div
									style={{ fontSize: 44, opacity: 0.18, marginBottom: 14 }}
								></div>
								<p
									style={{
										color: C.muted,
										fontWeight: 700,
										fontSize: 16,
										fontFamily: FONT_DISPLAY
									}}
								>
									No tasks yet
								</p>
								<p
									style={{
										color: C.faint,
										fontSize: 14,
										marginTop: 4,
										fontFamily: FONT_BODY
									}}
								>
									Create your first task to get started.
								</p>
								<button
									style={{ ...S.primaryBtn, marginTop: 20 }}
									onClick={() => setView('add')}
								>
									+ New Task
								</button>
							</div>
						)}

						{/* Task Grid with Filtered Results */}
						{(() => {
							const filteredTasks = getFilteredAndSortedTasks()

							if (!loading && tasks.length > 0 && filteredTasks.length === 0) {
								return (
									<div style={S.emptyState}>
										<div
											style={{ fontSize: 44, opacity: 0.18, marginBottom: 14 }}
										></div>
										<p
											style={{
												color: C.muted,
												fontWeight: 700,
												fontSize: 16,
												fontFamily: FONT_DISPLAY
											}}
										>
											No tasks match your filters
										</p>
										<p
											style={{
												color: C.faint,
												fontSize: 14,
												marginTop: 4,
												fontFamily: FONT_BODY
											}}
										>
											Try adjusting your search or filter criteria.
										</p>
									</div>
								)
							}

							return (
								<div style={S.grid}>
									{filteredTasks.map(task => {
										const sp = SP[task.status] || SP['Pending']
										const pp = PP[task.priority] || PP['Medium']
										const overdue = isOverdue(task.dueDate)

										return (
											<div
												key={task.taskId}
												style={{
													...S.taskCard,
													borderLeft: overdue
														? `3px solid ${C.danger}`
														: `3px solid transparent`,
													background: overdue ? `${C.danger}08` : C.card
												}}
												onMouseEnter={e =>
													(e.currentTarget.style.background = overdue
														? `${C.danger}12`
														: C.cardHover)
												}
												onMouseLeave={e =>
													(e.currentTarget.style.background = overdue
														? `${C.danger}08`
														: C.card)
												}
											>
												{/* Status + Priority + Actions row */}
												<div
													style={{
														display: 'flex',
														justifyContent: 'space-between',
														alignItems: 'center',
														marginBottom: 12,
														flexWrap: 'wrap',
														gap: 8
													}}
												>
													<div
														style={{
															display: 'flex',
															gap: 8,
															alignItems: 'center'
														}}
													>
														<span
															style={{
																...S.badge,
																background: sp.bg,
																color: sp.text
															}}
														>
															{task.status}
														</span>
														<span
															style={{
																...S.badge,
																background: pp.bg,
																color: pp.text,
																fontSize: 10
															}}
														>
															{pp.icon} {task.priority || 'Medium'}
														</span>
														{overdue && (
															<span
																style={{
																	...S.badge,
																	background: `${C.danger}1e`,
																	color: C.danger,
																	fontSize: 10
																}}
															>
																Overdue
															</span>
														)}
													</div>
													<div style={{ display: 'flex', gap: 6 }}>
														<button
															style={S.iconBtn(C.success)}
															title="Mark as completed"
															onClick={() => markTaskCompleted(task)}
														>
															<span style={S.iconBtnText}>OK</span>
														</button>
														<button
															style={S.iconBtn(C.info)}
															title="Edit"
															onClick={() => startEdit(task)}
														>
															<span style={S.iconBtnText}>ED</span>
														</button>
														<button
															style={S.iconBtn(C.danger)}
															title="Delete"
															onClick={() => removeTask(task.taskId)}
														>
															<span style={S.iconBtnText}>DEL</span>
														</button>
													</div>
												</div>

												<h3
													style={{
														fontSize: 15,
														fontWeight: 700,
														color: C.text,
														marginBottom: 6,
														lineHeight: 1.4,
														fontFamily: FONT_DISPLAY
													}}
												>
													{task.title}
												</h3>
												<p
													style={{
														fontSize: 13,
														color: C.muted,
														lineHeight: 1.65,
														marginBottom: 12,
														flexGrow: 1,
														fontFamily: FONT_BODY
													}}
												>
													{task.description || ''}
												</p>

												{/* Due Date */}
												{task.dueDate && (
													<div
														style={{
															fontSize: 11,
															color: overdue ? C.danger : C.faint,
															marginBottom: 12,
															fontFamily: FONT_BODY,
															fontWeight: overdue ? 600 : 400
														}}
													>
														Due: {new Date(task.dueDate).toLocaleDateString()}
														{task.dueTime
															? ` at ${formatDueTime(task.dueTime)}`
															: ''}
													</div>
												)}

												<button
													style={S.detailsBtn}
													onClick={() => viewTaskDetails(task)}
												>
													View details
												</button>
											</div>
										)
									})}
								</div>
							)
						})()}
					</div>
				)}

				{/*  KANBAN BOARD  */}
				{view === 'kanban' && (
					<div>
						<div
							style={{
								display: 'flex',
								alignItems: 'center',
								gap: 10,
								marginBottom: 14,
								background: C.card,
								border: `1px solid ${C.border}`,
								borderRadius: 12,
								padding: 12
							}}
						>
							<span
								style={{ color: C.muted, fontSize: 12, fontFamily: FONT_BODY }}
							>
								Project scope
							</span>
							<select
								style={{ ...S.input, margin: 0, minWidth: 220 }}
								value={activeProjectId}
								onChange={e => setActiveProjectId(e.target.value)}
							>
								<option value="all">All projects</option>
								<option value="0">No project</option>
								{projects.map(p => (
									<option key={p.projectId} value={String(p.projectId)}>
										{p.projectName}
									</option>
								))}
							</select>
							<span
								style={{ color: C.faint, fontSize: 12, fontFamily: FONT_BODY }}
							>
								{activeProjectId === 'all'
									? 'Showing tasks from all projects'
									: activeProjectId === '0'
										? 'Showing tasks without project'
										: `Showing ${projects.find(p => String(p.projectId) === String(activeProjectId))?.projectName || 'project'}`}
							</span>
						</div>
						<div
							style={{
								display: 'grid',
								gridTemplateColumns: 'repeat(3, 1fr)',
								gap: 20,
								minHeight: '60vh'
							}}
						>
							{['Pending', 'In Progress', 'Completed'].map(columnStatus => {
								const columnTasks = getProjectScopedTasks(tasks).filter(
									task => task.status === columnStatus
								)
								const sp = SP[columnStatus]

								return (
									<div
										key={columnStatus}
										style={{
											background: C.card,
											border: `1px solid ${C.border}`,
											borderRadius: 12,
											padding: 16,
											display: 'flex',
											flexDirection: 'column',
											boxShadow:
												dragOverStatus === columnStatus
													? `0 0 0 2px ${sp.text}55 inset`
													: 'none',
											transition: 'box-shadow 0.15s ease'
										}}
									>
										<div
											style={{
												display: 'flex',
												alignItems: 'center',
												justifyContent: 'space-between',
												marginBottom: 16,
												paddingBottom: 12,
												borderBottom: `2px solid ${sp.text}22`
											}}
										>
											<h3
												style={{
													fontSize: 14,
													fontWeight: 700,
													color: sp.text,
													margin: 0,
													fontFamily: FONT_DISPLAY,
													textTransform: 'uppercase',
													letterSpacing: '0.05em'
												}}
											>
												{columnStatus}
											</h3>
											<span
												style={{
													background: `${sp.text}1e`,
													color: sp.text,
													fontSize: 11,
													fontWeight: 700,
													padding: '4px 8px',
													borderRadius: 999,
													fontFamily: FONT_BODY
												}}
											>
												{columnTasks.length}
											</span>
										</div>

										<div
											style={{
												display: 'flex',
												flexDirection: 'column',
												gap: 12,
												flex: 1,
												minHeight: 200
											}}
											onDragOver={e => onKanbanDragOver(e, columnStatus)}
											onDragEnter={e => onKanbanDragOver(e, columnStatus)}
											onDragLeave={() => {
												if (dragOverStatus === columnStatus) {
													setDragOverStatus(null)
												}
											}}
											onDrop={e => onKanbanDrop(e, columnStatus)}
										>
											{columnTasks.length === 0 && (
												<div
													style={{
														display: 'flex',
														alignItems: 'center',
														justifyContent: 'center',
														height: 120,
														color: C.faint,
														fontSize: 13,
														fontFamily: FONT_BODY,
														border: `2px dashed ${C.border}`,
														borderRadius: 8
													}}
												>
													No {columnStatus.toLowerCase()} tasks
												</div>
											)}

											{columnTasks.map(task => {
												const pp = PP[task.priority] || PP['Medium']
												const overdue = isOverdue(task.dueDate)

												return (
													<div
														key={task.taskId}
														style={{
															background: C.inputBg,
															border: `1px solid ${C.border}`,
															borderRadius: 8,
															padding: 12,
															cursor: 'pointer',
															transition: 'all 0.15s',
															borderLeft: overdue
																? `3px solid ${C.danger}`
																: `3px solid ${pp.text}`,
															opacity: dragTaskId === task.taskId ? 0.6 : 1
														}}
														draggable
														onDragStart={() => onKanbanDragStart(task.taskId)}
														onDragEnd={onKanbanDragEnd}
														onClick={() => viewTaskDetails(task)}
													>
														<div
															style={{
																display: 'flex',
																justifyContent: 'space-between',
																alignItems: 'flex-start',
																marginBottom: 8
															}}
														>
															<h4
																style={{
																	fontSize: 13,
																	fontWeight: 600,
																	color: C.text,
																	margin: 0,
																	lineHeight: 1.3,
																	fontFamily: FONT_DISPLAY,
																	flex: 1
																}}
															>
																{task.title}
															</h4>
															{getTaskTagObjects(task).length > 0 && (
																<div
																	style={{
																		display: 'flex',
																		flexWrap: 'wrap',
																		gap: 6,
																		marginBottom: 10
																	}}
																>
																	{getTaskTagObjects(task).map(tag => (
																		<span
																			key={tag.tagId}
																			style={{
																				padding: '4px 8px',
																				borderRadius: 999,
																				fontSize: 11,
																				fontWeight: 700,
																				background: `${tag.color}20`,
																				color: tag.color,
																				border: `1px solid ${tag.color}35`,
																				fontFamily: FONT_BODY
																			}}
																		>
																			#{tag.tagName}
																		</span>
																	))}
																</div>
															)}
														</div>

														{task.description && (
															<p
																style={{
																	fontSize: 11,
																	color: C.muted,
																	margin: '0 0 8px 0',
																	lineHeight: 1.4,
																	fontFamily: FONT_BODY
																}}
															>
																{task.description.substring(0, 60)}...
															</p>
														)}

														<div
															style={{
																display: 'flex',
																justifyContent: 'space-between',
																alignItems: 'center',
																marginTop: 8
															}}
														>
															<span
																style={{
																	...S.badge,
																	background: pp.bg,
																	color: pp.text,
																	fontSize: 9,
																	padding: '2px 6px'
																}}
															>
																{pp.icon} {task.priority || 'Medium'}
															</span>

															{task.dueDate && (
																<div
																	style={{
																		fontSize: 9,
																		color: overdue ? C.danger : C.faint,
																		fontFamily: FONT_BODY
																	}}
																>
																	{new Date(task.dueDate).toLocaleDateString()}
																	{task.dueTime
																		? ` ${formatDueTime(task.dueTime)}`
																		: ''}
																</div>
															)}
														</div>
													</div>
												)
											})}
										</div>
									</div>
								)
							})}
						</div>
					</div>
				)}

				{/*  ADD / EDIT FORM  */}
				{(view === 'add' || view === 'edit') && (
					<div style={S.formWrap}>
						<form
							onSubmit={view === 'add' ? handleAddTask : handleEditTask}
							style={S.form}
						>
							<div style={S.formField}>
								<label style={S.label}>Task Title</label>
								<input
									style={S.input}
									placeholder="What needs to be done?"
									value={title}
									onChange={e => setTitle(e.target.value)}
									required
								/>
							</div>
							<div style={S.formField}>
								<label style={S.label}>Description</label>
								<textarea
									style={{ ...S.input, resize: 'vertical', minHeight: 100 }}
									rows={4}
									placeholder="Add more details"
									value={description}
									onChange={e => setDescription(e.target.value)}
								/>
							</div>

							{/* Row with Status and Priority */}
							<div style={{ display: 'flex', gap: 16 }}>
								<div style={{ ...S.formField, flex: 1 }}>
									<label style={S.label}>Status</label>
									<select
										style={S.input}
										value={status}
										onChange={e => setStatus(e.target.value)}
									>
										<option>Pending</option>
										<option>In Progress</option>
										<option>Completed</option>
									</select>
								</div>
								<div style={{ ...S.formField, flex: 1 }}>
									<label style={S.label}>Priority</label>
									<select
										style={S.input}
										value={priority}
										onChange={e => setPriority(e.target.value)}
									>
										<option value="Low">Low Priority</option>
										<option value="Medium">Medium Priority</option>
										<option value="High">High Priority</option>
									</select>
								</div>
							</div>

							<div style={S.formField}>
								<label style={S.label}>Project</label>
								<select
									style={S.input}
									value={projectId}
									onChange={e => setProjectId(e.target.value)}
								>
									<option value="">No project</option>
									{projects.map(p => (
										<option key={p.projectId} value={String(p.projectId)}>
											{p.projectName}
										</option>
									))}
								</select>
							</div>

							<div style={S.formField}>
								<label style={S.label}>Tags</label>
								<div
									style={{
										display: 'grid',
										gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
										gap: 10
									}}
								>
									{tags.length === 0 ? (
										<div style={{ color: C.muted, fontSize: 13 }}>
											Create tags first to assign them to tasks.
										</div>
									) : (
										tags.map(tag => {
											const checked = selectedTagIds.includes(String(tag.tagId))
											return (
												<label
													key={tag.tagId}
													style={{
														display: 'flex',
														alignItems: 'center',
														gap: 10,
														padding: '10px 12px',
														borderRadius: 14,
														border: `1px solid ${checked ? tag.color : C.border}`,
														background: checked ? `${tag.color}18` : C.inputBg,
														cursor: 'pointer',
														fontSize: 13,
														fontWeight: 600,
														color: C.text
													}}
												>
													<input
														type="checkbox"
														checked={checked}
														onChange={() => toggleTaskTagSelection(tag.tagId)}
														style={{ accentColor: tag.color }}
													/>
													<span
														style={{
															width: 10,
															height: 10,
															borderRadius: 999,
															background: tag.color,
															flexShrink: 0
														}}
													/>
													<span>#{tag.tagName}</span>
												</label>
											)
										})
									)}
								</div>
							</div>

							<div style={S.formField}>
								<label style={S.label}>Due Date</label>
								<input
									style={S.input}
									type="date"
									value={dueDate}
									onChange={e => setDueDate(e.target.value)}
									min={new Date().toISOString().split('T')[0]}
								/>
							</div>

							<div style={S.formField}>
								<label style={S.label}>Due Time</label>
								<input
									style={S.input}
									type="time"
									value={dueTime}
									onChange={e => setDueTime(e.target.value)}
								/>
							</div>

							<div style={{ display: 'flex', gap: 12, marginTop: 4 }}>
								<button type="submit" style={S.primaryBtn}>
									{view === 'add' ? 'Save Task' : 'Update Task'}
								</button>
								<button
									type="button"
									style={S.ghostBtn}
									onClick={() => {
										resetTaskForm()
										setView('list')
									}}
								>
									Cancel
								</button>
							</div>
						</form>
					</div>
				)}

				{/*  OVERVIEW STATS  */}
				{view === 'dashboard' && stats && (
					<div style={S.grid}>
						{[
							{
								label: 'Total Tasks',
								value: stats.totalTasks,
								icon: '',
								color: C.info
							},
							{
								label: 'Completed Today',
								value: stats.completedToday,
								icon: '',
								color: C.success
							},
							{
								label: 'Overdue',
								value: stats.overdueTasks,
								icon: '',
								color: C.danger
							},
							{
								label: 'In Progress',
								value: stats.inProgressTasks,
								icon: '',
								color: C.warning
							},
							{
								label: 'Completion %',
								value: `${stats.completionPercentage.toFixed(1)}%`,
								icon: '',
								color: C.accent
							},
							{
								label: 'Projects',
								value: stats.totalProjects,
								icon: '',
								color: '#06b6d4'
							}
						].map(s => (
							<div
								key={s.label}
								style={{ ...S.statCard, borderTop: `3px solid ${s.color}` }}
							>
								<div style={{ fontSize: 20, color: s.color, marginBottom: 10 }}>
									{s.icon}
								</div>
								<p
									style={{
										fontSize: 12,
										color: C.muted,
										marginBottom: 6,
										fontFamily: FONT_BODY,
										textTransform: 'uppercase',
										letterSpacing: '0.06em'
									}}
								>
									{s.label}
								</p>
								<p
									style={{
										fontSize: 34,
										fontWeight: 900,
										color: C.text,
										fontFamily: FONT_DISPLAY,
										letterSpacing: '-0.04em',
										lineHeight: 1
									}}
								>
									{s.value}
								</p>
							</div>
						))}
					</div>
				)}

				{/*  PROJECTS  */}
				{view === 'projects' && (
					<div>
						<form
							onSubmit={createProject}
							style={{
								display: 'grid',
								gridTemplateColumns: '2fr 2fr auto auto',
								gap: 10,
								background: C.card,
								border: `1px solid ${C.border}`,
								borderRadius: 12,
								padding: 14,
								marginBottom: 18
							}}
						>
							<input
								style={S.input}
								placeholder="Project name"
								value={newProjectName}
								onChange={e => setNewProjectName(e.target.value)}
								required
							/>
							<input
								style={S.input}
								placeholder="Description (optional)"
								value={newProjectDescription}
								onChange={e => setNewProjectDescription(e.target.value)}
							/>
							<input
								style={{ ...S.input, minWidth: 80, padding: 6 }}
								type="color"
								value={newProjectColor}
								onChange={e => setNewProjectColor(e.target.value)}
								title="Project color"
							/>
							<button type="submit" style={S.primaryBtn}>
								+ Add Project
							</button>
						</form>

						<div style={S.grid}>
							{projects.length === 0 && (
								<div style={S.emptyState}>
									<p style={{ color: C.muted, fontFamily: FONT_BODY }}>
										No projects yet. Create your first one above.
									</p>
								</div>
							)}
							{projects.map(p => (
								<div
									key={p.projectId}
									style={{
										...S.taskCard,
										borderLeft: `3px solid ${p.color || C.accent}`
									}}
								>
									<div
										style={{
											width: 8,
											height: 8,
											borderRadius: '50%',
											background: p.color || C.accent,
											marginBottom: 12
										}}
									/>
									<h3
										style={{
											fontSize: 15,
											fontWeight: 700,
											color: C.text,
											marginBottom: 6,
											fontFamily: FONT_DISPLAY
										}}
									>
										{p.projectName}
									</h3>
									<p
										style={{
											fontSize: 13,
											color: C.muted,
											lineHeight: 1.65,
											fontFamily: FONT_BODY
										}}
									>
										{p.description || ''}
									</p>
									<div style={{ display: 'flex', gap: 8, marginTop: 12 }}>
										<button
											style={S.primaryBtn}
											title="Open project on Kanban board"
											onClick={() => {
												setActiveProjectId(String(p.projectId))
												setView('kanban')
											}}
										>
											Open Kanban
										</button>
										<button
											style={S.iconBtn(C.info)}
											title="Edit project"
											onClick={() => editProject(p)}
										></button>
										<button
											style={S.iconBtn(C.danger)}
											title="Delete project"
											onClick={() => deleteProject(p)}
										></button>
									</div>
								</div>
							))}
						</div>
					</div>
				)}

				{/*  TAGS  */}
				{view === 'tags' && (
					<div>
						<form
							onSubmit={createTag}
							style={{
								display: 'grid',
								gridTemplateColumns: '2fr auto auto',
								gap: 10,
								background: C.card,
								border: `1px solid ${C.border}`,
								borderRadius: 12,
								padding: 14,
								marginBottom: 18
							}}
						>
							<input
								style={S.input}
								placeholder="Tag name"
								value={newTagName}
								onChange={e => setNewTagName(e.target.value)}
								required
							/>
							<input
								style={{ ...S.input, minWidth: 80, padding: 6 }}
								type="color"
								value={newTagColor}
								onChange={e => setNewTagColor(e.target.value)}
								title="Tag color"
							/>
							<button type="submit" style={S.primaryBtn}>
								+ Add Tag
							</button>
						</form>

						<div
							style={{
								display: 'flex',
								flexWrap: 'wrap',
								gap: 10,
								padding: '4px 0'
							}}
						>
							{tags.length === 0 && (
								<p style={{ color: C.muted, fontFamily: FONT_BODY }}>
									No tags yet. Create one above.
								</p>
							)}
							{tags.map(t => (
								<span
									key={t.tagId}
									style={{
										padding: '8px 18px',
										borderRadius: 999,
										background: t.color ? `${t.color}22` : `${C.accent}1e`,
										color: t.color || C.accentLt,
										border: `1px solid ${t.color ? `${t.color}44` : `${C.accent}33`}`,
										fontWeight: 600,
										fontSize: 13,
										fontFamily: FONT_BODY
									}}
								>
									# {t.tagName}
									<button
										title="Edit tag"
										onClick={() => editTag(t)}
										style={{
											marginLeft: 8,
											border: 0,
											background: 'transparent',
											color: t.color || C.accentLt,
											cursor: 'pointer'
										}}
									>
										✎
									</button>
									<button
										title="Delete tag"
										onClick={() => deleteTag(t)}
										style={{
											marginLeft: 4,
											border: 0,
											background: 'transparent',
											color: C.danger,
											cursor: 'pointer'
										}}
									>
										×
									</button>
								</span>
							))}
						</div>
					</div>
				)}

				{/*  TASK DETAILS  */}
				{view === 'details' && selectedTask && (
					<div style={{ maxWidth: 680 }}>
						<button style={S.backBtn} onClick={() => setView('list')}>
							{' '}
							Back to tasks
						</button>

						<div style={{ ...S.taskCard, marginBottom: 22 }}>
							{(() => {
								const sp = SP[selectedTask.status] || SP['Pending']
								return (
									<span
										style={{ ...S.badge, background: sp.bg, color: sp.text }}
									>
										{selectedTask.status}
									</span>
								)
							})()}
							<h2
								style={{
									fontSize: 22,
									fontWeight: 800,
									color: C.text,
									margin: '14px 0 8px',
									fontFamily: FONT_DISPLAY,
									letterSpacing: '-0.03em'
								}}
							>
								{selectedTask.title}
							</h2>
							<p
								style={{
									color: C.muted,
									lineHeight: 1.7,
									fontSize: 14,
									fontFamily: FONT_BODY
								}}
							>
								{selectedTask.description || ''}
							</p>
							<div
								style={{
									marginTop: 14,
									display: 'flex',
									alignItems: 'center',
									gap: 10,
									flexWrap: 'wrap'
								}}
							>
								<span
									style={{
										...S.badge,
										background: `${C.info}1e`,
										color: C.info
									}}
								>
									Project
								</span>
								<span
									style={{ color: C.text, fontFamily: FONT_BODY, fontSize: 13 }}
								>
									{projects.find(p => p.projectId === selectedTask.projectId)
										?.projectName || 'No project'}
								</span>
								{selectedTask.dueDate && (
									<span
										style={{
											color: C.faint,
											fontFamily: FONT_BODY,
											fontSize: 12
										}}
									>
										Due {new Date(selectedTask.dueDate).toLocaleDateString()}
										{selectedTask.dueTime
											? ` ${formatDueTime(selectedTask.dueTime)}`
											: ''}
									</span>
								)}
							</div>
						</div>

						<div style={S.section}>
							<p style={S.sectionLabel}>Tags</p>
							<div style={{ display: 'flex', gap: 10, marginBottom: 12 }}>
								<select
									style={{ ...S.input, margin: 0, flex: 1 }}
									value={selectedTagId}
									onChange={e => setSelectedTagId(e.target.value)}
								>
									<option value="">Select tag</option>
									{tags.map(t => (
										<option key={t.tagId} value={String(t.tagId)}>
											#{t.tagName}
										</option>
									))}
								</select>
								<button style={S.primaryBtn} onClick={addTagToSelectedTask}>
									Add Tag
								</button>
							</div>
							<div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
								{taskTags.length === 0 && (
									<p
										style={{
											color: C.faint,
											fontSize: 13,
											fontFamily: FONT_BODY
										}}
									>
										No tags linked to this task.
									</p>
								)}
								{taskTags.map(t => (
									<span
										key={t.tagId}
										style={{
											padding: '6px 12px',
											borderRadius: 999,
											background: t.color ? `${t.color}22` : `${C.accent}1e`,
											color: t.color || C.accentLt,
											border: `1px solid ${t.color ? `${t.color}44` : `${C.accent}33`}`,
											fontWeight: 600,
											fontSize: 12,
											fontFamily: FONT_BODY
										}}
									>
										# {t.tagName}
										<button
											style={{
												marginLeft: 8,
												border: 0,
												background: 'transparent',
												color: C.danger,
												cursor: 'pointer'
											}}
											onClick={() => removeTagFromSelectedTask(t.tagId)}
										>
											×
										</button>
									</span>
								))}
							</div>
						</div>

						{/* Subtasks */}
						<div style={S.section}>
							<p style={S.sectionLabel}>Subtasks</p>
							<div style={{ display: 'flex', gap: 10, marginBottom: 12 }}>
								<input
									style={{ ...S.input, margin: 0, flex: 1 }}
									placeholder="New subtask"
									value={newSubtaskTitle}
									onChange={e => setNewSubtaskTitle(e.target.value)}
									onKeyDown={e => {
										if (e.key === 'Enter') {
											e.preventDefault()
											addSubtaskToTask(selectedTask.taskId)
										}
									}}
								/>
								<button
									style={S.primaryBtn}
									onClick={() => addSubtaskToTask(selectedTask.taskId)}
								>
									Add
								</button>
							</div>
							{subtasks.length === 0 && (
								<p
									style={{
										color: C.faint,
										fontSize: 13,
										fontFamily: FONT_BODY
									}}
								>
									No subtasks.
								</p>
							)}
							{subtasks.map(s => (
								<div key={s.subtaskId} style={S.subtaskRow}>
									<input
										type="checkbox"
										checked={s.isCompleted}
										onChange={() => toggleSubtaskStatus(selectedTask.taskId, s)}
										style={{
											accentColor: C.accent,
											width: 15,
											height: 15,
											cursor: 'pointer'
										}}
									/>
									<span
										style={{
											fontSize: 14,
											fontFamily: FONT_BODY,
											color: s.isCompleted ? C.faint : C.text,
											textDecoration: s.isCompleted ? 'line-through' : 'none'
										}}
									>
										{s.title}
									</span>
									<button
										style={{ ...S.iconBtn(C.danger), width: 24, height: 24 }}
										onClick={() =>
											deleteSubtaskFromTask(selectedTask.taskId, s.subtaskId)
										}
										title="Delete subtask"
									>
										<span style={S.iconBtnText}>X</span>
									</button>
								</div>
							))}
						</div>

						{/* Comments */}
						<div style={S.section}>
							<p style={S.sectionLabel}>Comments</p>
							<div style={{ display: 'flex', gap: 10, marginBottom: 16 }}>
								<textarea
									style={{ ...S.input, flex: 1, resize: 'none', height: 80 }}
									placeholder="Write a comment"
									value={newComment}
									onChange={e => setNewComment(e.target.value)}
								/>
								<button
									style={{
										...S.primaryBtn,
										alignSelf: 'flex-end',
										whiteSpace: 'nowrap'
									}}
									onClick={() => addCommentToTask(selectedTask.taskId)}
								>
									Post
								</button>
							</div>
							{comments.length === 0 && (
								<p
									style={{
										color: C.faint,
										fontSize: 13,
										fontFamily: FONT_BODY
									}}
								>
									No comments yet.
								</p>
							)}
							{comments.map(c => (
								<div key={c.commentId} style={S.commentCard}>
									<div
										style={{
											display: 'flex',
											alignItems: 'center',
											gap: 10,
											marginBottom: 8
										}}
									>
										<div style={S.avatar}>
											{(c.userName || 'U')[0].toUpperCase()}
										</div>
										<strong
											style={{
												fontSize: 13,
												color: C.text,
												fontFamily: FONT_DISPLAY
											}}
										>
											{c.userName}
										</strong>
									</div>
									<p
										style={{
											fontSize: 14,
											color: C.muted,
											lineHeight: 1.65,
											fontFamily: FONT_BODY
										}}
									>
										{c.commentText}
									</p>
								</div>
							))}
						</div>
					</div>
				)}
			</main>

			{/*  PROFILE MODAL  */}
			{showProfileModal && (
				<div style={S.modalOverlay} onClick={() => setShowProfileModal(false)}>
					<div style={S.modalContent} onClick={e => e.stopPropagation()}>
						<div
							style={{
								display: 'flex',
								justifyContent: 'space-between',
								alignItems: 'center',
								marginBottom: 24
							}}
						>
							<h2
								style={{
									fontSize: 20,
									fontWeight: 800,
									color: C.text,
									fontFamily: FONT_DISPLAY,
									margin: 0
								}}
							>
								Edit Profile
							</h2>
							<button
								style={S.closeBtn}
								onClick={() => setShowProfileModal(false)}
							></button>
						</div>

						<form
							onSubmit={handleUpdateProfile}
							style={{ display: 'flex', flexDirection: 'column', gap: 20 }}
						>
							<div style={S.formField}>
								<label style={S.label}>Username</label>
								<input
									style={S.input}
									placeholder="Enter username"
									value={profileForm.username}
									onChange={e =>
										setProfileForm({ ...profileForm, username: e.target.value })
									}
									required
								/>
							</div>

							<div style={S.formField}>
								<label style={S.label}>Email</label>
								<input
									style={S.input}
									type="email"
									placeholder="Enter email"
									value={profileForm.email}
									onChange={e =>
										setProfileForm({ ...profileForm, email: e.target.value })
									}
									required
								/>
							</div>

							<div style={{ display: 'flex', gap: 12, marginTop: 4 }}>
								<button type="submit" style={S.primaryBtn}>
									Save Changes
								</button>
								<button
									type="button"
									style={S.ghostBtn}
									onClick={() => setShowProfileModal(false)}
								>
									Cancel
								</button>
							</div>
						</form>
					</div>
				</div>
			)}
		</div>
	)
}

/*  style factory (re-runs when C changes)  */
function makeStyles(C) {
	return {
		shell: {
			display: 'flex',
			minHeight: '100vh',
			background: C.bg,
			fontFamily: FONT_BODY,
			transition: 'background 0.3s'
		},
		sidebar: {
			width: 220,
			minWidth: 220,
			background: C.sidebar,
			borderRight: `1px solid ${C.border}`,
			padding: '22px 14px',
			display: 'flex',
			flexDirection: 'column',
			gap: 4,
			transition: 'background 0.3s, border-color 0.3s'
		},
		sidebarLogo: {
			display: 'flex',
			alignItems: 'center',
			gap: 10,
			padding: '4px 8px 18px'
		},
		logoIcon: {
			width: 34,
			height: 34,
			background: `linear-gradient(135deg,${C.accent},${C.accentLt})`,
			borderRadius: 9,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			fontSize: 15,
			color: '#fff',
			boxShadow: `0 6px 18px ${C.accent}55`,
			flexShrink: 0
		},
		logoText: {
			fontFamily: FONT_DISPLAY,
			fontWeight: 800,
			fontSize: 16,
			color: C.text,
			letterSpacing: '-0.03em'
		},
		divider: {
			height: 1,
			background: C.border,
			margin: '8px 0'
		},
		navBtn: active => ({
			display: 'flex',
			alignItems: 'center',
			gap: 10,
			padding: '10px 12px',
			borderRadius: 9,
			border: active ? `1px solid ${C.accent}33` : '1px solid transparent',
			cursor: 'pointer',
			background: active ? `${C.accent}18` : 'transparent',
			color: active ? C.accentLt : C.muted,
			fontSize: 13,
			fontFamily: FONT_BODY,
			fontWeight: active ? 600 : 400,
			textAlign: 'left',
			transition: 'all 0.15s'
		}),
		navIcon: active => ({
			width: 22,
			height: 22,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			fontSize: 14,
			color: active ? C.accent : C.faint,
			flexShrink: 0
		}),
		navPip: {
			width: 6,
			height: 6,
			borderRadius: '50%',
			background: C.accent,
			boxShadow: `0 0 6px ${C.accent}`,
			flexShrink: 0
		},
		miniStats: {
			background: C.statBg,
			border: `1px solid ${C.border}`,
			borderRadius: 10,
			padding: '12px 14px',
			display: 'flex',
			flexDirection: 'column',
			gap: 8,
			margin: '6px 0'
		},
		miniStatRow: {
			display: 'flex',
			justifyContent: 'space-between',
			alignItems: 'center'
		},
		logoutBtn: {
			display: 'flex',
			alignItems: 'center',
			gap: 8,
			padding: '10px 12px',
			borderRadius: 9,
			border: `1px solid ${C.logoutBorder}`,
			background: C.logoutBg,
			color: C.logoutColor,
			fontSize: 13,
			fontFamily: FONT_BODY,
			cursor: 'pointer',
			transition: 'all 0.15s'
		},
		main: {
			flex: 1,
			padding: '38px 44px',
			overflowY: 'auto',
			background: C.bg,
			transition: 'background 0.3s'
		},
		topbar: {
			display: 'flex',
			justifyContent: 'space-between',
			alignItems: 'flex-end',
			marginBottom: 30
		},
		pageTitle: {
			fontFamily: FONT_DISPLAY,
			fontSize: 28,
			fontWeight: 900,
			color: C.text,
			letterSpacing: '-0.04em',
			marginBottom: 3
		},
		pageSubtitle: {
			color: C.faint,
			fontSize: 13,
			margin: 0,
			fontFamily: FONT_BODY
		},
		themeBtn: {
			width: 36,
			height: 36,
			borderRadius: 10,
			border: `1px solid ${C.border}`,
			background: C.card,
			color: C.muted,
			cursor: 'pointer',
			fontSize: 16,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			transition: 'all 0.2s',
			flexShrink: 0
		},
		primaryBtn: {
			padding: '11px 22px',
			borderRadius: 10,
			border: 'none',
			background: `linear-gradient(135deg,${C.accent},${C.accentLt})`,
			color: '#fff',
			fontWeight: 700,
			fontSize: 14,
			fontFamily: FONT_DISPLAY,
			cursor: 'pointer',
			boxShadow: `0 6px 20px ${C.accent}44`,
			transition: 'all 0.15s',
			whiteSpace: 'nowrap'
		},
		ghostBtn: {
			padding: '11px 22px',
			borderRadius: 10,
			border: `1px solid ${C.border}`,
			background: 'transparent',
			color: C.muted,
			fontWeight: 500,
			fontSize: 14,
			fontFamily: FONT_BODY,
			cursor: 'pointer',
			transition: 'all 0.15s'
		},
		grid: {
			display: 'grid',
			gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
			gap: 16
		},
		taskCard: {
			background: C.card,
			border: `1px solid ${C.border}`,
			borderRadius: 14,
			padding: 20,
			display: 'flex',
			flexDirection: 'column',
			transition: 'background 0.15s, border-color 0.15s',
			cursor: 'default'
		},
		statCard: {
			background: C.card,
			border: `1px solid ${C.border}`,
			borderRadius: 14,
			padding: 22,
			transition: 'background 0.15s'
		},
		badge: {
			display: 'inline-flex',
			alignItems: 'center',
			padding: '4px 10px',
			borderRadius: 999,
			fontSize: 11,
			fontWeight: 700,
			fontFamily: FONT_BODY,
			letterSpacing: '0.01em'
		},
		iconBtn: color => ({
			width: 28,
			height: 28,
			borderRadius: 7,
			border: 'none',
			background: `${color}18`,
			color: color,
			cursor: 'pointer',
			fontSize: 13,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			transition: 'background 0.15s'
		}),
		iconBtnText: {
			fontSize: 11,
			fontWeight: 700,
			fontFamily: FONT_BODY,
			lineHeight: 1
		},
		detailsBtn: {
			marginTop: 'auto',
			padding: '8px 0',
			background: 'transparent',
			border: 'none',
			color: C.accent,
			fontSize: 13,
			fontWeight: 600,
			cursor: 'pointer',
			textAlign: 'left',
			fontFamily: FONT_BODY
		},
		emptyState: {
			display: 'flex',
			flexDirection: 'column',
			alignItems: 'center',
			justifyContent: 'center',
			padding: '80px 40px',
			textAlign: 'center'
		},
		spinner: {
			width: 34,
			height: 34,
			border: `3px solid ${C.accent}22`,
			borderTop: `3px solid ${C.accent}`,
			borderRadius: '50%',
			animation: 'spin 0.7s linear infinite'
		},
		formWrap: {
			maxWidth: 560,
			background: C.card,
			border: `1px solid ${C.border}`,
			borderRadius: 16,
			padding: 32,
			transition: 'background 0.3s'
		},
		form: { display: 'flex', flexDirection: 'column', gap: 20 },
		formField: { display: 'flex', flexDirection: 'column', gap: 8 },
		label: {
			fontSize: 11,
			fontWeight: 700,
			color: C.faint,
			textTransform: 'uppercase',
			letterSpacing: '0.09em',
			fontFamily: FONT_BODY
		},
		input: {
			width: '100%',
			padding: '12px 14px',
			borderRadius: 10,
			border: `1px solid ${C.border}`,
			background: C.inputBg,
			color: C.text,
			fontSize: 14,
			fontFamily: FONT_BODY,
			outline: 'none',
			boxSizing: 'border-box',
			transition: 'border-color 0.2s, background 0.2s, box-shadow 0.2s'
		},
		backBtn: {
			background: 'transparent',
			border: 'none',
			color: C.accent,
			fontSize: 14,
			fontWeight: 600,
			cursor: 'pointer',
			padding: 0,
			marginBottom: 20,
			fontFamily: FONT_BODY
		},
		section: {
			background: C.card,
			border: `1px solid ${C.border}`,
			borderRadius: 14,
			padding: 22,
			marginBottom: 18,
			transition: 'background 0.3s'
		},
		sectionLabel: {
			fontSize: 11,
			fontWeight: 700,
			color: C.faint,
			textTransform: 'uppercase',
			letterSpacing: '0.09em',
			marginBottom: 16,
			fontFamily: FONT_BODY
		},
		subtaskRow: {
			display: 'flex',
			alignItems: 'center',
			gap: 12,
			padding: '10px 0',
			borderBottom: `1px solid ${C.border}`
		},
		commentCard: {
			background: C.inputBg,
			border: `1px solid ${C.border}`,
			borderRadius: 10,
			padding: 14,
			marginBottom: 10
		},
		avatar: {
			width: 28,
			height: 28,
			borderRadius: '50%',
			background: `linear-gradient(135deg,${C.accent},${C.accentLt})`,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			fontSize: 12,
			fontWeight: 700,
			color: '#fff',
			flexShrink: 0
		},
		profileBtn: {
			display: 'flex',
			alignItems: 'center',
			gap: 10,
			padding: '8px 12px',
			borderRadius: 10,
			border: `1px solid ${C.border}`,
			background: C.card,
			cursor: 'pointer',
			transition: 'all 0.2s'
		},
		profileAvatar: {
			width: 36,
			height: 36,
			borderRadius: '50%',
			background: `linear-gradient(135deg,${C.accent},${C.accentLt})`,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			fontSize: 14,
			fontWeight: 700,
			color: '#fff',
			flexShrink: 0
		},
		profileMenu: {
			position: 'absolute',
			top: 'calc(100% + 8px)',
			right: 0,
			minWidth: 200,
			background: C.card,
			border: `1px solid ${C.border}`,
			borderRadius: 12,
			padding: '8px',
			boxShadow: `0 8px 24px ${C.accent}22`,
			zIndex: 1000
		},
		profileMenuItem: {
			width: '100%',
			display: 'flex',
			alignItems: 'center',
			gap: 10,
			padding: '10px 12px',
			borderRadius: 8,
			border: 'none',
			background: 'transparent',
			color: C.text,
			fontSize: 13,
			fontFamily: FONT_BODY,
			cursor: 'pointer',
			textAlign: 'left',
			transition: 'background 0.15s'
		},
		modalOverlay: {
			position: 'fixed',
			top: 0,
			left: 0,
			right: 0,
			bottom: 0,
			background: C.overlay,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			zIndex: 2000,
			backdropFilter: 'blur(4px)'
		},
		modalContent: {
			background: C.card,
			border: `1px solid ${C.border}`,
			borderRadius: 16,
			padding: 32,
			maxWidth: 480,
			width: '90%',
			maxHeight: '90vh',
			overflowY: 'auto',
			boxShadow: `0 20px 60px ${C.accent}33`
		},
		closeBtn: {
			width: 32,
			height: 32,
			borderRadius: 8,
			border: 'none',
			background: C.inputBg,
			color: C.muted,
			cursor: 'pointer',
			fontSize: 16,
			display: 'flex',
			alignItems: 'center',
			justifyContent: 'center',
			transition: 'all 0.15s'
		}
	}
}

/* spin keyframe injected once */
if (typeof document !== 'undefined' && !document.getElementById('tf-spin')) {
	const s = document.createElement('style')
	s.id = 'tf-spin'
	s.textContent = `@keyframes spin { to { transform: rotate(360deg); } }`
	document.head.appendChild(s)
}
