import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { registerUser } from '../services/api'
import { signUpSchema } from '../validation/authSchemas'
import '../App.css'

export default function SignUp() {
	const [loading, setLoading] = useState(false)
	const [apiError, setApiError] = useState('')
	const [theme, setTheme] = useState(
		() => localStorage.getItem('theme') || 'dark'
	)
	const navigate = useNavigate()

	const {
		register,
		handleSubmit,
		formState: { errors, isValid }
	} = useForm({
		resolver: zodResolver(signUpSchema),
		mode: 'onChange',
		defaultValues: {
			username: '',
			email: '',
			password: ''
		}
	})

	useEffect(() => {
		document.documentElement.setAttribute('data-theme', theme)
		localStorage.setItem('theme', theme)
	}, [theme])

	const toggleTheme = () => setTheme(t => (t === 'dark' ? 'light' : 'dark'))

	const onSubmit = async values => {
		setLoading(true)
		setApiError('')

		try {
			await registerUser(values)
			navigate('/', { state: { message: 'Account created. Please sign in.' } })
		} catch (err) {
			setApiError(err.message || 'Registration failed')
		} finally {
			setLoading(false)
		}
	}

	return (
		<div className="auth-wrapper">
			<div className="auth-noise" />

			<div className="auth-container">
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
						Create an account to manage tasks, projects, comments, and progress
						in one place.
					</p>
				</div>

				<div className="login-card">
					<button
						className="theme-toggle card-theme-toggle"
						onClick={toggleTheme}
						title={
							theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'
						}
						type="button"
					>
						{theme === 'dark' ? '☀' : '☾'}
					</button>

					<div className="login-header">
						<h2>Create account</h2>
						<p>Set up your workspace in minutes</p>
					</div>

					{apiError && <div className="error-alert">{apiError}</div>}

					<form onSubmit={handleSubmit(onSubmit)} noValidate>
						<div className="input-group">
							<label>Username</label>
							<input
								type="text"
								className="styled-input"
								placeholder="andrii_demchenko"
								autoComplete="username"
								{...register('username')}
							/>
							{errors.username && (
								<p className="field-error">{errors.username.message}</p>
							)}
						</div>

						<div className="input-group">
							<label>Email address</label>
							<input
								type="email"
								className="styled-input"
								placeholder="you@example.com"
								autoComplete="email"
								{...register('email')}
							/>
							{errors.email && (
								<p className="field-error">{errors.email.message}</p>
							)}
						</div>

						<div className="input-group">
							<label>Password</label>
							<input
								type="password"
								className="styled-input"
								placeholder="Create a strong password"
								autoComplete="new-password"
								{...register('password')}
							/>
							{errors.password && (
								<p className="field-error">{errors.password.message}</p>
							)}
						</div>

						<button
							type="submit"
							className="login-btn"
							disabled={!isValid || loading}
						>
							{loading ? 'Creating account…' : 'Create Account →'}
						</button>
					</form>

					<p className="footer-text">
						Already have an account? <Link to="/">Sign in</Link>
					</p>
				</div>
			</div>
		</div>
	)
}
