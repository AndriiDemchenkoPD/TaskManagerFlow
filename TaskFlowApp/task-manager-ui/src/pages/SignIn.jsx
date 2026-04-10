import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { loginUser } from '../services/api'
import { signInSchema } from '../validation/authSchemas'
import '../App.css'

export default function SignIn() {
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
		resolver: zodResolver(signInSchema),
		mode: 'onChange',
		defaultValues: {
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
			const result = await loginUser(values)
			localStorage.setItem('token', result.token)
			navigate('/dashboard')
		} catch (err) {
			setApiError(err.message || 'Invalid credentials')
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
						Your work,
						<br />
						beautifully
						<br />
						organized.
					</h1>
					<p>
						Sign in to continue planning tasks, projects, and deadlines in one
						focused workspace.
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
						<h2>Welcome back</h2>
						<p>Sign in to your account</p>
					</div>

					{apiError && <div className="error-alert">{apiError}</div>}

					<form onSubmit={handleSubmit(onSubmit)} noValidate>
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
							<div className="input-row">
								<label>Password</label>
								<Link to="/forgot-password" className="forgot-link">
									Forgot password?
								</Link>
							</div>
							<input
								type="password"
								className="styled-input"
								placeholder="••••••••"
								autoComplete="current-password"
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
							{loading ? 'Signing in…' : 'Sign In →'}
						</button>
					</form>

					<p className="footer-text">
						No account yet? <Link to="/signup">Create one</Link>
					</p>
				</div>
			</div>
		</div>
	)
}
