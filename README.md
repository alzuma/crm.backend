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

The project is organised in three layers plus shared contracts. The dependency
direction is fixed:

```
Endpoints  ->  Application  ->  Persistence
    |              |
    +--- Dtos, Validation ---+
```

| Folder | Role |
| --- | --- |
| `Endpoints/` | HTTP request handling. Binds and validates requests, calls the application layer, maps results to HTTP. Never touches persistence. One file per resource. |
| `Application/` | Business behaviour and service contracts. Depends on the persistence layer, never on endpoint types. One file per service. |
| `Persistence/` | Data storage contracts and implementations. SQLite/EF Core lands here in CRM-7; today an in-memory stand-in proves the flow. |
| `Dtos/` | Data crossing the API boundary. Data annotations on request DTOs declare the validation rules. |
| `Validation/` | Where incoming data is validated: the `DtoValidator` that runs the annotations and the custom validation rules beyond the built-in ones. |

The `sample` resource (`POST` / `GET /api/v1/samples`) and `GET /health/live`
are the visible example of the whole flow; later CRM work replaces and builds
on these folders.
