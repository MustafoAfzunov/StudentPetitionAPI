# Student Petition API

ASP.NET Core 8 REST API for student petitions (SQLite, JWT, layered architecture).

## Run

```bash
dotnet run --project StudentPetitionAPI
```

Swagger (Development): `/swagger`

## Demo users (Development only)

| Email | Password | Role |
|---|---|---|
| `student@test.com` | `Student123!` | Student |
| `reviewer@test.com` | `Reviewer123!` | Reviewer |

## Notes

- JWT signing key lives in configuration — use User Secrets / environment variables outside local demos.
- SQLite migrations apply automatically on startup.
