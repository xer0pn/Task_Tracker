# Task Tracker

A small full-stack task tracker built for the developer take-home assignment. It uses a Vue 3 frontend, an ASP.NET Core Web API, EF Core, and SQLite.

## Requirements

- .NET SDK 10 or later
- Node.js 20 or later
- npm 10 or later

SQLite is created as a local `tasktracker.db` file, so no separate database server is required.

## Optional AI assistant

The dashboard includes an optional assistant that turns a natural-language request into a reviewable task draft. It does not write to the database until the user confirms the draft.

To enable it locally, copy the root environment template and add a key:

```bash
cp .env.example .env
```

The Docker Compose configuration passes `OPENAI_API_KEY` and `OPENAI_MODEL` to the API. Without a key, the assistant returns a controlled temporary-unavailable state; the core task tracker continues to work normally.

The assistant uses structured JSON output with a strict schema. The API still validates and normalizes priority casing/synonyms, parses dates, and maps categories case-insensitively. A category that does not exist is never created automatically: the draft asks the user to choose an existing category before saving.

## Run the API

```bash
cd backend/TaskTracker.Api
dotnet restore
dotnet run
```

The API runs at `http://localhost:5172`. OpenAPI is available at `http://localhost:5172/openapi/v1.json` in development.

The application applies the checked-in EF Core migration on startup and seeds three categories and ten sample tasks. To create or apply migrations manually:

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Run the frontend

In a second terminal:

```bash
cd frontend
npm install
cp .env.example .env
npm run dev
```

Open the Vite URL shown in the terminal, normally `http://localhost:5173`.

## Run with Docker Compose

Start Docker Desktop, then run this from the repository root:

```bash
docker compose up --build
```

Open `http://localhost:5173`. The frontend container serves the Vue app and proxies `/api` requests to the API container. SQLite is stored in the named `tasktracker-data` volume, so the database survives container restarts. The API applies the checked-in migration and seed data when it starts.

After changing code, rebuild the affected service:

```bash
docker compose up --build frontend  # frontend changes
docker compose up --build api       # backend changes
docker compose up --build           # changes affecting both services
```

Stop the containers with `Ctrl+C`. To run them in the background, use `docker compose up --build -d`, then stop them with `docker compose down`.

## Project structure

- `backend/TaskTracker.Api/Controllers` contains HTTP routing and response behavior.
- `backend/TaskTracker.Api/DTOs` defines the public API contract without exposing EF entities.
- `backend/TaskTracker.Api/Entities` contains the database model and enums.
- `backend/TaskTracker.Api/Data` configures EF Core, relationships, and seed data.
- `backend/TaskTracker.Api/Services` contains database and business operations.
- `frontend/src/views` contains the task and category screens.
- `frontend/src/services/api.js` is the only frontend HTTP integration point.

Controllers are used instead of Minimal APIs because the application has several related endpoints, DTOs, validation rules, and service dependencies. The explicit controller structure makes responsibilities easier to review.

## Core features

- Task list with search, status/category filters, and pagination
- Create, edit, quick status update, and delete task flows
- Category list with task counts and category creation
- Client and server validation
- Loading, empty, and API error states
- SQLite persistence with EF Core migrations and seed data
- OpenAPI documentation, CORS, and global error handling

## Assumptions and limitations

- This is a single-user local application; authentication is not included in the core version.
- A task must belong to an existing category.
- Category deletion is not exposed because it was not required and could orphan tasks.
- Dates are stored as UTC timestamps by the API and displayed in the browser's local format.
- Page size is limited to 50 records per request.
- AI task drafting is optional and depends on an OpenAI API key; normal task creation remains available without it.

## AI usage

AI tools were used to help plan the architecture, draft repetitive scaffolding, review implementation details, and identify build issues. The submitted code is reviewed as part of the development process and is intended to be explainable during the follow-up call.

## Verification

```bash
cd backend/TaskTracker.Api
dotnet build

cd ../TaskTracker.Api.Tests
dotnet test

cd ../../frontend
npm run build
```

The API test project uses `WebApplicationFactory` with an isolated in-memory SQLite database. It exercises the real HTTP pipeline and verifies task listing, filtering, paging, lookup, creation, updates, status changes, deletion, validation errors, category counts, duplicate-category conflicts, and assistant input validation.
