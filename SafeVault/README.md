# SafeVault

A small ASP.NET Core Web API for storing sensitive "vault items" per user, built with security as the main focus for this project.

## What's in here

- **ASP.NET Identity** for user accounts, password hashing, and roles (Admin / User)
- **JWT authentication** — login returns a signed token, all vault endpoints require it
- **Role-based access control (RBAC)** — regular users can only see/edit their own items, only Admins can delete
- **Input validation** — blocks common SQL injection patterns and username junk before it hits the database
- **SQL injection prevention** — all queries go through EF Core with parameterized lambda expressions, no raw SQL string building anywhere
- **XSS prevention** — output is HTML-encoded before being returned
- **Tests** — `Tests/InputValidatorTests.cs` covers the validation logic directly; `Tests/ManualSecurityTestNotes.cs` documents the manual API-level checks done through Swagger during the debugging pass

## Running it

```bash
dotnet restore
dotnet run
```

Open `/swagger` to try it out. Register a user at `/api/auth/register`, log in at `/api/auth/login` to get a token, then use that token (Bearer scheme) to hit `/api/vaultitems`.

Before this goes anywhere near production: replace the `Jwt:Key` in `appsettings.json` with an actual random secret (an environment variable, not a committed file), and swap the in-memory database for a real one.

## Running the tests

```bash
cd Tests
dotnet test
```

## Vulnerabilities found & fixed

- **SQL injection**: an early version of the items endpoint built queries by concatenating the title string directly. Fixed by switching entirely to EF Core LINQ queries (parameterized by default) and adding an input check that rejects strings containing typical injection patterns as a second layer.
- **XSS**: item content was being returned to the client as-is, so a stored `<script>` tag would execute if rendered in a browser. Fixed by HTML-encoding output before it's sent back.
- **Missing ownership check**: the first version of `GetOne`/`Update` let any logged-in user fetch or edit any item by ID, not just their own. Added an ownership check (`item.OwnerId != CurrentUserId`) with an Admin override.
- **No role restriction on delete**: originally any authenticated user could delete any item. Locked that endpoint down to the Admin role only.
