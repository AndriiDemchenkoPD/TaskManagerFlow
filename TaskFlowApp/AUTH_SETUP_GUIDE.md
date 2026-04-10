# Auth Setup Guide (React + ASP.NET Core + SQL + Docker)

## 1. SMTP mode

By default, Docker starts a local SMTP server (Mailpit), so password reset works without external email setup.

### Local development mode (recommended)

- No SMTP credentials required.
- Mail inbox UI: `http://localhost:8025`
- API sends to SMTP host `mailpit:1025` automatically.

### Real email mode (Gmail/SMTP provider)

1. Copy `.env.example` to `.env` in the `TaskFlowApp` root.
2. Fill SMTP values in `.env`.
3. For Gmail: enable 2FA and create an App Password.

Keys:

- `SMTP_HOST`
- `SMTP_PORT`
- `SMTP_ENABLE_SSL`
- `SMTP_FROM`
- `SMTP_USERNAME`
- `SMTP_PASSWORD`
- `AUTH_RESET_PASSWORD_URL` (optional, default: `http://localhost:5173/reset-password`)

## 2. Run services

```bash
docker compose down
docker compose up --build -d
```

## 3. Verify email service

- Check status:
  - `GET http://localhost:5022/api/auth/email-status`
- Send test email:
  - `POST http://localhost:5022/api/auth/email-test`
  - body:

```json
{
	"email": "your_email@gmail.com"
}
```

## 4. Validate forgot/reset flow

1. `POST /api/auth/forgot-password` with account email.
2. Open reset email:

- Local mode: Mailpit inbox at `http://localhost:8025`
- Real SMTP mode: your mailbox

3. Submit new password on reset page.
4. Login using updated password.

## Common status codes (expected behavior)

- `409 /api/auth/register`: email already exists.
- `401 /api/auth/login`: invalid credentials.
- `200 /api/auth/forgot-password`: always returned for security when request is valid.

## Security notes

- Forgot-password endpoint is rate-limited by IP (5 requests / 15 minutes).
- Reset tokens are one-time and expire in 15 minutes.
- Passwords are stored as BCrypt hashes.
- Legacy SHA256 user passwords are migrated to BCrypt after successful login.
