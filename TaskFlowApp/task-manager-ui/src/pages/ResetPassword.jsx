import { useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { resetPassword } from '../services/api'
import { resetPasswordSchema } from '../validation/authSchemas'
import '../App.css'

export default function ResetPassword() {
	const [searchParams] = useSearchParams()
	const [loading, setLoading] = useState(false)
	const [apiError, setApiError] = useState('')
	const [successMessage, setSuccessMessage] = useState('')
	const [theme, setTheme] = useState(
		() => localStorage.getItem('theme') || 'dark'
	)

	const token = useMemo(() => searchParams.get('token') || '', [searchParams])

	const {
		register,
		handleSubmit,
		formState: { errors, isValid }
	} = useForm({
		resolver: zodResolver(resetPasswordSchema),
		mode: 'onChange',
		defaultValues: {
			newPassword: '',
			confirmPassword: ''
		}
	})

	useEffect(() => {
		document.documentElement.setAttribute('data-theme', theme)
		localStorage.setItem('theme', theme)
	}, [theme])

	const toggleTheme = () => setTheme(t => (t === 'dark' ? 'light' : 'dark'))

	const onSubmit = async values => {
		if (!token) {
			setApiError('Reset token is missing')
			return
		}

		setLoading(true)
		setApiError('')
		setSuccessMessage('')

		try {
			const result = await resetPassword({
				token,
				newPassword: values.newPassword,
				confirmPassword: values.confirmPassword
			})

			setSuccessMessage(
				result.message || 'Password has been reset successfully'
			)
		} catch (err) {
			setApiError(err.message || 'Reset link is invalid or expired')
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
						Set your
						<br />
						new
						<br />
						password.
					</h1>
					<p>
						Use a strong password with uppercase, lowercase, number, and a
						special character.
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
						<h2>Reset password</h2>
						<p>Create your new password</p>
					</div>

					{apiError && <div className="error-alert">{apiError}</div>}
					{successMessage && (
						<div className="success-alert">{successMessage}</div>
					)}

					<form onSubmit={handleSubmit(onSubmit)} noValidate>
						<div className="input-group">
							<label>New password</label>
							<input
								type="password"
								className="styled-input"
								placeholder="Enter new password"
								autoComplete="new-password"
								{...register('newPassword')}
							/>
							{errors.newPassword && (
								<p className="field-error">{errors.newPassword.message}</p>
							)}
						</div>

						<div className="input-group">
							<label>Confirm password</label>
							<input
								type="password"
								className="styled-input"
								placeholder="Confirm new password"
								autoComplete="new-password"
								{...register('confirmPassword')}
							/>
							{errors.confirmPassword && (
								<p className="field-error">{errors.confirmPassword.message}</p>
							)}
						</div>

						<button
							type="submit"
							className="login-btn"
							disabled={!isValid || loading || !token}
						>
							{loading ? 'Resetting…' : 'Reset Password →'}
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
