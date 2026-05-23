# TaskFlowApp - Full Stack Task Management Platform

TaskFlowApp is a full-stack task management system built with React, ASP.NET Core Web API, and SQL Server.
It supports secure authentication, user-specific workspaces, and advanced planning features for daily productivity.

## Tech Stack

### Frontend

- React 19 (Vite)
- React Router
- Axios
- Tailwind CSS
- Framer Motion

### Backend

- ASP.NET Core Web API (.NET)
- C#
- JWT Authentication

### Database

- Microsoft SQL Server 2022
- SQL scripts for schema and seed setup

### DevOps

- Docker and Docker Compose

## Core Features

- User registration and login
- JWT-based protected APIs
- User profile management
- Task CRUD with soft delete
- Projects management
- Subtasks management
- Tags management
- Comments on tasks
- Dashboard and analytics endpoints
- Audit logging support

## Architecture

```text
React Frontend (task-manager-ui)
  |
  v
ASP.NET Core Web API (TaskManagerApi)
  |
  v
SQL Server (BasicTraining database)
```

## Project Structure

```text
TaskFlowApp/
|-- task-manager-ui/       # React frontend
|-- TaskManagerApi/        # ASP.NET Core Web API
|-- docker-compose.yml     # Full stack local containers
|-- README.md
|-- QUICK_SETUP_GUIDE.md
|-- API_REFERENCE.md
```

## Authentication Flow

1. User registers or logs in.
2. API validates credentials.
3. API returns JWT token.
4. Frontend stores token in local storage.
5. Frontend sends Bearer token for protected routes.

## Quick Start

### Option 1: Run with Docker (recommended)

From the `TaskFlowApp` directory:

```bash
docker compose up --build
```

Services:

- UI: http://localhost:5173
- API: http://localhost:5022
- SQL Server: localhost:1433

### Option 2: Run manually

#### 1. Start API

```bash
cd TaskManagerApi
dotnet restore
dotnet run
```

API default URL:

- http://localhost:5022

#### 2. Start UI

```bash
cd task-manager-ui
npm install
npm run dev
```

UI default URL:

- http://localhost:5173

## Database Notes

- SQL Server is used as the primary database.
- Main entities include users, tasks, projects, subtasks, tags, comments, and audit records.
- Docker startup executes SQL initialization scripts from `TaskManagerApi/SQL`.

## API and Documentation

Additional documentation is included in this repository:

- `API_REFERENCE.md`
- `QUICK_SETUP_GUIDE.md`
- `PROJECT_STRUCTURE.md`

## Author

AndriiDemchenko

## License

This project is for educational and portfolio use.
