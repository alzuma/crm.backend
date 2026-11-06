# Simple CRM - backend

ASP.NET Core Web API (C#) for the Simple CRM system.

## Commands

| Command | What it does |
| --- | --- |
| `dotnet build` | Build the solution |
| `dotnet run --project src/Crm.Backend` | Start the API (default http://localhost:5080) |
| `dotnet test` | Run the unit and API integration tests |
| `dotnet watch --project src/Crm.Backend` | Start with reload on change |

OpenAPI document is served at `/openapi/v1.json` (Swagger UI in a later ticket)
when running in the Development environment.

## Layout

The project is organised in layers. The dependency direction is fixed:

```
Endpoints  ->  Application  ->  Persistence
    |              |
    +--- Dtos, Validation ---+
```

| Folder | Role |
| --- | --- |
| `Endpoints/` | HTTP request handling. Binds and validates requests, calls the application layer, maps results to HTTP. Never touches persistence. One file per resource. |
| `Application/` | Business behaviour and service contracts, added with the first CRM features (CRM-3 and later). Depends on the persistence layer, never on endpoint types. One file per service. |
| `Persistence/` | The SQLite database via EF Core: `CrmDbContext`, one configuration class per entity under `Configurations/`, the six record entities under `Entities/`, and the migrations under `Migrations/`. |
| `Dtos/` | Data crossing the API boundary. Data annotations on request DTOs declare the validation rules. |
| `Validation/` | Where incoming data is validated: the `DtoValidator` that runs the annotations and the custom validation rules beyond the built-in ones. |

## Database

The schema is applied by EF Core migrations (see `Persistence/Migrations/`).
Development applies them with:

```
dotnet ef database update --project src/Crm.Backend --startup-project src/Crm.Backend
```

The connection string is read from configuration, key `ConnectionStrings:Default`
(a `Data Source=<file>` SQLite connection string). The dev default in
`appsettings.json` points at `crm.db`; override it per environment with the
`ConnectionStrings__Default` environment variable. The database file is
development data and is never committed.

The database enforces the data model rules: UUID TEXT primary keys, UTC dates,
amounts as integer minor units with a nonnegative check, probability bounded
0-100, nonempty company names and opportunity titles, a unique index on the
normalized (upper-cased, trimmed) user email, foreign key indexes on all
reference columns, and deletes blocked while dependent records still exist.
