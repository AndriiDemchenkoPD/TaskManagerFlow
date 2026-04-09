import { useState, useEffect } from 'react'
import { registerUser } from '../services/api'
import { useNavigate } from 'react-router-dom'
import '../App.css'

export default function Register() {
	const [username, setUsername] = useState('')
	const [email, setEmail] = useState('')
	const [password, setPassword] = useState('')
	const [loading, setLoading] = useState(false)
	const [error, setError] = useState('')
	const [theme, setTheme] = useState(
		() => localStorage.getItem('theme') || 'dark'
	)
	const navigate = useNavigate()

	useEffect(() => {
		document.documentElement.setAttribute('data-theme', theme)
		localStorage.setItem('theme', theme)
	}, [theme])

	const toggleTheme = () => setTheme(t => (t === 'dark' ? 'light' : 'dark'))

	const handleRegister = async e => {
		e.preventDefault()
		setLoading(true)
		setError('')
		try {
			await registerUser({ username, email, password })
			navigate('/')
		} catch (err) {
			setError(err.message || 'Registration failed. Please try again.')
			console.error('Registration error:', err)
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
						Start doing
						<br />
						more with
						<br />
						less friction.
					</h1>
					<p>
						Create your account and unlock a smarter way to manage tasks,
						collaborate on projects, and hit every deadline with confidence.
					</p>

					<div className="auth-brand-stats">
						<div className="auth-stat">
							<span className="auth-stat-value">Free</span>
							<span className="auth-stat-label">To start</span>
						</div>
						<div className="auth-stat">
							<span className="auth-stat-value">2 min</span>
							<span className="auth-stat-label">Setup</span>
						</div>
						<div className="auth-stat">
							<span className="auth-stat-value">∞</span>
							<span className="auth-stat-label">Tasks</span>
						</div>
					</div>
				</div>

				{/* RIGHT — Form */}
				<div className="login-card">
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
						<h2>Create account</h2>
						<p>Get started in under 2 minutes</p>
					</div>

					{error && <div className="error-alert">{error}</div>}

					<form onSubmit={handleRegister}>
						<div className="input-group">
							<label>Username</label>
							<input
								type="text"
								className="styled-input"
								placeholder="Choose a username"
								value={username}
								onChange={e => setUsername(e.target.value)}
								required
								autoComplete="username"
							/>
						</div>

						<div className="input-group">
							<label>Email address</label>
							<input
								type="email"
								className="styled-input"
								placeholder="you@example.com"
								value={email}
								onChange={e => setEmail(e.target.value)}
								required
								autoComplete="email"
							/>
						</div>

						<div className="input-group">
							<label>Password</label>
							<input
								type="password"
								className="styled-input"
								placeholder="Create a strong password"
								value={password}
								onChange={e => setPassword(e.target.value)}
								required
								autoComplete="new-password"
							/>
						</div>

						<button type="submit" className="login-btn" disabled={loading}>
							{loading ? 'Creating account…' : 'Create Account →'}
						</button>
					</form>

					<p className="footer-text">
						Already have an account? <a href="/">Sign in</a>
					</p>
				</div>
			</div>
		</div>
	)
}
