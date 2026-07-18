# Student Petition API

ASP.NET Core 8 REST API for submitting and reviewing student petitions (course retake, academic leave, major change, and other requests).

Built with a layered architecture (Domain / Application / Infrastructure), Entity Framework Core (SQLite), JWT authentication, FluentValidation, AutoMapper, Serilog, and Swagger.

---

## Table of contents

1. [Features](#features)
2. [Tech stack](#tech-stack)
3. [Project structure](#project-structure)
4. [Prerequisites](#prerequisites)
5. [How to run](#how-to-run)
6. [Demo users](#demo-users)
7. [Authentication](#authentication)
8. [Roles and permissions](#roles-and-permissions)
9. [Business rules](#business-rules)
10. [API endpoints](#api-endpoints)
11. [How to test with Swagger UI](#how-to-test-with-swagger-ui)
12. [How to test with curl](#how-to-test-with-curl)
13. [Enums reference](#enums-reference)
14. [Common errors](#common-errors)
15. [Troubleshooting](#troubleshooting)

---

## Features

- Create and manage student profiles
- Create petitions in **Draft** status
- Update petitions only while in Draft
- Submit petitions for review (`Draft` → `Submitted`)
- Review petitions (`Submitted` → `UnderReview` → `Approved` / `Rejected`)
- Filter and paginate petitions
- Paginate students
- JWT auth with role-based policies (Student / Reviewer)
- Students can only access **their own** petitions (matched by email)
- Global exception handling (`ProblemDetails`)
- FluentValidation on request bodies
- Serilog logging (console + rolling file under `logs/`)
- Swagger UI with JWT Authorize button
- SQLite database with automatic EF Core migrations on startup
- Dates in API responses formatted as `MM/DD/YYYY`

---

## Tech stack

| Technology | Purpose |
|---|---|
| ASP.NET Core 8 | Web API host |
| Entity Framework Core 8 | ORM (Code First) |
| SQLite | Database (`StudentPetitions.db`) |
| JWT Bearer | Authentication |
| FluentValidation | Request validation |
| AutoMapper | Entity ↔ DTO mapping |
| Serilog | Structured logging |
| Swashbuckle | Swagger / OpenAPI |

---

## Project structure

```text
StudentPetitionAPI/
├── Controllers/                 # HTTP endpoints (thin)
├── Domain/
│   ├── Entities/                # Student, Petition
│   └── Enums/                   # PetitionType, PetitionStatus
├── Application/
│   ├── DTOs/Requests|Responses  # API contracts
│   ├── Interfaces/              # Service + repository contracts
│   ├── Services/                # Business logic
│   ├── Validators/              # FluentValidation
│   ├── Mappings/                # AutoMapper profiles
│   └── Exceptions/              # Domain/application exceptions
├── Infrastructure/
│   ├── Data/                    # DbContext, Fluent configs, Migrations
│   ├── Repositories/            # EF Core data access
│   ├── Authentication/          # JWT login, roles, current user
│   └── Serialization/           # Date format converters
├── Middleware/                  # Global exception handling
├── Extensions/                  # DI + pipeline helpers
├── Swagger/                     # Enum/examples filters
├── Program.cs
└── appsettings.json
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or newer SDK that can target `net8.0`)
- Optional: `dotnet-ef` tool if you want to manage migrations manually:

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

---

## How to run

### 1. Clone / open the solution

```bash
cd ~/TestTask
```

### 2. Restore and build

```bash
dotnet restore StudentPetitionAPI.sln
dotnet build StudentPetitionAPI.sln
```

### 3. Run the API

```bash
cd StudentPetitionAPI
dotnet run
```

Or from the repo root:

```bash
dotnet run --project StudentPetitionAPI --launch-profile http
```

### 4. Open Swagger

When the console shows:

```text
Now listening on: http://localhost:5192
```

open:

**http://localhost:5192/swagger**

> Keep this terminal open. If you press `Ctrl+C`, the API stops and Swagger will show **Failed to fetch**.

### 5. Database

- Connection string: `Data Source=StudentPetitions.db` (created in the project working directory)
- Migrations apply automatically on startup (`Database.Migrate()`)
- Manual update (optional):

```bash
cd StudentPetitionAPI
dotnet ef database update
```

### 6. Logs

- Console output while running
- Rolling files: `StudentPetitionAPI/logs/student-petition-YYYYMMDD.log`

---

## Demo users

Hardcoded for local / Development testing only:

| Email | Password | Role | What they can do |
|---|---|---|---|
| `student@test.com` | `Student123!` | **Student** | Create student profile, create/update/submit **own** petitions |
| `reviewer@test.com` | `Reviewer123!` | **Reviewer** | View students/petitions, **review** petitions; cannot create students |

---

## Authentication

1. Call `POST /api/auth/login` with email + password
2. Copy `accessToken` from the response
3. Send it on protected requests:

```http
Authorization: Bearer <accessToken>
```

In Swagger:
1. Execute login
2. Click **Authorize** (top right)
3. Paste the token (Swagger uses Bearer automatically)
4. Click **Authorize** → **Close**

Token lifetime: **60 minutes** (see `Jwt:ExpirationMinutes` in `appsettings.json`).

**Important ownership rule:** a Student JWT is tied to email `student@test.com`. When creating petitions, `studentId` must belong to a student profile whose **email matches** that JWT email. Otherwise you get **403 Forbidden**.

---

## Roles and permissions

| Endpoint | Student | Reviewer | Anonymous |
|---|---|---|---|
| `POST /api/auth/login` | ✅ | ✅ | ✅ |
| `POST /api/students` | ✅ | ❌ 403 | ❌ 401 |
| `GET /api/students` | ✅ | ✅ | ❌ 401 |
| `GET /api/students/{id}` | ✅ | ✅ | ❌ 401 |
| `POST /api/petitions` | ✅ (own profile only) | ❌ 403 | ❌ 401 |
| `GET /api/petitions` | ✅ (own only) | ✅ (all) | ❌ 401 |
| `GET /api/petitions/{id}` | ✅ (own only) | ✅ | ❌ 401 |
| `PUT /api/petitions/{id}` | ✅ (own + Draft) | ❌ 403 | ❌ 401 |
| `POST /api/petitions/{id}/submit` | ✅ (own + Draft) | ❌ 403 | ❌ 401 |
| `POST /api/petitions/{id}/review` | ❌ 403 | ✅ | ❌ 401 |

---

## Business rules

### Petition status flow

```text
Draft ──submit──► Submitted ──(auto)──► UnderReview ──review──► Approved
                                                      └──review──► Rejected
```

| Action | Allowed from | Result |
|---|---|---|
| Create petition | — | Always starts as **Draft** |
| Update petition | **Draft** only | Stays Draft |
| Submit | **Draft** only | **Submitted** |
| Review | **Submitted** or **UnderReview** | Auto-moves Submitted → UnderReview, then **Approved** or **Rejected** |

### Review requirements

- `status` must be `Approved` or `Rejected`
- `reviewComment` is required
- `reviewedBy` is required
- `reviewedAt` and `updatedAt` are set by the server

### Uniqueness

- Student `Email` must be unique
- Student `StudentNumber` must be unique

---

## API endpoints

Base URL (local HTTP profile): `http://localhost:5192`

### Auth

#### `POST /api/auth/login`

Login and receive a JWT.

**Auth:** none  

**Request body:**

```json
{
  "email": "student@test.com",
  "password": "Student123!"
}
```

**Success `200`:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresInMinutes": 60,
  "email": "student@test.com",
  "role": "Student"
}
```

**Errors:** `400` validation, `401` invalid credentials

---

### Students

#### `POST /api/students`

Create a student profile.

**Auth:** Bearer, role **Student**  

**Request body:**

```json
{
  "firstName": "Ada",
  "lastName": "Lovelace",
  "email": "student@test.com",
  "studentNumber": "S-10001"
}
```

**Success `201`:** returns `StudentResponse` (includes `id`, names, email, studentNumber, createdAt)

**Errors:** `400`, `403` (Reviewer), `409` (duplicate email/number)

---

#### `GET /api/students/{id}`

Get one student by id.

**Auth:** Bearer, Student or Reviewer  

**Success `200`:** student object  

**Errors:** `404`

---

#### `GET /api/students`

Paged list of students.

**Auth:** Bearer, Student or Reviewer  

**Query parameters:**

| Name | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number (1-based) |
| `pageSize` | int | 10 | Page size (max 100) |

**Example:** `GET /api/students?page=1&pageSize=10`

**Success `200`:**

```json
{
  "items": [ /* StudentResponse */ ],
  "totalCount": 2,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

---

### Petitions

#### `POST /api/petitions`

Create a petition (status = **Draft**).

**Auth:** Bearer, role **Student**  

**Request body:**

```json
{
  "studentId": 1,
  "petitionType": "CourseRetake",
  "title": "Retake Calculus I",
  "description": "I want to retake the course next semester."
}
```

`petitionType` values: `CourseRetake`, `AcademicLeave`, `MajorChange`, `Other`

**Success `201`:** `PetitionResponse` with `status: "Draft"`

**Errors:** `400`, `403` (wrong student / Reviewer), `404` (student not found)

---

#### `GET /api/petitions/{id}`

Get one petition.

**Auth:** Bearer  
- Student: only if petition belongs to their email  
- Reviewer: any petition  

**Success `200`**  
**Errors:** `403`, `404`

---

#### `GET /api/petitions`

Filtered, paged petition list.

**Auth:** Bearer  
- Student: automatically scoped to own petitions  
- Reviewer: can see all (and filter)

**Query parameters:**

| Name | Type | Description |
|---|---|---|
| `status` | enum | `Draft`, `Submitted`, `UnderReview`, `Approved`, `Rejected` |
| `petitionType` | enum | `CourseRetake`, `AcademicLeave`, `MajorChange`, `Other` |
| `studentId` | int | Filter by student (Reviewer; Student cannot query others) |
| `dateFrom` | datetime | CreatedAt >= |
| `dateTo` | datetime | CreatedAt <= |
| `page` | int | Default 1 |
| `pageSize` | int | Default 10 (max 100) |

**Example:**

```text
GET /api/petitions?status=Submitted&petitionType=CourseRetake&page=1&pageSize=10
```

**Success `200`:** paged `PetitionResponse` list

---

#### `PUT /api/petitions/{id}`

Update petition content. **Only while Draft.**

**Auth:** Bearer, role **Student** (owner only)

**Request body:**

```json
{
  "petitionType": "AcademicLeave",
  "title": "Request academic leave",
  "description": "Updated description"
}
```

**Success `200`**  
**Errors:** `400` (not Draft / validation), `403`, `404`

---

#### `POST /api/petitions/{id}/submit`

Submit a draft petition for review.

**Auth:** Bearer, role **Student** (owner only)

**Body:** none

**Success `200`:** status becomes `Submitted`  
**Errors:** `400` (not Draft), `403`, `404`

---

#### `POST /api/petitions/{id}/review`

Review a submitted petition.

**Auth:** Bearer, role **Reviewer**

**Request body:**

```json
{
  "status": "Approved",
  "reviewedBy": "reviewer@test.com",
  "reviewComment": "Documents verified. Approved."
}
```

Or reject:

```json
{
  "status": "Rejected",
  "reviewedBy": "reviewer@test.com",
  "reviewComment": "Missing supporting documents."
}
```

**Success `200`:** status becomes `Approved` or `Rejected`  
**Errors:** `400` (invalid status / missing comment / wrong current status), `403`, `404`

---

## How to test with Swagger UI

### Full happy path

1. **Start the API** (`dotnet run`) and open http://localhost:5192/swagger  
2. **Login as Student**  
   - `POST /api/auth/login` with `student@test.com` / `Student123!`  
   - Copy `accessToken`  
   - Click **Authorize** → paste token → Authorize  
3. **Create student profile**  
   - `POST /api/students` with email `student@test.com`  
   - Note returned `id` (e.g. `1`)  
   - If you get `409`, the student already exists — use `GET /api/students` to find the id for `student@test.com`  
4. **Create petition**  
   - `POST /api/petitions` with that `studentId`  
   - Expect `status: Draft`  
5. **Update petition**  
   - `PUT /api/petitions/{id}` while still Draft  
6. **Submit petition**  
   - `POST /api/petitions/{id}/submit`  
   - Expect `status: Submitted`  
7. **Switch to Reviewer**  
   - Authorize → Logout  
   - Login as `reviewer@test.com` / `Reviewer123!`  
   - Authorize with new token  
8. **Review petition**  
   - `POST /api/petitions/{id}/review` with Approved/Rejected + comment  
   - Expect final status Approved or Rejected  

### Requirement checks in Swagger

| Requirement | Steps | Expected |
|---|---|---|
| Draft-only edit | Submit, then `PUT` again | 400 invalid transition |
| Draft-only submit | Submit twice | second call fails |
| Review comment required | Review without `reviewComment` | 400 |
| Reviewer cannot create students | Login as Reviewer → `POST /api/students` | 403 |
| Student cannot review | Login as Student → `POST .../review` | 403 |
| Unique email/number | Create student twice same email | 409 |
| Student sees only own petitions | Create another student’s petition (as other email) | Student list scoped to self |
| Pagination | `GET /api/students?page=1&pageSize=1` | `totalPages` / `items` length |
| Filters | `GET /api/petitions?status=Approved` | only matching rows |

---

## How to test with curl

Replace `TOKEN` after login.

### Login (Student)

```bash
curl -s -X POST 'http://localhost:5192/api/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{"email":"student@test.com","password":"Student123!"}'
```

### Create student

```bash
curl -s -X POST 'http://localhost:5192/api/students' \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"firstName":"Ada","lastName":"Lovelace","email":"student@test.com","studentNumber":"S-10001"}'
```

### List students

```bash
curl -s 'http://localhost:5192/api/students?page=1&pageSize=10' \
  -H "Authorization: Bearer $TOKEN"
```

### Create petition

```bash
curl -s -X POST 'http://localhost:5192/api/petitions' \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"studentId":1,"petitionType":"CourseRetake","title":"Retake Calculus I","description":"I want to retake the course next semester."}'
```

### Submit petition

```bash
curl -s -X POST "http://localhost:5192/api/petitions/1/submit" \
  -H "Authorization: Bearer $TOKEN"
```

### Login (Reviewer) + review

```bash
REVIEWER_TOKEN=$(curl -s -X POST 'http://localhost:5192/api/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{"email":"reviewer@test.com","password":"Reviewer123!"}' \
  | python3 -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

curl -s -X POST 'http://localhost:5192/api/petitions/1/review' \
  -H "Authorization: Bearer $REVIEWER_TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"status":"Approved","reviewedBy":"reviewer@test.com","reviewComment":"Documents verified. Approved."}'
```

### Filter petitions

```bash
curl -s 'http://localhost:5192/api/petitions?status=Approved&page=1&pageSize=10' \
  -H "Authorization: Bearer $REVIEWER_TOKEN"
```

---

## Enums reference

### PetitionType

| Value | Meaning |
|---|---|
| `CourseRetake` | Request to retake a course |
| `AcademicLeave` | Academic leave request |
| `MajorChange` | Change of major / specialty |
| `Other` | Other petition type |

### PetitionStatus

| Value | Meaning |
|---|---|
| `Draft` | Editable, not submitted |
| `Submitted` | Waiting to be taken into review |
| `UnderReview` | Taken into work |
| `Approved` | Accepted (terminal) |
| `Rejected` | Denied (terminal) |

Send enum values as **strings** in JSON (not numbers).

---

## Common errors

| HTTP | When |
|---|---|
| `400` | Validation failed, invalid status transition, bad review payload |
| `401` | Missing/invalid JWT, bad login password |
| `403` | Wrong role, or Student accessing another student’s petition |
| `404` | Student/petition id not found |
| `409` | Duplicate email or student number |
| Failed to fetch (Swagger) | API process stopped / wrong URL / not listening on 5192 |

Example error shape (`ProblemDetails`):

```json
{
  "type": "...",
  "title": "Forbidden",
  "status": 403,
  "detail": "Students can only access their own petitions.",
  "instance": "/api/petitions",
  "traceId": "..."
}
```

---

## Troubleshooting

### Swagger shows “Failed to fetch”

1. Check the run terminal still shows `Now listening on: http://localhost:5192`
2. If not, restart: `cd StudentPetitionAPI && dotnet run`
3. Use **http** (not https) for local profile: `http://localhost:5192/swagger`
4. Hard-refresh the browser tab

### 403 when creating a petition

- You are logged in as `student@test.com`
- `studentId` must be the profile whose email is also `student@test.com`
- Check with `GET /api/students` and pick the correct `id`

### 409 when creating a student

- Email or student number already exists
- Use `GET /api/students` and continue with the existing id, or choose a new `studentNumber`

### Port already in use

```bash
# find process on 5192
ss -ltnp | grep 5192
# or stop your previous dotnet run with Ctrl+C
```

### Reset local database

Stop the API, then:

```bash
cd StudentPetitionAPI
rm -f StudentPetitions.db
dotnet run
```

Migrations recreate the schema on startup.

---

## Configuration notes

Key settings in `StudentPetitionAPI/appsettings.json`:

- `ConnectionStrings:DefaultConnection` — SQLite file path
- `Jwt:Issuer` / `Audience` / `SecretKey` / `ExpirationMinutes`
- Serilog sinks (console + `logs/`)

For anything beyond a local demo, move the JWT secret to User Secrets or environment variables and do not commit production secrets.
