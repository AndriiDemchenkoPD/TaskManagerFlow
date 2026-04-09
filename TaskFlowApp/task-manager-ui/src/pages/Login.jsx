import { useState, useEffect } from 'react'
import { loginUser } from '../services/api'
import { useNavigate } from 'react-router-dom'
import '../App.css'

export default function Login() {
	const [username, setUsername] = useState('')
	const [password, setPassword] = useState('')
	const [loading, setLoading] = useState(false)
	const [error, setError] = useState('')
	const [theme, setTheme] = useState(
		() => localStorage.getItem('theme') || 'dark'
	)
	const navigate = useNavigate()

	/* apply theme to <html> */
	useEffect(() => {
		document.documentElement.setAttribute('data-theme', theme)
		localStorage.setItem('theme', theme)
	}, [theme])

	const toggleTheme = () => setTheme(t => (t === 'dark' ? 'light' : 'dark'))

	const handleLogin = async e => {
		e.preventDefault()
		setLoading(true)
		setError('')
		try {
			const result = await loginUser({ username, password })
			localStorage.setItem('token', result.token)
			navigate('/dashboard')
		} catch (err) {
			setError(err.message || 'Invalid username or password. Please try again.')
		} finally {
			setLoading(false)
		}
	}

	return (
		<div className="auth-wrapper">
			<div className="auth-noise" />

			<div className="auth-container">
				{/* LEFT — Brand */}
				<div className="auth-brand">
					<div className="auth-brand-logo">
						<div className="auth-brand-icon">✦</div>
						<span className="auth-brand-name">Taskflow</span>
					</div>

					<h1>
						Your work,
						<br />
						beautifully
						<br />
						organized.
					</h1>
					<p>
						Manage tasks, track progress, and stay on top of every project — all
						in one focused workspace.
					</p>

					<div className="auth-brand-stats">
						<div className="auth-stat">
							<span className="auth-stat-value">10k+</span>
							<span className="auth-stat-label">Tasks done</span>
						</div>
						<div className="auth-stat">
							<span className="auth-stat-value">99%</span>
							<span className="auth-stat-label">Uptime</span>
						</div>
						<div className="auth-stat">
							<span className="auth-stat-value">∞</span>
							<span className="auth-stat-label">Projects</span>
						</div>
					</div>
				</div>

				{/* RIGHT — Form */}
				<div className="login-card">
					{/* Theme toggle */}
					<button
						className="theme-toggle card-theme-toggle"
						onClick={toggleTheme}
						title={
							theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'
						}
					>
						{theme === 'dark' ? '☀' : '☾'}
					</button>

					<div className="login-header">
						<h2>Welcome back</h2>
						<p>Sign in to your workspace</p>
					</div>

					{error && <div className="error-alert">{error}</div>}

					<form onSubmit={handleLogin}>
						<div className="input-group">
							<label>Username or Email</label>
							<input
								type="text"
								className="styled-input"
								placeholder="Enter your username or email"
								value={username}
								onChange={e => setUsername(e.target.value)}
								required
								autoComplete="username"
							/>
						</div>

						<div className="input-group">
							<div className="input-row">
								<label>Password</label>
								<a href="#" className="forgot-link">
									Forgot password?
								</a>
							</div>
							<input
								type="password"
								className="styled-input"
								placeholder="••••••••"
								value={password}
								onChange={e => setPassword(e.target.value)}
								required
								autoComplete="current-password"
							/>
						</div>

						<button type="submit" className="login-btn" disabled={loading}>
							{loading ? 'Signing in…' : 'Sign In →'}
						</button>
					</form>

					<p className="footer-text">
						No account yet? <a href="/register">Create one free</a>
					</p>
				</div>
			</div>
		</div>
	)
}
