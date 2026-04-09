const API_BASE = 'http://localhost:5022/api'

/* ================= AUTH ================= */

export async function loginUser(data) {
	const response = await fetch(`${API_BASE}/auth/login`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(data)
	})

	if (!response.ok) throw new Error('Login failed')

	return response.json()
}

export async function registerUser(data) {
	const response = await fetch(`${API_BASE}/auth/register`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(data)
	})

	if (!response.ok) {
		const errorData = await response.json().catch(() => ({}))
		throw new Error(errorData.message || 'Registration failed')
	}

	return response.json()
}

/* ================= TOKEN HELPER ================= */

function authHeader() {
	return {
		'Content-Type': 'application/json',
		Authorization: `Bearer ${localStorage.getItem('token')}`
	}
}

/* ================= TASK APIs ================= */

export async function getTasks() {
	const res = await fetch(`${API_BASE}/task`, {
		headers: authHeader()
	})

	if (!res.ok) throw new Error('Failed to load tasks')

	return res.json()
}

export async function addTask(task) {
	const res = await fetch(`${API_BASE}/task`, {
		method: 'POST',
		headers: authHeader(),
		body: JSON.stringify(task)
	})

	if (!res.ok) throw new Error('Add task failed')
}

export async function deleteTask(id) {
	const res = await fetch(`${API_BASE}/task/${id}`, {
		method: 'DELETE',
		headers: authHeader()
	})

	if (!res.ok) throw new Error('Delete failed')
}

/* ================= USER PROFILE APIs ================= */

export async function getUserProfile() {
	const res = await fetch(`${API_BASE}/user/profile`, {
		headers: authHeader()
	})

	if (!res.ok) throw new Error('Failed to load profile')

	return res.json()
}

export async function updateUserProfile(data) {
	const res = await fetch(`${API_BASE}/user/profile`, {
		method: 'PUT',
		headers: authHeader(),
		body: JSON.stringify(data)
	})

	if (!res.ok) throw new Error('Failed to update profile')

	return res.json()
}
