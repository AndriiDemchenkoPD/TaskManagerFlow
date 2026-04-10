import { z } from 'zod'

const usernameRegex = /^[A-Za-z0-9_]+$/
const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/

export const signUpSchema = z.object({
	username: z
		.string()
		.min(3, 'Username must be at least 3 characters')
		.max(20, 'Username must be at most 20 characters')
		.regex(usernameRegex, 'Use letters, numbers, and underscores only'),
	email: z.string().email('Enter a valid email address'),
	password: z
		.string()
		.min(8, 'Password must be at least 8 characters')
		.regex(
			passwordRegex,
			'Use uppercase, lowercase, number, and special character'
		)
})

export const signInSchema = z.object({
	email: z.string().email('Enter a valid email address'),
	password: z.string().min(1, 'Password is required')
})

export const forgotPasswordSchema = z.object({
	email: z.string().email('Enter a valid email address')
})

export const resetPasswordSchema = z
	.object({
		newPassword: z
			.string()
			.min(8, 'Password must be at least 8 characters')
			.regex(
				passwordRegex,
				'Use uppercase, lowercase, number, and special character'
			),
		confirmPassword: z.string().min(1, 'Confirm password is required')
	})
	.refine(data => data.newPassword === data.confirmPassword, {
		path: ['confirmPassword'],
		message: 'Passwords do not match'
	})
