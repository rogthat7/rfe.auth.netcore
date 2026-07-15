# RFE Auth Netcore Service

This project is a modern, high-performance C# .NET 10.0 Web API authentication service backed by a PostgreSQL database. It is designed to run in a containerized environment using Docker Compose.

---

## Technical Stack & Architecture

- **Backend Framework**: .NET 10.0 Web API (running inside the official Linux `.NET 10.0 ASP.NET` base container image).
- **Database**: PostgreSQL (Docker container service `db`).
- **Data Access Layer**: 
  - **Entity Framework Core 10.0**: Used for DB context mapping, database schema generation, and startup migrations.
  - **Dapper**: High-performance micro-ORM used in the repositories (`UserRepository`, `AuthRepository`) for executing fast database-agnostic ANSI-SQL queries.
- **OAuth 2.1 & OpenID Connect**: **OpenIddict** is used to configure our API as a secure Authorization Server.
- **Federated Login**: **Google Authentication** is integrated into the sign-in flow.
- **API Documentation**: **Scalar API Reference** integrated directly at `http://localhost/scalar/v1`.

---

## How to Run the Project

### Prerequisites
- Install [Docker Desktop](https://docs.docker.com/desktop/windows/install/).

### Execution Steps
1. Navigate to the root directory containing `docker-compose.yml`.
2. Build and start the container stack:
   ```bash
   docker compose up -d --build
   ```
3. Docker Compose will start two services:
   - **`db`** (PostgreSQL) mapped to host port `5432`.
   - **`rfe-auth-api`** (.NET 10 Web API) mapped to host port `80` (routing internally to port `8080`).

---

## API Documentation (Scalar)

The project includes integrated **Scalar API Documentation** for interactive testing. 
Once the containers are running, open your web browser and navigate to:
👉 **`http://localhost/scalar/v1`**

---

## Walkthrough: .NET 10 & Postgres Modernization

This project was recently modernized from an older legacy stack:
1. **Framework Upgrade**: Upgraded from .NET Core 3.1 to **.NET 10.0**. All projects now target `net10.0` and utilize updated libraries.
2. **Database Provider Migration**: Swapped the SQL Server provider for **PostgreSQL**.
   - Removed all `System.Data.SqlClient` and `Microsoft.EntityFrameworkCore.SqlServer` dependencies.
   - Integrated `Npgsql` and `Npgsql.EntityFrameworkCore.PostgreSQL`.
   - Updated `UnitOfWork.cs` to construct `NpgsqlConnection` instead of `SqlConnection`.
   - Switched from SQL Server-specific stored procedures (`AUTH.spr_*`) to database-agnostic standard ANSI-SQL queries mapped directly in Dapper repositories.
   - Rebuilt Entity Framework Core migrations specifically for PostgreSQL (`RFE.Auth.API/Migrations`).
4. **OAuth 2.1 & Google Federated Authentication**:
   - Integrated **OpenIddict** as the OAuth 2.1 framework.
   - Overrode `OnModelCreating` in `UserContext` to call `modelBuilder.UseOpenIddict()`, and generated database migrations specifically for the OpenIddict schema (`AddOpenIddict` migration).
   - Configured Cookie, Google, and OpenIddict server/validation middlewares inside `Startup.cs`.
   - Created `AuthorizationController` containing standard endpoints for `/connect/authorize` (including a custom user consent screen) and `/connect/token` (verifying PKCE verifiers).
   - Created `GoogleAuthController` to handle the Google challenge and callback redirects.
   - Seeded a default external client (`mock-external-app` / `mock-client-secret`) during database initialization.
5. **Startup Refactoring & Modernization**:
   * Created `ServiceCollectionExtensions` to package services, repositories, DB contexts, option configurations, OpenIddict, Google Auth, and Swagger/Scalar setups into cleanly chained, fluent extension methods.
   * Cleaned up `Startup.cs` to call these extensions, reducing file complexity from ~300 lines to a highly readable and modular format.
   * Fixed legacy routing calls (`app.UseMvc()`) to use modern endpoint routing, enabling the Scalar documentation mapping to be correctly resolved.
