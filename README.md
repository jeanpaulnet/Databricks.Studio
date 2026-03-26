# Databricks.Studio

Full-stack analytics management platform — ASP.NET Core 10 Web API + Angular 19 frontend.

---

## Solution Structure

```
Databricks.Studio/
├── src/
│   ├── Databricks.Studio.API          # ASP.NET Core Web API (.NET 10)
│   ├── Databricks.Studio.Core         # EF Core DbContext + Entities
│   ├── Databricks.Studio.Managers     # Business logic layer
│   └── Databricks.Studio.Shared       # DTOs, Constants, Response wrappers
├── tests/
│   ├── Databricks.Studio.UnitTests    # xUnit + FluentAssertions (Managers)
│   └── Databricks.Studio.LoadTests    # NBomber load tests (API endpoints)
├── client/
│   └── databricks-studio-ui/          # Angular 19 standalone app
└── database/
    ├── 01_create_tables.sql
    ├── 02_seed_data.sql
    └── 03_views_and_procs.sql
```

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.0+ |
| SQL Server | 2019+ or Azure SQL |
| Node.js | 20+ |
| Angular CLI | 19+ |

---

## Database Setup

```sql
-- Run in order against your SQL Server instance
-- 1. Create tables
sqlcmd -S localhost -i database/01_create_tables.sql

-- 2. Seed test data
sqlcmd -S localhost -d DatabricksStudio -i database/02_seed_data.sql

-- 3. Views and stored procedures
sqlcmd -S localhost -d DatabricksStudio -i database/03_views_and_procs.sql
```

---

## Backend Setup

### 1. Configure connection string

Edit `src/Databricks.Studio.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DatabricksStudio;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Auth": {
    "Authority": "https://accounts.google.com",
    "Audience": "YOUR_GOOGLE_CLIENT_ID"
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=YOUR_KEY;..."
  }
}
```

### 2. Apply EF Core migrations (optional — SQL scripts are provided above)

```bash
cd src/Databricks.Studio.API
dotnet ef migrations add InitialCreate --project ../Databricks.Studio.Core
dotnet ef database update
```

### 3. Run the API

```bash
cd src/Databricks.Studio.API
dotnet run
# Swagger UI → https://localhost:7001/swagger
```

---

## Frontend Setup

### 1. Configure environment

Edit `client/databricks-studio-ui/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7001',
  google: {
    clientId: 'YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com',
    redirectUri: 'http://localhost:4200',
    scope: 'openid profile email',
    issuer: 'https://accounts.google.com'
  }
};
```

### 2. Install dependencies and run

```bash
cd client/databricks-studio-ui
npm install
ng serve
# App → http://localhost:4200
```

---

## Testing

### Unit Tests

```bash
cd tests/Databricks.Studio.UnitTests
dotnet test --logger "console;verbosity=normal"
```

### Load Tests

```bash
cd tests/Databricks.Studio.LoadTests

# Run all scenarios
dotnet run

# Run only analytics scenarios
dotnet run -- analytics

# Run only run scenarios
dotnet run -- runs
```

HTML reports are saved to `tests/Databricks.Studio.LoadTests/load-test-results/`.

---

## API Endpoints

### AnalyticsManageController — `/api/analytics`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/analytics` | List analytics (paged) |
| GET | `/api/analytics/{id}` | Get by ID |
| POST | `/api/analytics` | Create |
| PUT | `/api/analytics/{id}` | Update |
| DELETE | `/api/analytics/{id}` | Delete |

### AnalyticsRunController — `/api/analytics/{analyticsId}/runs`

| Method | Path | Description |
|--------|------|-------------|
| POST | `.../runs/start` | Start a run |
| POST | `.../runs/{runId}/stop` | Stop a run |
| GET | `.../runs/{runId}` | Get run by ID |
| GET | `.../runs/history` | Run history |

### AnalyticsReviewController — `/api/analytics/review` *(Reviewer role required)*

| Method | Path | Description |
|--------|------|-------------|
| POST | `.../review/{id}/approve` | Approve |
| POST | `.../review/{id}/reject` | Reject |

---

## Logging

| Environment | Sink |
|-------------|------|
| Development | Console + Debug (via `appsettings.Development.json`) |
| Production | Azure Application Insights |

Switch between environments using `ASPNETCORE_ENVIRONMENT`.

---

## Google SSO Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create an OAuth 2.0 Client ID (Web Application)
3. Add `http://localhost:4200` as an Authorized JavaScript origin
4. Add `http://localhost:4200` as an Authorized redirect URI
5. Copy the Client ID to both `appsettings.json` (API) and `environment.ts` (Angular)

---

## Entities & Status Enumerations

### AnalyticsStatus
`0` Draft → `1` Submitted → `2` Approved / `3` Rejected → `4` Published

### AnalyticsRunStatus
`0` Queued → `1` Started → `2` Completed / `3` Terminated
