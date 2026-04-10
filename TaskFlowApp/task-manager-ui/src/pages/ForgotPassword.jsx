import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { forgotPassword } from '../services/api'
import { forgotPasswordSchema } from '../validation/authSchemas'
import '../App.css'

export default function ForgotPassword() {
	const [loading, setLoading] = useState(false)
	const [apiError, setApiError] = useState('')
	const [successMessage, setSuccessMessage] = useState('')
	const [theme, setTheme] = useState(
		() => localStorage.getItem('theme') || 'dark'
	)

	const {
		register,
		handleSubmit,
		formState: { errors, isValid }
	} = useForm({
		resolver: zodResolver(forgotPasswordSchema),
		mode: 'onChange',
		defaultValues: {
			email: ''
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
		setSuccessMessage('')

		try {
			const result = await forgotPassword(values)
			setSuccessMessage(
				(result.message ||
					'If the account exists, a reset link has been sent.') +
					' In local Docker mode, open Mailpit inbox at http://localhost:8025.'
			)
		} catch (err) {
			setApiError(err.message || 'Unable to process request')
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
						Recover
						<br />
						your
						<br />
						account.
					</h1>
					<p>
						Enter your email and we will send a secure reset link if the account
						exists.
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
						<h2>Forgot password</h2>
						<p>We will email a reset link</p>
					</div>

					{apiError && <div className="error-alert">{apiError}</div>}
					{successMessage && (
						<div className="success-alert">{successMessage}</div>
					)}

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

						<button
							type="submit"
							className="login-btn"
							disabled={!isValid || loading}
						>
							{loading ? 'Sending link…' : 'Send Reset Link →'}
						</button>
					</form>

					<p className="footer-text">
						Back to <Link to="/">Sign in</Link>
					</p>
				</div>
			</div>
		</div>
	)
}
