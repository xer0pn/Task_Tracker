# Task Tracker — Product Requirements Document

**Project type:** Developer take-home application  
**Status:** Draft for implementation  
**Date:** 2026-10-14  
**Primary goal:** Deliver a small, reliable, explainable task-management application that demonstrates full-stack development ability.

## 1. Product summary

Task Tracker is a web application for creating, organizing, filtering, updating, and deleting tasks. Users manage tasks through a Vue 3 frontend, which communicates with an ASP.NET Core REST API. The API persists data in a relational database through Entity Framework Core.

The application should prioritize a complete end-to-end flow, clean architecture, correct REST behavior, and easy setup over visual complexity or a large feature set.

## 2. Objectives

### 2.1 Required objectives

- A user can view, search, filter, paginate, create, edit, update status, and delete tasks.
- A user can view categories and create a category.
- The frontend, API, and database work together on a fresh machine using documented setup steps.
- The API follows conventional routes, HTTP methods, status codes, DTO boundaries, and validation behavior.
- Database queries perform filtering and paging in the database rather than loading all records into memory.
- The codebase is structured so that controllers, services, persistence, and UI components have clear responsibilities.
- The implementation is understandable enough to explain during a technical follow-up call.

### 2.2 Non-goals for the first version

- User accounts, authentication, and authorization.
- Multi-user collaboration.
- File attachments, comments, notifications, or recurring tasks.
- Advanced drag-and-drop task management.
- Production deployment infrastructure.
- Bonus features until all core requirements are complete and tested.

## 3. Users and primary scenarios

### 3.1 Target user

An individual who wants a simple way to maintain a categorized personal task list.

### 3.2 Core scenarios

1. The user opens the task list and sees existing tasks.
2. The user searches for a task by title.
3. The user filters tasks by status and category.
4. The user moves between pages of tasks.
5. The user creates a task with valid information.
6. The user edits an existing task.
7. The user changes a task’s status directly from the task list.
8. The user deletes a task after confirming the action.
9. The user opens the categories page and sees each category’s task count.
10. The user creates a new category and receives a clear error if its name already exists.

## 4. Product scope

### 4.1 Task list page

The page must display tasks in a table or card layout with:

- Title
- Category
- Status
- Priority
- Due date
- An edit action
- A delete action
- A quick status-change control

The page must also provide:

- Search by title
- Status filter
- Category filter
- Pagination
- Loading state
- Empty state with the message “No tasks found”
- User-friendly API error state

### 4.2 Create and edit task form

Fields:

- Title — required, maximum 200 characters
- Description — optional
- Status — Todo, InProgress, or Done
- Priority — Low, Medium, or High
- Due date — optional
- Category — required foreign-key selection

Behavior:

- Client-side validation runs before submission.
- Server validation errors are shown to the user.
- Successful creation returns the user to the task list or shows a clear success state.
- Successful editing updates the displayed task.
- The form does not directly call HTTP methods; calls go through the frontend API service.

### 4.3 Categories page

The page must:

- List all categories.
- Display the number of tasks in each category.
- Provide a form to add a category.
- Validate that the name is present and no longer than 50 characters.
- Display a clear duplicate-name error when the API returns HTTP 409.

### 4.4 Navigation

Vue Router must provide at least these routes:

- `/tasks` — task list
- `/tasks/new` — create task
- `/tasks/:id/edit` — edit task
- `/categories` — categories list and create form

## 5. Data model

### 5.1 Category

| Field | Type | Rules |
|---|---|---|
| Id | Integer or UUID | Primary key |
| Name | String | Required, unique, maximum 50 characters |

### 5.2 Task

| Field | Type | Rules |
|---|---|---|
| Id | Integer or UUID | Primary key |
| Title | String | Required, maximum 200 characters |
| Description | String | Optional |
| Status | Enum/string | Todo, InProgress, or Done |
| Priority | Enum/string | Low, Medium, or High |
| DueDate | Date/time | Optional |
| CategoryId | Integer or UUID | Required foreign key |
| CreatedAt | Date/time | Set by the server |
| UpdatedAt | Date/time | Set by the server on create/update |

Relationship: one Category has many Tasks. A task must belong to one category. Deleting a category should be restricted or explicitly handled; the initial product should not silently delete its tasks.

### 5.3 Seed data

The application must seed three categories and approximately ten sample tasks. Seed data must be safe to run repeatedly without creating duplicates.

## 6. API requirements

Base route: `/api`

### 6.1 Task endpoints

| Method | Route | Behavior | Success |
|---|---|---|---|
| GET | `/api/tasks` | List tasks with filters and paging | 200 |
| GET | `/api/tasks/{id}` | Return one task | 200; 404 if missing |
| POST | `/api/tasks` | Create a task | 201 with `Location` header |
| PUT | `/api/tasks/{id}` | Replace editable task data | 200 or 204; 404 if missing |
| PATCH | `/api/tasks/{id}/status` | Change only status | 200 or 204; 404 if missing |
| DELETE | `/api/tasks/{id}` | Delete a task | 204; 404 if missing |

`GET /api/tasks` query parameters:

- `status` — optional status filter
- `categoryId` — optional category filter
- `search` — optional case-insensitive title search
- `page` — optional page number, default 1
- `pageSize` — optional page size, default 10, with a safe maximum

The list response should include the task collection plus pagination metadata such as total count, current page, page size, and total pages.

### 6.2 Category endpoints

| Method | Route | Behavior | Success |
|---|---|---|---|
| GET | `/api/categories` | List categories with task counts | 200 |
| POST | `/api/categories` | Create a category | 201; 409 for duplicate name |

### 6.3 API behavior rules

- Use request and response DTOs; never expose EF Core entities directly from controllers.
- Validate all input and return HTTP 400 with clear field-level messages.
- Return HTTP 404 when a requested record does not exist.
- Return HTTP 409 when creating a duplicate category name.
- Use async/await for database and API operations.
- Keep database logic in services or repositories, not controllers.
- Use dependency injection for services and `DbContext`.
- Enable CORS for the Vue development origin.
- Expose Swagger/OpenAPI when the API runs.
- Add global exception handling that returns consistent JSON and never exposes stack traces.
- Use a consistent error shape, for example:

```json
{
  "message": "Validation failed.",
  "errors": {
    "title": ["Title is required."]
  }
}
```

## 7. Technical decisions

These are implementation decisions for this project, not additional employer requirements.

### Backend

- ASP.NET Core Web API on .NET 8 or later.
- Controllers rather than Minimal APIs, because the endpoint set benefits from explicit separation and the structure is easy to explain in the interview.
- Entity Framework Core with migrations.
- SQLite for simple local setup and a file-based relational database.
- Service layer between controllers and EF Core.
- Data annotations for straightforward validation unless a rule becomes easier to express with FluentValidation.
- Swagger/OpenAPI enabled in development.

### Frontend

- Vue 3 with Composition API and Vite.
- Vue Router.
- Axios or the native Fetch API through one centralized `services/api` module. The preferred initial choice is Axios if it is already available; otherwise Fetch keeps dependencies smaller.
- Plain CSS or a small CSS module approach. Avoid adding a UI framework unless it materially improves speed without hiding behavior we need to explain.
- API base URL read from `.env` through Vite environment variables.

### Repository structure

```text
/
├── backend/
│   ├── TaskTracker.sln
│   └── TaskTracker.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── DTOs/
│       ├── Entities/
│       ├── Services/
│       ├── Migrations/
│       └── Program.cs
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── views/
│   │   ├── services/
│   │   ├── router/
│   │   └── stores/
│   └── .env.example
├── PRD.md
└── README.md
```

## 8. Quality and acceptance criteria

### 8.1 End-to-end acceptance

- A fresh clone can be started by following the README within approximately 15 minutes.
- The seeded categories and tasks appear in the UI.
- Creating a task through the UI persists it to the database and it remains after restarting the API.
- Editing, status changes, filtering, searching, paging, and deletion work through the API and UI.
- Category counts reflect the related task records.

### 8.2 Backend acceptance

- Every required endpoint exists at the documented route.
- Correct HTTP methods and success/error status codes are returned.
- DTOs are used for input and output.
- Filtering and pagination are performed by the database query.
- Missing records return 404.
- Duplicate category names return 409.
- Invalid payloads return 400 with understandable messages.
- Unexpected errors return a clean JSON response.
- Swagger documents the endpoints.

### 8.3 Frontend acceptance

- The task list clearly exposes the required information and actions.
- Filters and search can be combined with pagination.
- Create and edit forms validate input before sending requests.
- API errors are visible and actionable.
- Loading and empty states are visible.
- API requests are centralized in a service module.
- The API base URL is configurable through `.env`.

### 8.4 Code and submission acceptance

- No secrets or real credentials are committed.
- The repository contains meaningful incremental commits.
- The README documents setup, migrations, seed data, structure, assumptions, AI usage, limitations, and completed bonuses.
- Three to five screenshots or a two-to-three-minute walkthrough are included.
- The author can explain the save flow, REST choices, dependency injection, EF Core scope, Vue reactivity, and the project structure.

## 9. Error and edge-case requirements

- Empty task results show “No tasks found” rather than a blank screen.
- Invalid or missing task IDs return 404 or a suitable 400 response.
- Whitespace-only names and titles are rejected.
- Duplicate category names are handled case-insensitively where practical.
- Page numbers below 1 are normalized or rejected consistently.
- Page sizes are bounded to prevent unreasonably large responses.
- A task cannot be created with a nonexistent category.
- Network failures show a recoverable message and do not crash the Vue application.
- Delete requires explicit confirmation.
- Optional dates are handled consistently across the API and UI.

## 10. Security and privacy

- Do not commit API keys, passwords, or production connection strings.
- Use local configuration or user secrets for any future external service credentials.
- Do not expose stack traces, SQL details, or sensitive configuration in API responses.
- Keep CORS limited to the known frontend development origin rather than allowing all origins by default in the documented configuration.
- Authentication is out of scope for the core version.

## 11. Bonus strategy

The first bonus to consider is the dashboard because it adds visible value without external credentials. It should only be attempted after the core application is working.

Possible order after core completion:

1. Dashboard with status/priority counts and overdue tasks.
2. Unit tests for the service layer.
3. Arabic/English toggle with RTL support.
4. Docker Compose.
5. AI task assistant only if there is enough time to implement safe JSON handling and explain the design.
6. JWT login only if the rest of the application is already polished.

## 12. Delivery plan

### Milestone 1 — Foundation

- Initialize repository structure.
- Create ASP.NET Core API.
- Create Vue/Vite frontend.
- Add initial README and `.gitignore`.
- Make the first meaningful commit.

### Milestone 2 — Data and API

- Add entities, enums, `DbContext`, migration, and seed data.
- Add DTOs and validation.
- Implement category endpoints.
- Implement task endpoints, filtering, search, and paging.
- Add service layer, CORS, Swagger, and global exception handling.
- Verify endpoints with Swagger or an HTTP client.

### Milestone 3 — Frontend core

- Add router and application layout.
- Build task list with filters, search, paging, and states.
- Build create/edit form.
- Add quick status update and delete confirmation.
- Build categories page.
- Connect all views through the API service.

### Milestone 4 — Verification and polish

- Test the complete UI-to-database flow.
- Test validation, 404, 409, empty, loading, and network-error states.
- Improve accessibility and responsive layout.
- Add screenshots or a walkthrough recording.
- Update README and document assumptions and AI usage.
- Make final cleanup commits.

### Milestone 5 — Optional bonus

- Choose one bonus only if all core acceptance criteria pass.
- Add tests for the bonus and document it separately.

## 13. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Spending too long on styling | Use a clean, restrained layout and prioritize behavior. |
| Newness to ASP.NET Core or Vue | Keep the architecture conventional and explain each layer as it is built. |
| Integration bugs late in the task | Test each endpoint before building the corresponding UI. |
| Overcommitting to bonuses | Do not start a bonus until core functionality is complete. |
| Inability to explain AI-generated code | Review and rewrite unfamiliar code before committing it. |
| Fresh-machine setup failure | Rehearse the README setup from a clean environment before submission. |

## 14. Assumptions to document in README

- SQLite is used to minimize setup requirements.
- No authentication is included in the core submission.
- A task must belong to an existing category.
- Category deletion is not exposed because the brief only requires category listing and creation.
- Dates are stored and returned in a consistent UTC or date-only format, with the choice documented.
- The default page size is 10 and the maximum page size is bounded.
- The application is designed for a single local user in the take-home version.
- AI tools are used as an implementation and review aid; all submitted code is reviewed and understood by the author.

## 15. Follow-up call preparation

The implementation should make it possible to explain:

1. What happens from clicking Save to the database row being created.
2. Why controllers were chosen over Minimal APIs.
3. The difference between PUT and PATCH, and why creation returns 201.
4. What `AddScoped` means and why `DbContext` is scoped.
5. Where authorization would be added in a future version.
6. The difference between Vue `ref`, `reactive`, and `computed`.
7. How a future AI assistant would validate and handle malformed JSON.
8. How to add a `Tags` field end to end: migration, DTOs, service, API, and form.

