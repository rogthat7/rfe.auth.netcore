# RFE Auth Netcore Service

This project is a modern, high-performance C# .NET 10.0 Web API authentication service backed by a PostgreSQL database. It is designed to run in a containerized environment using Docker Compose.

---

## Technical Stack & Architecture

- **Backend Framework**: .NET 10.0 Web API (running inside the official Linux `.NET 10.0 ASP.NET` base container image).
- **Database**: PostgreSQL (Docker container service `db`).
- **Data Access Layer**: 
  - **Entity Framework Core 10.0**: Used for DB context mapping, database schema generation, and startup migrations.
  - **Dapper**: High-performance micro-ORM used in the repositories (`UserRepository`, `AuthRepository`) for executing fast database-agnostic ANSI-SQL queries.
- **API Documentation**: **Scalar API Reference** integrated directly at `http://localhost/scalar`.

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
👉 **`http://localhost/scalar`**

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
3. **Docker Configurations**:
   - Replaced MS SQL Server docker container with PostgreSQL alpine container.
   - Replaced Windows-style backslashes in `.dockerignore` with forward slashes for Linux Docker compatibility.
   - Updated port mapping to target the new .NET 10.0 container default port (`8080`).
