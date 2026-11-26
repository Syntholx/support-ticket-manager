# Support Ticket Manager

A full-stack help desk application built with React, TypeScript, ASP.NET Core and SQL Server. Users submit and track their own tickets. Support staff can triage the shared queue and manage tickets through resolution.

[Open the live demo](https://tsm.szymon-michalek.dev/)

The demo homepage lists shared sign-in credentials for both roles. Use fictional data only: demo tickets are periodically reset, and registration and account recovery are disabled in the public demo. See the [demo guide](docs/DEMO.md).

## What to try

- **User:** create a ticket, inspect its status and priority, close it, and find it in the archive.
- **Support:** view tickets across accounts, start work, change priority, close or reopen tickets.
- **Both:** browse paginated active and archived lists, search titles and change sorting. Search and client-side sorting apply to the currently loaded page.

The API enforces ticket ownership and role permissions. It derives the owner from the signed-in account, not from submitted JSON. Tickets move from `Open` to `InProgress` or `Closed`; Support can reopen closed tickets.

## Architecture

- `frontend/` — React, TypeScript, React Router, Tailwind CSS and Vite.
- `src/SupportTicketManager.Core/` — ticket model and domain rules.
- `src/SupportTicketManager.Api/` — ASP.NET Core Minimal API, Identity, EF Core migrations and SQL Server persistence.
- `tests/SupportTicketManager.Tests/` — xUnit tests for domain rules, API flows, ownership and database behavior.

The backend uses session cookies, CSRF protection, rate limits and optimistic concurrency. The public demo runs on a VPS with Docker Compose and Caddy. This is a demonstration, not a production security audit.

## Run locally

The full local version supports registration, email confirmation and password recovery. Requirements: **.NET 10 SDK**, **Node.js 24**, **npm**, a local **SQL Server** on port 1433, and local SMTP (such as Mailpit) for account emails. The existing Vite HTTPS development setup is Windows-specific.

From the repository root in PowerShell:

```powershell
dotnet restore
dotnet tool restore
dotnet dev-certs https --trust
```

Store your local SQL connection string in .NET User Secrets; the credentials below are placeholders. Never point the development or test setup at production data.

```powershell
dotnet user-secrets set "ConnectionStrings:TicketDatabase" 'Server=localhost,1433;Database=SupportTicketManager;User Id=YOUR_LOCAL_SQL_LOGIN;Password=YOUR_LOCAL_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True' --project src/SupportTicketManager.Api
dotnet user-secrets set "Email:Host" "127.0.0.1" --project src/SupportTicketManager.Api
dotnet user-secrets set "Email:Port" "1025" --project src/SupportTicketManager.Api
dotnet user-secrets set "Email:From" "tsm@example.com" --project src/SupportTicketManager.Api
dotnet user-secrets set "Email:FrontendBaseUrl" "https://localhost:5500" --project src/SupportTicketManager.Api
dotnet ef database update --project src/SupportTicketManager.Api --startup-project src/SupportTicketManager.Api -- --environment Development
dotnet run --project src/SupportTicketManager.Api --launch-profile https
```

The SMTP example assumes Mailpit is already running locally on SMTP port 1025 and web port 8025. The API listens at `https://localhost:7280`.

For Vite, place a trusted localhost PEM certificate and its private key at `%LOCALAPPDATA%\TSMDevCert\localhost.pem` and `%LOCALAPPDATA%\TSMDevCert\localhost.key`. Trusting the .NET certificate does not create those files. Keep the key out of Git. In a second terminal:

```powershell
cd frontend
npm.cmd ci
npm.cmd run dev
```

Open `https://localhost:5500`. Register a test account, open its confirmation email in Mailpit, then sign in. For a separate confirmed local account, stop the API and run the development-only Support grant command:

```powershell
dotnet run --project src/SupportTicketManager.Api --launch-profile https -- --grant-support support@example.com
```

Restart the API and sign in again to pick up the role. The command only works against local SQL Server in Development.

## Checks and documentation

From the repository root:

```powershell
dotnet test SupportTicketManager.slnx
cd frontend
npm.cmd test
npm.cmd run typecheck:tests
npx.cmd tsc --noEmit
npx.cmd vite build
```

SQL integration tests use unique temporary databases on local SQL Server. The test login must be able to create and drop those databases. Frontend tests use Vitest and Testing Library with mocked responses.

- [Public demo setup and limits](docs/DEMO.md)
- [API security and production configuration](docs/PRODUCTION.md)

## My contribution

I started this project while learning C# and .NET, then developed the React frontend as part of a guided frontend course. I implemented frontend flows and tests with AI-assisted examples and review. The backend, test suite and deployment documentation also received AI assistance.
