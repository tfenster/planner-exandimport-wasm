A little tool to get information about your Planner plans and also duplicate plans with adjustments.

## Architecture

- **backend** – an ASP.NET Core (minimal API) service on .NET 10 that talks to the MS Graph API to ex- and import Planner groups, plans, tasks and details.
- **frontend** – a Blazor Server app on .NET 10 (using AntDesign) that authenticates the user via Azure AD and calls the backend.
- **shared** – the JSON models and the `IPlanner` interface shared by both components.

> Note: this project originally ran the backend as a WebAssembly component on [Fermyon Spin](https://www.fermyon.com/spin). It has since been migrated to a plain .NET service (no Spin, no WASM). The original approach is described in this blog post: https://tobiasfenster.io/net-in-webassembly-with-fermyon-spin-or-how-to-duplicate-your-planner-plans-with-adjustments

## Building and running

Both components target **.NET 10**.

```bash
# backend
dotnet run --project backend/Project.csproj

# frontend
dotnet run --project frontend/frontend.csproj
```

Container images are built from `Dockerfile.backend` / `Dockerfile.frontend`, and `docker-compose.yml` / `docker-compose.build.yml` run them together. The frontend reaches the backend via the `BackendBaseUrl` setting; Azure AD configuration (`AzureAd:ClientId`, `TenantId`, `ClientSecret`) is supplied through configuration / environment variables.
