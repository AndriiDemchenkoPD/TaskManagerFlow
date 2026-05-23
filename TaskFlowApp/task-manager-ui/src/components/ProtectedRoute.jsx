import { Navigate } from 'react-router-dom'

function isTokenExpired(token) {
	try {
		const payload = JSON.parse(atob(token.split('.')[1] || ''))
		if (!payload?.exp) {
			return true
		}

		return payload.exp * 1000 <= Date.now()
	} catch {
		return true
	}
}

export default function ProtectedRoute({ children }) {
	const token = localStorage.getItem('token')

	if (!token || isTokenExpired(token)) {
		localStorage.removeItem('token')
		return <Navigate to="/" />
	}

	return children
}
